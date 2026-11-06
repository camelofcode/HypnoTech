# Lantern House public example

Lantern House is a fictional independent arts venue built to demonstrate the reusable
HypnoTech CMS, Media, Ecommerce, Frontend, and MongoDB modules. Its names, writing,
models, seed documents, vector artwork, audio, video, and styling were created for this
repository. Nothing was copied or adapted from a production client site.

## What it demonstrates

- Runtime-generated CMS controllers, routes, lists, forms, roles, search, ordering,
  duplication, and file cloning.
- Text, textarea, rich-text, date, checkbox, money, hidden, generated, read-only,
  static-select, repository-select, nested-select, list, file, image, and gallery
  mappings.
- Articles, events, ordered artist profiles, audio, local video, photo galleries,
  merchandise categories, product options, postage profiles, a basket, and
  server-calculated totals.
- Database-backed navigation that the admin Features screen can enable, rename, and
  reorder, plus editable site and email configuration.
- A responsive original website with stable `data-testid` hooks for the later
  Playwright phase.

## Prerequisites

- .NET SDK 10.0
- Podman

No Node.js build is required for the example. All example browser assets are committed
and self-hosted.

## Start MongoDB

The example uses the same pinned supported MongoDB release as the engine integration
suite. Start it with Podman:

```bash
podman run -d --replace --name hypnotech-demo-mongo \
  -p 127.0.0.1:27017:27017 \
  -v hypnotech-demo-mongo:/data/db \
  docker.io/library/mongo:8.0.32
```

If a Compose provider is configured for Podman, the equivalent command is:

```bash
podman compose -f examples/compose.yaml up -d mongo
```

Neither command requires the Docker daemon or a Docker-command compatibility package.

## Seed the demo database

From the repository root, run:

```bash
dotnet run --project examples/HypnoTech.Demo.Admin -- --seed-only
```

Seeding is additive and idempotent per collection: it populates only empty demo
collections. The seed command refuses to run outside the Development environment.

The development administrator is:

- Username: `admin`
- Password: `HypnoTechDemo!2026`

The plaintext password is deliberately documented for local automation. It is hashed
with the engine's bcrypt helper at seed time and is never stored in MongoDB as
plaintext. The known credential is seeded only in Development.

## Run the applications

Start the admin application in one terminal:

```bash
dotnet run --project examples/HypnoTech.Demo.Admin
```

Start the public website in another:

```bash
dotnet run --project examples/HypnoTech.Demo.Website
```

Then open:

- Admin: <http://localhost:5101>
- Website: <http://localhost:5100>

The admin also seeds empty collections during Development startup, so a separate seed
command is optional for ordinary local use.

## Payment safety

The example recalculates basket, option, postage, tracked-delivery, and total values on
the server using the Ecommerce engine. It never creates an order or contacts PayPal,
Stripe, or any other provider.

Legacy payment, completion, and cancellation routes return HTTP 410 before MVC can
invoke a payment service. The visible checkout stopping point explicitly confirms that
no payment, email, download code, order, or fulfilment action occurred. A reusable
Development/Test-only mock provider is recorded as deferred work in
[`../ecommerce-payments-plan.md`](../ecommerce-payments-plan.md).

## Reset local demo data

Stop the applications before resetting. The following removes only the named demo
container and its named volume:

```bash
podman rm -f hypnotech-demo-mongo
podman volume rm hypnotech-demo-mongo
```

Run the start and seed commands again to recreate the example.
