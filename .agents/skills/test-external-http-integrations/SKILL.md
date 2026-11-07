---
name: test-external-http-integrations
description: Test HypnoTech outbound HTTP clients and third-party SDK adapters without contacting live or sandbox services. Use when adding or revising provider integrations, protocol stubs, request/response mapping, authentication, provider errors, or cancellation behaviour.
---

# Test external HTTP integrations

Exercise the real protocol boundary locally. Do not make automated test reliability or
safety depend on a provider account, network access, sandbox state, or SDK internals.

## Establish the seam

1. Read `AGENTS.md`, `tests/README.md`, `docs/compatibility.md`, and any current plan
   explicitly governing the requested work.
2. Identify the HypnoTech adapter, the real `HttpClient` or provider SDK it uses, and
   the observable engine response.
3. Introduce only the smallest test seam needed to select a loopback endpoint or client,
   such as dependency injection, a message handler, or an overridable client factory.
   Do not leak provider-specific types across engine contracts merely to make testing
   convenient.
4. Keep pure request/response mapping unit-testable when it has substantial branching,
   but prove serialization and transport behaviour through a protocol integration test.

## Use a local protocol stub

- Prefer an in-process ASP.NET Core/Kestrel application bound to
  `http://127.0.0.1:0`, following `PayPalServiceTests`, and discover its assigned port
  after startup.
- Send requests through the production SDK or HTTP client. Do not mock the SDK's
  internal services, handlers, or response objects.
- Capture requests in concurrency-safe structures and dispose the host asynchronously.
- Use synthetic credentials, identities, addresses, payloads, and provider IDs. Never
  copy production or private-site values into fixtures.
- Never contact live or sandbox endpoints. The test must remain deterministic without
  DNS, provider credentials, or internet access.
- Prefer the existing dependency-free Kestrel pattern. Add MockServer, WireMock, or a
  similar package only when repeated protocol complexity justifies the dependency;
  pin it centrally and review its direct and transient packages.

## Cover the contract that matters

Assert semantic behaviour rather than incidental JSON formatting:

- HTTP method, path, meaningful query values, authentication scheme, required headers,
  and important request-body fields;
- successful response mapping and externally visible identifiers;
- provider validation failures and safe user-facing error mapping;
- authentication failure, rate limiting, server errors, malformed payloads, and
  timeouts or cancellation when the production client supports them;
- idempotency keys, retry behaviour, and the uncertain-outcome boundary for
  side-effecting requests when applicable.

Do not invent automatic retries for payments or other non-idempotent operations. Do not
log secrets or retain them in test artifacts. A browser journey may prove the engine's
UI safety boundary, but it should not replace protocol-level adapter coverage.

## Verify and record

1. Run the focused integration project and repeat stateful cases when lifecycle leaks
   are plausible.
2. Run architecture and unit tests affected by a new client seam.
3. Run `verify-engine-change` and record provider behaviours, commands, results, skips,
   and remaining limitations in the current task or its explicitly active plan.
