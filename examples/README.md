# Lantern House public example

Lantern House is a fictional independent arts venue built to demonstrate the reusable
HypnoTech CMS, Media, Ecommerce, Frontend, and swappable persistence modules. Its names, writing,
models, seed documents, vector artwork, audio, video, and styling were created for this
repository. Nothing was copied or adapted from a production client site.

## What it demonstrates

- Runtime-generated CMS controllers, routes, lists, forms, roles, search, ordering,
  duplication, and file cloning.
- Text, textarea, rich-text, date, checkbox, money, hidden, generated, read-only,
  static-select, repository-select, nested-select, list, file, image, and gallery
  mappings.
- Articles, events, ordered artist profiles, the platform audio player, uploaded
  images/files, local MP4 video, CMS-managed YouTube video with a lightbox, photo
  galleries, merchandise categories, product options, postage profiles, and the
  platform basket and calculation-only checkout.
- Database-backed navigation that the admin Features screen can enable, rename, and
  reorder, plus editable site and email configuration.
- A responsive original website exercised by focused Playwright desktop and mobile
  journeys.
- Progressive partial-page navigation with full-document fallback, persistent audio
  player and basket drawers, page-level track launch events, and a deliberately
  separate checkout flow.
- A deliberate Admin/Website boundary: shared domain models remain separate from edit
  mappings, fluent CMS mappings and migrations remain in Admin, and the public site uses focused read
  repositories without referencing CMS web or admin assemblies.
- One readable `Demo:Provider` switch in each Startup selects MongoDB, PostgreSQL, or
  SQL Server. Admin automatically migrates relational databases before idempotent seed.

## Developer guides

- [Build a CMS-managed model](../docs/cms-models.md) explains the model, fluent admin
  mapping, MongoDB mapping, generated controller, relationships, ordering and uploads.
- [Integrate frontend features](../docs/frontend-integration.md) explains the compiled
  engine bundles, partial navigation contract, persistent player, event bus, basket,
  page scripts and full-document boundaries.

## Prerequisites

- .NET SDK 10.0
- Podman

No Node.js build is required to run the example. Its generated development and
minified browser assets are committed and self-hosted. Contributors changing files in
`HypnoTech.Demo.Website.Frontend` regenerate them with `npm run build:frontend` and
verify them with `npm run check:frontend` from the repository root.

## One-command launch

From the repository root, the launcher starts the selected database, migrates when
needed, seeds the demo, and runs both applications. MongoDB is the default:

```bash
./examples/run-demo.sh
./examples/run-demo.sh --provider postgres
./examples/run-demo.sh --provider sqlserver
```

It prints the URLs and development login when both applications are ready. It also
pins the admin and website to the same absolute `.demo/uploads` path, so files uploaded
through the CMS are visible from either host. Press Ctrl+C to stop the admin and
website. The named database container and volume remain in place for the next run; use
the reset commands below when you want fresh data.

The showcase projects intentionally reference all three adapters so one configuration
value can demonstrate each option. A real application references only its selected
thin provider project, so choosing SQL Server does not bring in Npgsql and choosing
PostgreSQL does not bring in the SQL Server driver.

## Automated browser journeys

From the repository root, this command starts an isolated MongoDB container and both
example hosts, then runs the focused Chromium suite:

```bash
./tests/run-end-to-end.sh
```

Run the same journeys across Chromium, Firefox, and WebKit with:

```bash
./tests/run-end-to-end.sh --all-browsers
```

The complete three-engine command is intended for a Playwright-supported
Ubuntu/Debian host and the future GitHub Actions matrix. On Fedora, use the Chromium
or Firefox selector for local checks; WebKit is covered by the supported Ubuntu run.

The browser suite uses a unique disposable database and upload directory. It does not
use the persistent manual-demo database and deliberately stops before any payment or
order submission.

Run all journeys against either relational provider with:

```bash
./tests/run-end-to-end.sh --provider postgres
./tests/run-end-to-end.sh --provider sqlserver
```

## Start a database manually

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
podman compose -f examples/compose.yaml --profile mongodb up -d mongo
podman compose -f examples/compose.yaml --profile postgresql up -d postgres
podman compose -f examples/compose.yaml --profile sqlserver up -d sqlserver
```

Neither command requires the Docker daemon or a Docker-command compatibility package.

## Seed the demo database

From the repository root, run:

```bash
Demo__Provider=PostgreSql dotnet run --project examples/HypnoTech.Demo.Admin -- --seed-only
```

Seeding is additive and idempotent per collection: it populates only empty demo
tables or collections. Relational startup first applies pending Admin-owned migrations.
Seeding also copies the synthetic media fixtures into the configured shared
upload store. The seed command refuses to run outside the Development environment.

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

The initial Phase 4 preview used the database name `hypnotech_demo`. The corrected
example uses `hypnotech_demo_lantern`, so an existing named volume receives the revised
seed without deleting the earlier preview database. The full reset below removes both.

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
podman rm -f hypnotech-demo-postgres hypnotech-demo-sqlserver
podman volume rm hypnotech-demo-postgres hypnotech-demo-sqlserver
```

Run the start and seed commands again to recreate the example.
