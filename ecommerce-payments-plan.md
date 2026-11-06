# Ecommerce payments: PayPal v2 and Stripe

## Purpose and scope

This is the repository-specific implementation plan for adding PayPal v2 and Stripe while retaining the current PayPal v1 integration for backwards compatibility.

The editable ecommerce frontend is:

src/Ecommerce/HypnoTech.Ecommerce.Frontend.Frontend/js/checkout.js

That source is authoritative for the generated browser bundle. Do not edit generated wwwroot assets directly.

The current flow is PayPal-specific:

1. checkout.js posts the browser cart and address to POST /Checkout/Pay.
2. CheckoutService.Pay recalculates the basket and calls IPayPalService.
3. PayPalService creates a legacy PayPal.v1 payment and returns a redirect URL.
4. The browser stores paypal_payment_id in a cookie and redirects to PayPal.
5. /Checkout/Complete executes the payment, completes the order, allocates download codes, and sends email.

The target is an additive provider-neutral core with v1 retained as a compatibility adapter.

## Current repository findings

### Existing PayPal v1 implementation

- src/Ecommerce/HypnoTech.Ecommerce.Frontend.Frontend/js/checkout.js has the only payment button and calls payWithPaypal.
- src/Ecommerce/HypnoTech.Ecommerce.Frontend/Controllers/CheckoutController.cs owns Pay, Complete, and Cancelled.
- src/Ecommerce/HypnoTech.Ecommerce.Frontend/Services/CheckoutService.cs currently combines pricing, payment orchestration, order persistence, download-code allocation, and email.
- src/Ecommerce/HypnoTech.Ecommerce.Frontend/Services/PayPalService.cs uses the legacy PayPal package and PayPal.v1.
- PaypalCreateResponse, PaypalExecuteResponse, and PaymentCompletion contain PayPal-specific fields.
- OrderRepository finds and completes orders by PaymentId, assuming SaleId, PayerId, and SaleUrl are PayPal values.
- Config and the CMS settings page currently expose only one PaypalConfig.

### Hosting sites

- Esoteric is the only site currently hosting checkout (sites/Esoteric/Esoteric.Website/Views/Checkout).
- SelfHypnosis currently has no ecommerce checkout views or PayPal references. Shared engine changes should be reusable there later, without adding a second implementation now.

### Invariants to preserve

- The browser cart contains identifiers, not authoritative prices. The server must reload merchandise, options, prices, postage, and availability.
- CheckoutDetails is the source of truth for subtotal, tracked/special-delivery charge, postage, currency, and total.
- Payment success must come from a verified provider response or webhook, never from a browser redirect alone.
- Download-code allocation and customer/admin email must happen once even if a redirect, webhook, retry, or worker processes the same payment more than once.

## Target architecture

### Provider-neutral contracts

Add a payment domain layer instead of making CheckoutService understand three APIs.

Suggested concepts:

~~~csharp
public enum PaymentProvider
{
    PayPalV1,
    PayPalV2,
    Stripe
}

public enum PaymentStatus
{
    Created,
    Pending,
    Approved,
    Paid,
    Failed,
    Cancelled,
    Refunded
}

public sealed record PaymentCreateRequest(
    string InternalOrderId,
    CheckoutDetails Details,
    PostageAddress Address,
    Uri ReturnUrl,
    Uri CancelUrl);

public sealed record PaymentSession(
    PaymentProvider Provider,
    string InternalOrderId,
    string ProviderReference,
    Uri? RedirectUrl,
    PaymentStatus Status);

public sealed record PaymentCompletion(
    PaymentProvider Provider,
    string InternalOrderId,
    string ProviderReference,
    string? CaptureReference,
    string? SessionReference,
    string? PayerId,
    string? EmailAddress,
    PaymentStatus Status,
    DateTimeOffset? PaidAt);
~~~

Exact names can follow existing conventions; the important distinction is that a provider reference is not the internal order ID.

Add separate adapters:

- Keep IPayPalService and its current signatures as the v1 adapter.
- Add IPayPalV2Service for Orders v2 create/capture.
- Add IStripePaymentService for hosted Checkout Sessions and webhook/session mapping.
- Add IOrderPaymentService to build an immutable, server-priced payment request and delegate to the selected adapter.
- Add IOrderFulfillmentService to perform completion side effects exactly once.

Keep CheckoutService.Pay and CheckoutService.Complete as compatibility methods initially. They can delegate to the new layer while preserving v1 response types and routes.

### Stable order and payment-attempt IDs

Create an internal order ID before creating any provider payment. A retry or provider switch should create a separate payment attempt rather than overwrite provider identifiers on one ambiguous order.

Recommended PaymentAttempt fields:

- InternalOrderId
- Provider
- ProviderReference (PayPal order/capture ID or Stripe session/payment-intent ID)
- Status, Amount, and Currency
- Provider idempotency key
- Created/updated/paid timestamps
- Last processed webhook/event ID
- Failure or cancellation reason

## Data model and Mongo changes

### Additive Order fields

Add fields to src/Ecommerce/HypnoTech.Ecommerce.Models/Order.cs without removing or renaming existing PayPal fields:

- PaymentProvider (paypal-v1, paypal-v2, or stripe)
- InternalOrderId
- ProviderOrderId or ProviderReference
- ProviderCaptureId
- ProviderSessionId
- PaymentStatus
- PaymentEventId / last processed event ID
- PaidAt
- Currency
- CustomerEmail (retain EmailAddress as a v1-compatible alias where needed)

Continue populating PaymentId, PayerId, SaleId, and SaleUrl for v1. For new providers, populate neutral/provider-specific fields and only populate legacy fields where a useful compatibility mapping exists.

For old documents with no provider field, infer paypal-v1 when reading. Do not rewrite all historical orders in the first deployment.

### Repository API

Extend IOrderRepository and OrderRepository with operations such as:

- SavePendingOrder / SavePaymentAttempt
- FindByInternalOrderId
- FindByProviderReference
- TryCompleteOrder (idempotent state transition)
- MarkPaymentFailed and MarkPaymentCancelled
- HasProcessedPaymentEvent

Keep CompleteOrder(paymentId) and CancelOrder(paymentId) as v1 adapters.

Use unique Mongo indexes for internal order IDs, provider references, and processed event IDs where the schema permits. Duplicate completion must return the existing completed order and must not allocate another code or send another email.

Update src/Common/HypnoTech.Common.Dal.MongoDb/StartupBuilderExtensions.cs with BSON mappings. Confirm missing fields deserialize safely for existing records.

## PayPal v2 implementation

PayPal's current server flow is to create an Orders v2 order on the server, let the buyer approve it, then capture it on the server. See the [PayPal Orders v2 API](https://developer.paypal.com/api/orders/v2/) and [standard checkout integration](https://developer.paypal.com/platforms/checkout/standard/integrate).

### Package and client isolation

- Retain the existing PayPal package and PayPalService for v1.
- Add the official PayPalServerSDK package for the new adapter, pinning a reviewed version. Use the [official .NET SDK repository](https://github.com/paypal/PayPal-Dotnet-Server-SDK) and [NuGet package](https://www.nuget.org/packages/PayPalServerSDK/) as references; the NuGet page currently lists 2.4.0 at the time of this plan.
- Keep v1 and v2 namespaces in separate classes/files so a package upgrade cannot silently change v1 behavior.
- Prefer the SDK or a typed HttpClientFactory client; do not use the synchronous .Result pattern currently present in PayPalService.

### Server flow

1. POST /Checkout/Payments/paypal-v2/orders validates antiforgery and address, reloads CheckoutDetails, creates an internal order/payment attempt, and creates an Orders v2 order with intent CAPTURE.
2. Build the order using the configured currency, complete amount breakdown, line items, postage/special-delivery charge, and shipping address.
3. Include the internal order ID as custom_id or an equivalent provider reference so a webhook can reconcile without trusting browser state.
4. Send a unique PayPal-Request-Id for create and capture retries. PayPal documents OAuth client-credentials authentication in its [REST authentication guide](https://developer.paypal.com/api/rest/authentication/).
5. Return only a neutral session and PayPal order ID to the browser.
6. After buyer approval, POST /Checkout/Payments/paypal-v2/orders/{id}/capture captures on the server. Verify captured amount, currency, merchant account, and internal reference.
7. Map COMPLETED to Paid; map payer-action, instrument-declined, and other recoverable states to retryable responses; map permanent failures to Failed.
8. Call IOrderFulfillmentService only after verified capture, and make that call idempotent.

### Browser flow

Use the current PayPal JavaScript integration supported by PayPal documentation (validate the exact SDK version during implementation; current documentation points new integrations toward the current JS SDK/Orders v2 flow). The button's createOrder calls the server create endpoint, and its approval callback calls the server capture endpoint. The browser must not submit a client-calculated amount or treat approval itself as payment proof.

If a server redirect is deliberately chosen instead, use v2-specific return/cancel URLs and verify the order server-side before fulfillment.

### Webhooks and reconciliation

Add a PayPal webhook endpoint for capture completion, denial, refund, and dispute events that matter to the business. Verify each signature using the transmission fields, webhook ID, certificate/signature, and raw body; process each event ID once. See [PayPal webhooks](https://developer.paypal.com/api/rest/webhooks) and [PayPal security guidance](https://developer.paypal.com/security-guidelines).

The webhook is a reconciliation path, not a reason to complete twice. If capture response and webhook race, the idempotent repository transition chooses the winner.

## Stripe implementation

### Recommended integration shape

Use Stripe-hosted Checkout rather than handling card details in this application. A [Stripe Checkout Session](https://docs.stripe.com/api/checkout/sessions) is created by the server and sends the customer to the hosted page. This keeps card data out of the site and is lower-risk than introducing Elements in the first release.

Add the current reviewed Stripe.net package and use the official StripeClient/SessionService pattern documented in the [Stripe .NET SDK](https://github.com/stripe/stripe-dotnet).

### Server flow

1. POST /Checkout/Payments/stripe/session validates antiforgery and address, reloads authoritative CheckoutDetails, creates an internal order/payment attempt, and creates a Checkout Session with mode=payment.
2. Create line_items from server-calculated merchandise and the same postage/special-delivery breakdown used by PayPal. Never use the posted total.
3. Set client_reference_id to the internal order ID and bounded metadata containing the internal order ID/provider. See [Checkout Session creation](https://docs.stripe.com/api/checkout/sessions/create) and [metadata guidance](https://docs.stripe.com/api/metadata).
4. Set provider-specific success/cancel URLs and return the hosted session URL (or session ID).
5. Do not finalize an order from the success URL; it is only a customer-facing status page.
6. Process checkout.session.completed when the session is paid. If asynchronous methods are enabled, also handle async success/failure events.
7. Retrieve the PaymentIntent when needed for a durable reference, then call the shared idempotent fulfillment service.

Use a deterministic idempotency key derived from the internal payment-attempt ID. See [Stripe idempotent requests](https://docs.stripe.com/api/idempotent_requests).

### Webhook endpoint

Add POST /webhooks/stripe (or an equivalent dedicated endpoint) without MVC antiforgery validation. Read the raw body, verify Stripe-Signature with the endpoint secret, reject invalid signatures, and store the event ID before applying side effects. See [Stripe webhook signature verification](https://docs.stripe.com/webhooks/signature) and [Stripe fulfillment guidance](https://docs.stripe.com/checkout/fulfillment).

The success page may show “payment processing” until the webhook marks the order paid. It must not allocate codes on its own.

## Controllers and route compatibility

Preserve these v1 endpoints unchanged:

- POST /Checkout/Pay
- GET /Checkout/Complete
- GET /Checkout/Cancelled

Add explicit routes instead of overloading legacy Pay:

- POST /Checkout/Payments/paypal-v2/orders
- POST /Checkout/Payments/paypal-v2/orders/{paypalOrderId}/capture
- POST /Checkout/Payments/stripe/session
- GET /Checkout/Payments/stripe/success
- GET /Checkout/Payments/stripe/cancel
- POST /webhooks/paypal
- POST /webhooks/stripe

The exact names may follow application conventions, but create, capture, return, cancel, and webhook responsibilities should remain distinguishable.

Refactor GetPaymentId so it is explicitly a v1 compatibility resolver. New flows must use a validated internal order ID plus provider reference, not a PayPal cookie or TempData.

## Frontend changes

### Raw ecommerce frontend

Refactor src/Ecommerce/HypnoTech.Ecommerce.Frontend.Frontend/js/checkout.js around shared helpers:

- buildCheckoutPayload() for address, country, delivery choice, and cart identifiers.
- validateCheckout() for the existing Knockout validation.
- processingProvider and one payment error observable rather than one undifferentiated flag.
- startPayPalV1() retaining the current behavior.
- startPayPalV2() using the new create/capture endpoints.
- startStripe() creating a hosted Checkout Session and redirecting.
- Disable every payment button while a request is in flight, then re-enable on recoverable failure.

Keep payWithPaypal as an alias to v1 until templates and external callers migrate.

Expose only public configuration to the page: provider enabled flags, PayPal public client ID/environment if required by its browser SDK, and a Stripe publishable key only if a later client SDK needs it. Never render secrets or webhook configuration in HTML/JavaScript.

### Esoteric views

Update sites/Esoteric/Esoteric.Website/Views/Checkout/Index.cshtml to retain the v1 button and add separate PayPal v2 and Stripe controls. Use neutral labels and one provider-specific error at a time. The v2 button may render into a provider container; Stripe initially needs only a redirect button.

Update Complete.cshtml and Cancelled.cshtml to use a provider-neutral result view model. Do not assume every provider has a PayPal SaleId; show a safe reference and a pending state when fulfillment is still being reconciled.

Update _CheckoutLayout.cshtml only as needed to load the current PayPal browser SDK when v2 is enabled. Stripe-hosted Checkout does not require Stripe.js in the first implementation.

SelfHypnosis has no checkout views. Do not add a duplicate payment implementation there; add site markup only when it actually enables ecommerce.

## Configuration and CMS

Preserve PaypalConfig for v1. Add separate sections so credentials cannot be mixed:

~~~csharp
public sealed class PaypalV2Config
{
    public bool Enabled { get; set; }
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public bool IsProduction { get; set; }
    public string ReturnUrl { get; set; } = "";
    public string CancelUrl { get; set; } = "";
    public string WebhookId { get; set; } = "";
}

public sealed class StripeConfig
{
    public bool Enabled { get; set; }
    public string PublishableKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string WebhookSecret { get; set; } = "";
    public string SuccessUrl { get; set; } = "";
    public string CancelUrl { get; set; } = "";
}
~~~

Prefer environment/secret-store overrides. If CMS remains the operational source, mask secrets, do not echo them to browser responses, restrict the settings area, and provide explicit rotation.

Update:

- src/Common/HypnoTech.Common.Models/Settings/Config.cs
- new PaypalV2Config.cs and StripeConfig.cs
- src/Common/HypnoTech.Common.Dal.MongoDb/StartupBuilderExtensions.cs
- src/Cms/HypnoTech.Cms/Areas/Settings/Views/Config/Index.cshtml
- restored raw source src/Cms/HypnoTech.Cms.Frontend/js/config.js
- startup registration and configuration validation

Register typed provider clients through IHttpClientFactory or official SDK clients. Do not construct clients per request with secrets read ad hoc from the database.

## Fulfillment and state transitions

Move the current completion block from CheckoutService.Complete into IOrderFulfillmentService:

1. Verify provider status and amount.
2. Atomically transition pending/approved to paid/complete.
3. Allocate download codes once.
4. Send customer and administrator email once.
5. Record provider reference, paid timestamp, and processed event ID.

Recommended transitions:

Created -> Pending -> Approved -> Paid -> Shipped

with branches:

Pending -> Failed, Pending -> Cancelled, Paid -> Refunded.

The transition must be safe when called by both browser capture and webhook. A previously paid order returns success without repeating side effects.

## Security requirements

- Recalculate prices, postage, currency, and availability on the server.
- Treat cart IDs, addresses, return URLs, provider IDs, and browser status values as untrusted.
- Use HTTPS and allow-listed return/cancel hosts.
- Keep antiforgery protection on browser POSTs; webhook endpoints use provider signature verification instead.
- Preserve raw webhook bodies for signature verification.
- Never log card data, secrets, OAuth tokens, signatures, or complete payment payloads.
- Apply rate limits and correlation IDs to payment and webhook endpoints.
- Use provider idempotency keys and Mongo uniqueness constraints.
- Keep credentials out of frontend bundles and generated assets.

## Testing plan

### Unit tests

- Authoritative amount breakdown, including tracked/special-delivery and postage.
- Currency rounding and provider amount mismatch rejection.
- Unchanged PayPal v1 mapping.
- PayPal v2 order/capture mapping.
- Stripe Session options, metadata, and idempotency key.
- Order state transitions and duplicate completion.
- Backwards-compatible deserialization of old Mongo orders.

### Integration tests

- Provider timeout, retry, malformed response, decline, and amount mismatch.
- No secret in HTML, JSON, logs, or exceptions.
- Valid/invalid PayPal signatures and valid/invalid Stripe signatures.
- Duplicate/out-of-order webhook events cause one fulfillment.
- v1 routes and response shapes remain compatible.

### Sandbox/browser tests

PayPal Sandbox: create/approve/capture, declined instrument, payer action required, duplicate capture, cancel, abandoned checkout, webhook validation, and amount mismatch.

Stripe test mode/CLI: hosted success/cancel, checkout.session.completed, async success/failure if enabled, duplicate/out-of-order events, invalid signature, and expired session.

Esoteric browser coverage: address validation, switching providers, double-click prevention, refresh while pending, mobile layout, and cart clearing only after verified completion. Run the existing frontend checks after rebuilding raw sources.

## Deployment and rollout

1. Add neutral contracts, additive fields, and idempotency without changing v1.
2. Deploy configuration and feature flags with v2/Stripe disabled.
3. Deploy adapters and webhooks in sandbox/test mode.
4. Register public HTTPS webhook endpoints and verify signatures.
5. Enable PayPal v2 for internal/test users, then a small production cohort.
6. Enable Stripe after Checkout and fulfillment reconciliation is confirmed.
7. Monitor payment attempts, webhook failures, duplicate events, amount mismatches, failed fulfillment, and email/download-code errors.
8. Keep v1 until v2 and Stripe have production history and reconciliation reports. Deprecate it separately; do not remove its package, routes, fields, or historical mappings in this work.

Rollback is a feature-flag change: disable new providers while v1 readers and routes remain available.

## Suggested implementation commits

1. **Payment contracts and data model** — neutral models, payment attempts, additive Order fields, mappings, indexes, and v1 readers.
2. **Checkout orchestration and fulfillment** — stable internal IDs, provider selection, idempotent transitions, and extraction of side effects.
3. **PayPal v2 adapter** — package, OAuth/client registration, Orders v2 create/capture, mapping, flag, and webhook verification.
4. **Stripe adapter** — Stripe.net, hosted Session, idempotency, mapping, and webhook verification.
5. **Frontend and Esoteric views** — raw checkout source, provider controls/config, neutral result views, and bundle rebuild.
6. **CMS/configuration and observability** — masked settings, secret handling, logs, dashboards, and operational documentation.
7. **Tests and sandbox rollout** — unit/integration/browser coverage, migration checks, and staged enablement.

## File-level change map

| Area | Files | Planned change |
| --- | --- | --- |
| Frontend source | src/Ecommerce/HypnoTech.Ecommerce.Frontend.Frontend/js/checkout.js | Provider-neutral state/helpers; retain v1 alias; add v2/Stripe starts. |
| Ecommerce views | sites/Esoteric/Esoteric.Website/Views/Checkout/* | Provider controls and neutral result/pending views. |
| Controller | src/Ecommerce/HypnoTech.Ecommerce.Frontend/Controllers/CheckoutController.cs | Preserve v1 actions; add provider actions and webhooks. |
| Orchestration | src/Ecommerce/HypnoTech.Ecommerce.Frontend/Services/CheckoutService.cs | Delegate to neutral payment and fulfillment services. |
| Providers | Services/PayPalService.cs, new PayPalV2Service.cs, new StripePaymentService.cs | Isolate v1, PayPal Orders v2, and Stripe Checkout. |
| Contracts | Services/Interfaces/*, new payment models | Provider-neutral session/completion contracts. |
| Persistence | src/Ecommerce/HypnoTech.Ecommerce.Models/Order.cs, PaymentAttempt.cs, Ecommerce.Dal/Repositories/* | Provider references, attempts, indexes, and idempotent transitions. |
| Configuration | Common.Models/Settings/*, Mongo startup mappings | PayPal v2 and Stripe options; preserve v1 options. |
| CMS | src/Cms/HypnoTech.Cms/Areas/Settings/Views/Config/Index.cshtml, src/Cms/HypnoTech.Cms.Frontend/js/config.js | Masked settings and public-config projection. |
| Admin order UI | src/Ecommerce/HypnoTech.Ecommerce/Areas/Settings/Views/Order/Details.cshtml | Provider-neutral payment details. |
| Startup | Ecommerce StartupBuilderExtensions.cs | Provider clients, options, webhook handling, and fulfillment. |

## Acceptance criteria

- Existing PayPal v1 checkout, completion, cancellation, order history, and admin links still work.
- PayPal v2 uses server-created Orders v2 and server-side capture; browser totals cannot create a payable order.
- Stripe uses a server-created hosted Checkout Session and webhook-backed fulfillment.
- Every new payment has a stable internal order/payment-attempt ID and provider reference.
- Duplicate callbacks/webhooks cannot duplicate download codes or email.
- Old Mongo orders remain readable and new orders show provider-neutral details in admin.
- Secrets never appear in browser assets or logs.
- Esoteric can enable each provider independently; SelfHypnosis needs no duplicate implementation.
- The frontend is rebuilt from raw source and passes existing frontend checks.

## Decisions to confirm before implementation

1. PayPal current JS SDK button flow or a v2 approval redirect. Button flow is the recommended default because it maps cleanly to server create/capture endpoints; validate exact SDK version and browser support before coding.
2. Stripe hosted Checkout (recommended) or Elements. Hosted Checkout is the lower-risk first release.
3. Keep the existing site address as shipping source, or let each provider collect it. The initial implementation should avoid a second conflicting address.
4. Store payment attempts separately from orders. This is recommended because retries/provider switching otherwise make reconciliation ambiguous.
5. Immediate capture (CAPTURE) or authorize then capture. Immediate capture fits the current one-time digital/merchandise flow unless fulfillment policy changes.

## References

- [PayPal standard checkout integration](https://developer.paypal.com/platforms/checkout/standard/integrate)
- [PayPal Orders API v2](https://developer.paypal.com/api/orders/v2/)
- [PayPal REST authentication](https://developer.paypal.com/api/rest/authentication/)
- [PayPal webhooks](https://developer.paypal.com/api/rest/webhooks)
- [PayPal security guidelines](https://developer.paypal.com/security-guidelines)
- [PayPal .NET Server SDK](https://github.com/paypal/PayPal-Dotnet-Server-SDK)
- [PayPalServerSDK on NuGet](https://www.nuget.org/packages/PayPalServerSDK/)
- [Stripe Checkout Sessions](https://docs.stripe.com/api/checkout/sessions)
- [Stripe create Checkout Session](https://docs.stripe.com/api/checkout/sessions/create)
- [Stripe Checkout quickstart](https://docs.stripe.com/payments/checkout/quickstarts)
- [Stripe fulfillment](https://docs.stripe.com/checkout/fulfillment)
- [Stripe webhook signature verification](https://docs.stripe.com/webhooks/signature)
- [Stripe idempotent requests](https://docs.stripe.com/api/idempotent_requests)
- [Stripe .NET SDK](https://github.com/stripe/stripe-dotnet)
