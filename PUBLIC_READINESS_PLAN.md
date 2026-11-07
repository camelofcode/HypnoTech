# HypnoTech public-readiness plan

> **Document type:** Living roadmap and delivery record
>
> **Repository:** `camelofcode/HypnoTech`
>
> **Last updated:** 2026-11-07
>
> **Current phase:** Phase 9 in progress; shared EF repositories pass on PostgreSQL and SQL Server
>
> **Next action:** Add provider-specific Lantern House Admin contexts/migrations and a documented demo provider switch.

## How to maintain this document

This file is the source of truth for preparing the HypnoTech engine for public release.
Update it in every pull request or work session that advances this programme:

1. Update the summary and phase status table.
2. Check off only work that is complete and verified.
3. Record material decisions in the decision log.
4. Record completed work and its verification in the work log.
5. Add newly discovered risks or questions instead of silently expanding scope.
6. Keep the **Next action** field at the top accurate.

Use these statuses consistently:

- **Not started** — no implementation work has begun.
- **In progress** — implementation has begun but the exit gate has not passed.
- **Blocked** — progress requires a decision or external change; record the blocker.
- **Complete** — every required item and the phase exit gate have been verified.
- **Deferred** — explicitly outside the current release, with a reason recorded.

The work log should identify the commit or pull request when one exists. A phase is not
complete merely because its code was written; its exit gate must also pass.

## Goal

Prepare the reusable HypnoTech engine for a credible public portfolio release without
exposing private site content, disrupting existing production sites, or accidentally
forcing a MongoDB migration.

The programme has two intended releases:

1. **v1.0 — public MongoDB baseline:** preserve current behaviour, improve repository
   structure and documentation, add representative examples, and establish automated
   tests and CI, then modernise genuine I/O paths to use async APIs before publication.
2. **v1.1 — interchangeable persistence:** add provider-neutral Entity Framework Core
   implementations and thin database-provider packages after the MongoDB baseline is
   stable and public.

## Scope and non-negotiable boundaries

### In scope

- Only the engine repository at `camelofcode/HypnoTech`.
- Preserving a stable production-compatible engine baseline.
- Public-repository safety, licensing, documentation, and presentation.
- A compact root `AGENTS.md` and a small set of useful repository skills.
- Honest GitHub language classification.
- Physical organisation of engine projects by functional area.
- Focused unit, integration, and browser tests.
- Original public example admin and website applications.
- Async repository, service, and controller I/O paths with cancellation support.
- Linux GitHub Actions for build and test verification.
- A later provider-neutral EF Core persistence architecture.

### Out of scope

- Changes to the Esoteric repository or its committed files.
- Changes to the SelfHypnosis repository or its committed files.
- Publishing private content, branding, seed data, templates, or imagery from either
  site.
- Migrating the production MongoDB deployment as part of v1.0. A deferred, non-
  executable runbook is recorded in
  [`docs/operations/mongodb-production-migration.md`](docs/operations/mongodb-production-migration.md).
- PayPal v2, Stripe, or other work described in `ecommerce-payments-plan.md`.
- Exhaustive test coverage.
- Windows as a required CI or production platform.
- Reintroducing synchronous repository or I/O-bound service APIs on public `main`.
  Production consumers retain the historic API on `support/legacy-production`.

Esoteric and SelfHypnosis are external production consumers. They may be used for
read-only compatibility analysis and temporary, uncommitted smoke-test copies, but
this programme must not commit changes to either repository.

## Current repository baseline

Recorded during the planning audit on 2026-11-06:

- Engine working tree was clean and synchronized with `origin/master`.
- Current engine commit was `95c9f6a` (`Document Ecommerce upgrade plan`).
- Commit `95c9f6a` was confirmed as the engine revision used by both live sites.
- Both live sites were confirmed to use MongoDB Server 4.0.3.
- The consolidated repository had 10 commits and no tags.
- The default branch was `master`; the desired eventual default is `main`.
- The GitHub repository was confirmed to be private with `master` as its default branch.
- Phase 0 created `main` at the production baseline and made it the private repository's
  default while retaining frozen `master` as a transition safeguard.
- The solution contained 21 .NET 10 projects.
- Projects were grouped virtually in `HypnoTech.slnx` but physically flat under
  `src/`.
- No engine test projects, GitHub Actions workflows, root README, root licence,
  `.gitattributes`, or `AGENTS.md` were present.
- Frontend tooling already used four npm workspaces, exact build-tool versions, and a
  root lockfile.
- Tracked source contained approximately 15.7 MB of JavaScript and 396 KB of C#.
  After excluding vendored libraries and generated bundles, authored JavaScript was
  approximately 75 KB. The GitHub language result is therefore dominated by vendored
  and generated assets rather than authored application code.
- Both private sites referenced engine project files by their current physical paths.
  Moving those projects would break a site checkout that was pointed directly at the
  reorganised engine `main` branch.
- The engine referenced `MongoDB.Driver` 3.4.3 and the live sites used MongoDB Server
  4.0.3. This exact pair is the compatibility baseline until it is exercised by an
  integration test and a separate migration plan authorises a change.
- The engine contained 287 tracked C# files, 20 controller files, 21 repository
  interface files, and 23 repository implementation files. Only three files contained
  actual `await` expressions. Authentication and custom MVC result rendering already
  used async APIs, while most MongoDB, CMS, email, file, and HTTP I/O remained
  synchronous.

Re-run and update this baseline if work begins significantly later than the recorded
date.

## Programme status

| Phase | Outcome | Status | Target release |
| --- | --- | --- | --- |
| Planning | Evidence-based roadmap and tracking system | Complete | Pre-work |
| 0 | Stable baseline preserved | Complete | Pre-work |
| 1 | Repository safe and understandable for public release | Complete | v1.0 |
| 2 | CMS and MongoDB behaviour characterised by tests | Complete | v1.0 |
| 3 | Projects physically organised without semantic changes | Complete | v1.0 |
| 4 | Original public example admin and website | Complete | v1.0 |
| 5 | Focused Playwright portfolio smoke suite | Complete | v1.0 |
| 6 | Async I/O modernisation | Complete | v1.0 |
| 7 | CI, documentation, `main`, and public release | Complete | v1.0 |
| 8 | Persistence contracts hardened | Complete | v1.1 |
| 9 | EF Core implementations and provider packaging | In progress | v1.1 |

### Ordering constraints

- Phase 4 must produce a runnable example admin and website before Phase 5 creates
  browser tests against them.
- Phase 5 must establish regression journeys before Phase 6 converts request paths to
  async, so the conversion is checked through the user interface as well as unit and
  integration tests.
- Phase 6 must establish the primary async contracts while MongoDB is still the only
  DAL implementation.
- Phases 8 and 9 must consume those async contracts. No synchronous EF Core DAL should
  be created with the intention of converting it later.

## Recommended technical decisions

These remain proposals until marked **Accepted** in the decision log.

| Area | Recommendation |
| --- | --- |
| Baseline preservation | Annotated tag plus a long-lived `support/legacy-production` branch |
| Public development | Work on `public-readiness`, ultimately merge into `main` |
| Tests | xUnit v3 on Microsoft Testing Platform, Testcontainers, then Playwright for .NET |
| CI | Linux-only GitHub Actions initially |
| Async APIs | Add true async repository/service methods, use them from controllers, and propagate cancellation |
| JavaScript | Keep self-hosted assets; classify vendor/generated paths with Linguist attributes |
| Source licence | PolyForm Noncommercial 1.0.0 plus separate commercial licensing terms |
| Public examples | New neutral demo content, never sanitised private-site content |
| Persistence | Shared provider-neutral EF code plus thin provider-specific packages |
| Payments | Keep the existing payment plan deferred and out of v1.0/v1.1 scope |

### Deferred operational plans

- [Future production MongoDB migration](docs/operations/mongodb-production-migration.md)
  — prefer a rehearsed blue/green logical BSON migration to a pinned Podman container;
  retain successive in-place major upgrades as the fallback. No production work is
  authorised by this plan.

## Phase 0 — Preserve and define the stable baseline

### Purpose

Make the present state recoverable before changing code, layout, dependencies, or Git
history. Distinguish the current development commit from the version actually deployed
to production.

### Work

- [x] Record the exact MongoDB server version used by production: 4.0.3.
- [x] Identify the exact engine commit currently deployed to production: `95c9f6a`.
- [x] Decide whether `95c9f6a` is a production baseline or only the pre-public
      development baseline.
- [x] Create the annotated tag `production-baseline-2026-11-06` at `95c9f6a`.
- [x] Create `support/legacy-production` at the deployed/stable commit.
- [x] Push the baseline tag and support branch to the remote.
- [x] Create and push `main` at `95c9f6a` and make it the GitHub default branch.
- [x] Retain `master` frozen at `95c9f6a` during the transition.
- [x] Create and push `public-readiness` from `main` for public-release work.
- [x] Create and verify a private `git bundle` backup before any history cleanup.
- [x] Verify authenticated GitHub CLI and Git access with effective `admin` and `push`
      permissions on the private repository.
- [x] Record .NET SDK, Node, npm, MongoDB driver, build commands, and relevant generated
      asset hashes.
- [x] Define the rule that the support branch receives production hotfixes only.
- [x] Complete Phase 0 without upgrading production-sensitive dependencies.

### MongoDB compatibility gate

The current engine uses MongoDB.Driver 3.4.3 against MongoDB Server 4.0.3. Driver and
server version numbers are independent. This exact production pair must be exercised
by an integration test before any driver upgrade or support claim. The support branch
must not advance the MongoDB driver or server compatibility floor without a separate,
tested migration decision.

### Exit gate

- [x] Baseline tag is present on the remote.
- [x] Support branch is present on the remote.
- [x] `main` is the private repository's default branch and `master` remains frozen.
- [x] Public work is isolated on `public-readiness`.
- [x] Production commit and MongoDB version are documented.
- [x] Private backup passes `git bundle verify` and records complete branch history.

## Phase 1 — Public-repository safety and foundation

### 1.1 Secrets and history audit

- [x] Scan the current tree and every historical commit for credentials and secrets.
- [x] Inspect deleted history as well as current configuration.
- [x] Check connection strings, SMTP credentials, API keys, payment credentials,
      certificates, seed users, and private URLs.
- [x] Rotate any exposed credential before cleaning history. No exposed credential was
      found, so no rotation was required.
- [x] If required, clean history with a reviewed procedure or publish a deliberately
      clean public history while retaining the private bundle. No cleanup was indicated.
- [x] Re-scan the exact history intended for publication immediately before visibility
      changes.
- [x] Record the scanner, version, command, date, and result in the work log and
      `docs/audits/publication-history-2026-11-06.md`.

### 1.2 Licensing and third-party material

- [x] Confirm the source-code licence decision.
- [x] Add the unmodified PolyForm Noncommercial 1.0.0 licence.
- [x] Add `COMMERCIAL-LICENSE.md` explaining that commercial use requires a separate
      written agreement.
- [x] Describe the project as **source-available for noncommercial use**, not OSI open
      source, if a noncommercial licence is selected.
- [x] Audit Bootstrap, jQuery, validation libraries, TinyMCE, Font Awesome, fonts, npm
      build tools, and NuGet dependencies.
- [x] Preserve existing third-party licence files and add common MIT and SIL OFL 1.1
      texts for vendored assets that lacked a tracked licence copy.
- [x] Add `THIRD_PARTY_NOTICES.md` with source, version, licence, and location.
- [x] Define a contribution policy compatible with any future dual-licensing model.
      External code contributions remain closed pending an explicit agreement/process.

### 1.3 GitHub language classification and frontend reproducibility

- [x] Add a root `.gitattributes`.
- [x] Mark third-party `lib` trees as `linguist-vendored`.
- [x] Mark generated `wwwroot` bundle/output trees as `linguist-generated`.
- [x] Do not mark authored JavaScript as C# or otherwise misclassify it.
- [x] Verify attributes locally with `git check-attr`; a full local GitHub Linguist
      installation was not introduced solely for this check.
- [x] Push to a branch and confirm the GitHub language bar after its cache refreshes;
      GitHub reports C# as the clear primary language.
- [x] Keep runtime assets self-hosted and checked in.
- [x] Keep exact npm build-tool versions and the root lockfile.
- [x] Require `npm ci` in documented verification and future automation.
- [x] Keep generated runtime assets committed so ordinary .NET consumers do not need
      npm merely to build or use the engine.

### 1.4 Public repository documentation

- [x] Add `README.md` with purpose, feature summary, status, and quickstart.
- [x] Add `SECURITY.md`.
- [x] Add `CONTRIBUTING.md`.
- [x] Add `docs/architecture.md`.
- [x] Add `docs/compatibility.md` with tested .NET, Node, and MongoDB versions.
- [x] Add this public-readiness roadmap and work record.
- [x] Add `.editorconfig` after documenting existing formatting conventions.
- [x] Avoid combining repository hygiene with a wholesale nullable-reference or
      warnings-as-errors migration.

### 1.5 Agent guidance and repository skills

Add a compact root `AGENTS.md` containing:

- [x] Repository purpose and architecture map.
- [x] Standard restore, build, frontend, and test commands.
- [x] Public API and production-data compatibility rules.
- [x] The authoritative source versus generated asset locations.
- [x] A rule not to edit generated bundles directly.
- [x] A rule to preserve persisted MongoDB member names unless a migration is planned.
- [x] A requirement for characterisation tests before dynamic CMS changes.
- [x] The boundary excluding private site repositories.
- [x] The boundary deferring payment-provider work.
- [x] Explicit triggers for the initial repository skills.

The initial repository guidance started with two narrow skills:

- [x] `.agents/skills/verify-engine-change/`
  - Select and run the appropriate build, frontend freshness, unit, integration, and
    browser checks for the changed paths.
  - Report commands, results, skips, and remaining verification gaps.
- [x] `.agents/skills/cms-compatibility-review/`
  - Review changes affecting mappings, forms, dynamic controllers, routes,
    repositories, persisted data, and public behaviour before implementation.
  - Require a compatibility decision and targeted test plan.

Add further skills only after a repeated workflow exists. A public-release skill is a
good later candidate once the release process has been exercised manually.

Repeated implementation work has since justified these workflow-backed additions:

- [x] `.agents/skills/author-characterisation-tests/`
  - Choose the narrowest honest test boundary and define when new coverage is required.
  - Keep fixtures synthetic, deterministic, isolated, and compatible with the MongoDB
    test matrix.
- [x] `.agents/skills/author-async-request-paths/`
  - Keep repository and I/O-bound service contracts async-only with cancellation.
  - Preserve controller, module, and provider boundaries.
- [x] `.agents/skills/test-external-http-integrations/`
  - Exercise real HTTP clients and SDK adapters against local protocol stubs.
  - Prohibit live/sandbox calls and brittle mocks of third-party SDK internals.

### Exit gate

- [x] The exact publication history has passed the final pre-publication secret scan.
- [x] Source and third-party licensing are explicit, including dependencies whose
      restored metadata does not declare a licence.
- [x] GitHub language classification represents authored code honestly after the remote
      cache refreshes; local attribute and byte checks pass.
- [x] A new contributor can understand and build the engine from repository docs.
- [x] Agent instructions are concise, accurate, and repository skills pass the skill
      validator.

## Phase 2 — Characterisation and test foundation

### Test structure

Create an initial test layout:

```text
tests/
  HypnoTech.Cms.UnitTests/
  HypnoTech.Cms.MongoDb.IntegrationTests/
  HypnoTech.Common.UnitTests/
  HypnoTech.ArchitectureTests/
```

- [x] Use xUnit v3 consistently unless implementation research identifies a blocker.
- [x] Add the Microsoft Testing Platform runner and coverage dependencies centrally.
- [x] Add test projects to the solution under functional test folders.
- [x] Do not introduce a high global coverage threshold at the start.

### CMS unit and characterisation tests

- [x] `FormClassMapping` registration and lookup.
- [x] Member mappings for every supported input type.
- [x] Controller name, display name, role, data source, title, order, and capability
      configuration.
- [x] Generic controller discovery by `CmsControllerFeatureProvider`.
- [x] Dynamic route creation by `CmsControllerRouteConvention`.
- [x] `CmsController<T>` add, edit, duplicate, remove, ordering, redirect, and flash
      behaviour.
- [x] Unsupported-operation gates.
- [x] HTTP verbs and antiforgery attributes.
- [x] Form construction and validation.
- [x] Generated, hidden, and default member handling.
- [x] File, image, gallery, and list behaviour using isolated synthetic data and
      temporary storage.
- [x] `CmsService<T>` repository interactions.
- [x] Explicit isolation for static mapping, role, navigation, and model registries.

### MongoDB integration tests

- [x] Use Testcontainers for .NET with an xUnit lifecycle fixture.
- [x] Run the repository against the exact production `mongo:4.0.3` image.
- [x] Run it against the supported, pinned `mongo:8.0.32-noble` image.
- [x] Run it against the current, pinned `mongo:9.0.4` image.
- [x] Generic add, get, edit, and remove.
- [x] Pagination and total-page calculations.
- [x] Default and explicit sorting.
- [x] Search, including zero, one, and multiple searchable fields.
- [x] Ordered item movement and boundary behaviour.
- [x] Persisted BSON member names.
- [x] Child population, including a missing optional child reference.
- [x] List and gallery identifier generation.
- [x] Select-list lookup.
- [x] Seed behaviour.
- [x] Common, ecommerce, and media repository registration.
- [x] Deserialisation of representative legacy documents.
- [x] Repeated application-host construction and idempotent BSON/model registration.

### Resolved characterisation findings

The initial suite exposed four failure modes. A compatibility-reviewed follow-up fixed
them without changing routes, interfaces, collection names, BSON names, stored data, or
the MongoDB.Driver 3.4.3 baseline:

- search with zero searchable fields returns no matches instead of sending an invalid
  empty `$or`;
- child population skips a null child ID;
- an ordered move beyond either boundary is an idempotent no-op;
- repeated Common, Ecommerce, and Media MongoDB startup reuses explicitly registered
  BSON class maps and avoids duplicate model-registry entries.

### Exit gate

- [x] Dynamic CMS behaviour is characterised before structural refactoring.
- [x] The production MongoDB server version is exercised automatically.
- [x] Tests are deterministic when repeated and when run as a full suite.
- [x] Coverage is published as information even if it is not yet a hard global gate.

## Phase 3 — Physical project reorganisation

### Intended layout

```text
src/
  Cms/
    HypnoTech.Cms/
    HypnoTech.Cms.Contracts/
    HypnoTech.Cms.MongoDb/
    HypnoTech.Cms.Frontend/
  Common/
    HypnoTech.Common/
    HypnoTech.Common.Dal/
    HypnoTech.Common.Dal.MongoDb/
    HypnoTech.Common.Emails/
    HypnoTech.Common.Models/
    HypnoTech.Common.Security/
  Ecommerce/
    ...
  Media/
    ...
  Persistence/
    HypnoTech.MongoDb/
  Frontend/
    HypnoTech.Frontend/
    HypnoTech.Frontend.Frontend/
```

### Compatibility policy

The private sites currently reference engine project paths directly. The recommended
v1.0 policy is:

- Production sites remain on the preserved support branch/checkout.
- The public `main` branch receives the clean layout.
- Neither private site receives a committed project-reference change.
- Compatibility smoke builds use temporary copies or worktrees only.
- Engine-side compatibility shims are added only if unchanged private consumers must
  later build against public `main`.

### Work

- [x] Confirm the compatibility policy before moving files.
- [x] Perform moves with `git mv` in a dedicated mechanical commit.
- [x] Do not combine moves with namespace, assembly, API, or behaviour changes.
- [x] Update `HypnoTech.slnx`.
- [x] Update every engine `ProjectReference`.
- [x] Update npm workspace paths.
- [x] Update frontend builder path configuration.
- [x] Update documentation and automation paths.
- [x] Preserve assembly names and `_content/HypnoTech.*` static asset paths.
- [x] Confirm the diff is overwhelmingly recognised as renames.
- [x] Run all build, frontend, unit, and integration checks.
- [x] Run temporary private-consumer smoke builds without committing site changes.

### Exit gate

- [x] Public branch has the intended physical organisation.
- [x] No namespace, public API, assembly identity, persisted name, or asset URL changed
      accidentally.
- [x] All automated verification passes.
- [x] Production consumers remain untouched and pinned to their stable engine branch.

## Phase 4 — Original public example applications

### Intended layout

```text
examples/
  HypnoTech.Demo.Models/
  HypnoTech.Demo.Admin/
  HypnoTech.Demo.Website.Frontend/
  HypnoTech.Demo.Website/
  HypnoTech.Demo.slnx
  SeedData/
  README.md
  compose.yaml
```

### Content policy

Use a neutral fictional organisation, such as an independent arts venue, label, or
community festival. All names, prose, images, files, seed data, templates, and branding
must be newly created or permissively licensed. Do not sanitise or adapt private-site
content for publication.

### Features to demonstrate

- [x] Dynamic CMS controller generation.
- [x] Text, textarea, rich text, date, checkbox, money, hidden, generated, and read-only
      mappings.
- [x] Static, repository-backed, and nested select lists.
- [x] Images and file uploads.
- [x] Photo galleries and nested list items.
- [x] Ordering and duplication, including file-cloning behaviour.
- [x] Role restrictions.
- [x] Navigation and configuration.
- [x] News or articles.
- [x] Events.
- [x] People or team profiles.
- [x] Audio, video, and photo media.
- [x] Merchandise categories, items, options, postage, basket, and order administration.
- [x] Responsive public website rendering.
- [x] Partial-page navigation with direct-request fallback and persistent player/cart
      state; checkout remains an intentional full-document boundary.
- [x] A safe fake/demo payment boundary or checkout stopping point; no new real payment
      provider.

### Developer experience

- [x] One documented command starts the required local database.
- [x] One documented command seeds safe demo data.
- [x] Both applications have documented start commands.
- [x] Development credentials are safe, explicit, and impossible to use in production.
- [x] The example doubles as an integration-test host.
- [x] User-facing guides cover fluent CMS model setup and frontend integration.

### Exit gate

- [x] A fresh clone can launch the demo using only documented steps.
- [x] Major engine features are visibly demonstrated.
- [x] No private-site material or credentials are present.
- [x] Example behaviour is covered by integration tests.
- [x] The admin and website expose stable test hooks and startup commands required by
      the Phase 5 Playwright harness.

## Phase 5 — Focused Playwright suite

Create `tests/HypnoTech.Demo.EndToEndTests/` using Playwright for .NET and xUnit.
This phase starts only after the Phase 4 example admin and website run reliably; the
example applications are the browser-test system under test.

### Initial journeys

- [x] Admin login and logout.
- [x] Create, edit, search, reorder, duplicate, and remove a dynamic CMS item.
- [x] Validation errors preserve the submitted form and display useful feedback.
- [x] Upload an image and verify it appears on the public website.
- [x] Add merchandise to the basket and verify server-calculated totals.
- [x] View representative audio, video, and gallery content.
- [x] Verify unauthorised access is rejected or redirected.
- [x] Run a basic mobile viewport smoke test.

### Harness

- [x] Start MongoDB with Testcontainers.
- [x] Start example applications on dynamically allocated ports.
- [x] Use isolated browser contexts.
- [x] Run Chromium, Firefox, and WebKit on Linux; use a supported Ubuntu environment
      as the WebKit source of truth.
- [x] Serve the admin test host through ephemeral HTTPS so its secure authentication
      cookie is covered consistently by every engine.
- [x] Capture traces, screenshots, console errors, and application logs on failure.
- [x] Keep the suite deliberately focused rather than exhaustive.

### Exit gate

- [x] One command runs the suite locally.
- [x] Repeated runs are reliable.
- [x] Failures retain enough evidence to diagnose them.
- [x] The suite is suitable as a required CI check.

## Phase 6 — Async I/O modernisation

### Purpose

Modernise genuine I/O paths after behaviour is protected by unit, integration, and
browser tests. The goal is scalable, idiomatic ASP.NET Core code rather than making
every method return `Task` for appearance's sake.

This work deliberately happens while MongoDB is the only DAL implementation. It defines
and exercises the async repository contracts once, before any EF Core provider is
written, avoiding a temporary synchronous EF layer and a second conversion project.

The initial audit found real async work in authentication and custom MVC result
rendering, but only three C# files contained `await`. MongoDB repository operations,
the generic CMS service/controller flow, email sending and template reads, uploaded
file handling, and some HTTP calls were synchronous. `ApiService` used `.Wait()` and
the existing PayPal v1 service used `.Result` on HTTP operations.

### Design rules

- [x] Use native asynchronous APIs from MongoDB, streams, SMTP, HTTP, and ASP.NET Core.
- [x] Add the `Async` suffix to repository and service methods that return `Task` or
      `ValueTask`; preserve controller action routes and names.
- [x] Accept `CancellationToken` on public async I/O methods and pass
      `HttpContext.RequestAborted` from web requests.
- [x] Do not implement fake async with `Task.Run` around MongoDB, file, or other blocking
      I/O.
- [x] Keep CPU-only mapping, validation, image transformation, collection projection,
      and filesystem metadata operations synchronous unless measurement proves a need.
- [x] Do not block on tasks with `.Result`, `.Wait()`, or
      `GetAwaiter().GetResult()` in request paths.
- [x] Preserve routes, response types, validation, flash messages, persisted BSON names,
      and observable ordering.
- [x] Keep the existing PayPal v1 routes and behaviour; replacing its sync-over-async
      calls must not expand into the deferred payment-provider work.

### Compatibility approach

- [x] Add true async methods before removing synchronous methods.
- [x] Make async methods the implementation used by public-engine controllers and
      services.
- [x] Remove duplicate synchronous repository and I/O-bound service contracts and
      implementations before the first public release.
- [x] Keep unchanged production consumers pinned to `support/legacy-production` rather
      than maintaining two implementations on public `main`.
- [x] Keep CPU-only work synchronous and isolate framework-mandated synchronous
      contracts, such as Serilog `Emit`, behind an asynchronous boundary.

### Repository and service work

- [x] Add async CMS repository operations for list, page, get, add, edit, remove, and
      ordered movement.
- [x] Use MongoDB `FirstOrDefaultAsync`, `ToListAsync`, `InsertOneAsync`,
      `ReplaceOneAsync`, `DeleteOneAsync`, and `CountDocumentsAsync` as appropriate.
- [x] Add async methods across Common, Ecommerce, and Media repositories in bounded
      module-sized changes.
- [x] Add async methods to the public example's focused read repositories and Media
      enrichment hook so its database-backed routes are async end to end.
- [x] Update CMS and module services to await repository I/O.
- [x] Propagate cancellation tokens without replacing them with `CancellationToken.None`.
- [x] Convert select-list, seed, and child-population database access to async flows.
- [x] Decide whether multi-document ordered movement needs a transaction or documented
      best-effort semantics; async conversion must not hide the existing atomicity issue.

### Controller and infrastructure work

- [x] Convert `CmsController<T>` I/O actions to `async Task<IActionResult>`.
- [x] Convert settings, ecommerce, media, and other I/O-bound controllers in small
      reviewed groups.
- [x] Convert the public example's Articles, Artists, Events, Home, Media, and Shop
      database-backed actions while retaining synchronous render-only actions.
- [x] Preserve synchronous actions that perform only validation or in-memory work.
- [x] Replace blocking `ApiService` HTTP calls with awaited `HttpClient` calls.
- [x] Replace PayPal v1 `.Result` calls in engine request paths with awaited SDK calls
      while preserving the existing provider and routes.
- [x] Use async SMTP sending and async template/file reads where supported.
- [x] Use `IFormFile.CopyToAsync`, async streams, and async file writes for uploads.
- [x] Review custom MVC result execution and remove the synchronous-I/O server option if
      the complete response path no longer needs it.
- [x] Treat synchronous-only framework contracts, such as a Serilog sink `Emit` method,
      as explicit exceptions rather than disguising them as async.

### Verification

- [x] Extend unit tests to cover async success, validation, exception, and cancellation
      paths.
- [x] Run MongoDB contract and integration tests through the async APIs.
- [x] Run Playwright journeys against the async controller paths.
- [x] Add a repository check that rejects new blocking task waits in request-path code,
      with documented exceptions if any remain.
- [x] Compare routes and representative HTTP responses before and after conversion.
- [x] Add an architecture check that keeps public I/O contracts async-only.

### Exit gate

- [x] Dynamic CMS controller-to-MongoDB request paths are async end to end.
- [x] Other high-value database and network request paths are async end to end.
- [x] Cancellation reaches the underlying I/O provider across every module, with the
      documented PayPal v1 transport limitation.
- [x] No unapproved `.Result`, `.Wait()`, or `GetAwaiter().GetResult()` remains in
      request-path code.
- [x] No duplicate synchronous repository or I/O-bound service API remains on `main`.
- [x] CPU-only code has not been wrapped in fake async.
- [x] All unit, integration, and browser tests pass without route or behaviour changes.

## Phase 7 — CI, documentation, and the v1.0 public release

### GitHub Actions jobs

- [x] `frontend`: pinned Node/npm, `npm ci`, and frontend freshness check.
- [x] `build`: SDK from `global.json`, restore, and Release build.
- [x] `unit-tests`: xUnit tests and coverage artifact.
- [x] `mongo-integration`: production and supported-current MongoDB images.
- [x] `demo-integration`: isolated public-example host, data, and safety coverage.
- [x] `end-to-end`: required Ubuntu matrix for Playwright Chromium, Firefox, and
      WebKit, with engine-specific failure artifacts.
- [x] `platform-smoke`: build and container-free tests on Ubuntu, Windows, and macOS.
- [x] Keep Windows/macOS browser journeys deferred: the v1.0 Linux matrix is the
      required browser environment, while container-free builds and tests provide
      informative portability coverage on standard Windows and macOS runners.

### Workflow controls

- [x] Run on pull requests and pushes to `main`.
- [x] Use minimal permissions, normally `contents: read`.
- [x] Cancel superseded pull-request runs.
- [x] Pin actions to immutable commit SHAs.
- [x] Keep deployment and production credentials out of test workflows.
- [x] Use only standard public-repository hosted runners; do not opt into paid larger
      runners or custom hosted images.
- [x] Keep Linux as the required release platform while exercising Windows and macOS
      through non-blocking portability jobs.
- [x] Do not claim a browser/OS combination until it has a repeatable CI environment;
      prefer the full cross-platform matrix where runner/container support permits it.

### Code style and static analysis

- [x] Audit the built-in .NET/Roslyn analyzers against the current source and record a
      clean, intentional baseline before increasing severities.
- [x] Enforce selected correctness and code-style diagnostics during normal builds,
      excluding generated and vendored source where appropriate.
- [x] Decide whether CI should treat all build warnings as errors after the analyzer
      baseline is repeatably clean.
- [x] Add `dotnet format --no-restore --verify-no-changes` to CI after a clean timed
      run completed in roughly nine seconds; analyzer enforcement remains the primary
      semantic quality gate.
- [x] If baseline formatting is needed, make it a dedicated mechanical change rather
      than mixing it with behavioural work.

### Public presentation

- [x] Add a concise architecture diagram.
- [x] Add screenshots or a short demo recording.
- [x] Document the feature set and intended audience honestly.
- [x] Document known limitations and compatibility versions.
- [x] Link the example, tests, CI, agent guidance, skills, and roadmap from the README.
- [x] Add a `main`-branch CI status badge at the top of the README.
- [x] Ensure repository topics and description represent the engine accurately.

### Branch and release transition

- [x] Create `main` from the preserved pre-public base during Phase 0.
- [x] Make `main` the default branch during Phase 0.
- [x] Merge the public-readiness phases through reviewed pull requests to `main`.
- [x] Require the agreed CI checks on `main`.
- [x] Retain `support/legacy-production`.
- [x] Retain `master` as a frozen alias of `support/legacy-production` through the v1.0
      release; revisit deletion after external links and consumers have moved to the
      named support branch.
- [x] Publish an annotated `v1.0.0` release.
- [x] Enable public-repository secret scanning and dependency alerts.
- [x] Enable private vulnerability reporting so the channel documented in
      `SECURITY.md` is available.
- [x] Configure Dependabot only after its update policy is documented.
- [x] Complete hosted verification of the candidate that replaces CMS jQuery 3.3.1
      and public jQuery 3.5.1 with exact jQuery 3.7.1, and Knockout 3.5.1 with exact
      Knockout 3.5.3.
- [x] Enable CodeQL with its default query suite, review the initial C#/JavaScript
      baseline without making it a required check, and decide whether vendored and
      generated paths need an explicit exclusion after seeing the real results.

### Exit gate

- [x] A fresh Linux clone passes every documented command.
- [x] Required CI is green.
- [x] No unresolved secret, copyright, or licensing blocker remains.
- [x] `main` is the public default branch.
- [x] `v1.0.0` is published with useful release notes.

## Phase 8 — Persistence contracts for v1.1

Before implementing EF Core, create a provider-independent contract suite that defines
the behaviour every persistence implementation must share.
The suite must start from the async-first contracts completed in Phase 6 rather than
designing a parallel synchronous provider surface.

- [x] CRUD semantics.
- [x] Ordering and movement semantics.
- [x] Search semantics, including case behaviour.
- [x] Pagination and stable ordering.
- [x] Identifier generation.
- [x] Nested-value identifier handling.
- [x] Provider-neutral child relationship handling.
- [x] Concurrency expectations.
- [x] Transaction boundaries.
- [x] Duplicate or idempotent operation behaviour where relevant.
- [x] Legacy MongoDB document compatibility where relevant.
- [x] A clear distinction between engine contracts and MongoDB implementation accidents.
- [x] Treat the async contracts introduced in Phase 6 as the primary provider API.
- [x] Keep provider contracts async-only; do not design a parallel synchronous surface.
- [x] Extract Common, Ecommerce, and Media repository behaviour into inherited provider
      suites rather than copying assertions into each provider.

### Exit gate

- [x] MongoDB passes the provider-independent contract suite.
- [x] Required relational semantics are documented.
- [x] Known semantic differences have explicit decisions.

## Phase 9 — EF Core implementations and provider packaging

This phase cannot start until the Phase 6 async contracts and Phase 8 provider contract
suite are complete. EF Core implementations are async-first from their first commit;
there is no intermediate synchronous EF DAL.

### Intended package shape

```text
src/Persistence/
  HypnoTech.EntityFrameworkCore/
  HypnoTech.EntityFrameworkCore.SqlServer/
  HypnoTech.EntityFrameworkCore.PostgreSql/

src/Cms/
  HypnoTech.Cms.EntityFrameworkCore/
src/Common/
  HypnoTech.Common.Dal.EntityFrameworkCore/
src/Ecommerce/
  HypnoTech.Ecommerce.Dal.EntityFrameworkCore/
src/Media/
  HypnoTech.Media.Dal.EntityFrameworkCore/
```

### Architecture

- [x] Shared EF project contains context abstractions, conventions, model contributors,
      transactions, and no concrete database driver.
- [x] Module EF projects implement the existing repository interfaces without taking a
      dependency on SQL Server, PostgreSQL, or MySQL provider packages.
- [x] Thin provider projects contain only the selected driver and provider-specific
      registration/configuration.
- [x] A host installs exactly one provider unless it intentionally uses multiple
      contexts.
- [x] Admin/CMS and public Website contexts and mappings remain independent. Admin owns
      write mappings and migrations; Website owns focused read repositories and may
      use different models, mappings, DAL, or ORM entirely.
- [ ] Site-specific migrations live in the Admin host/provider example because
      consumer CMS model types contribute to the EF model.
- [x] Normal EF startup applies pending migrations before any seed operation and fails
      startup if either step fails; no manual migration command is required by default.
- [x] Provide an explicit automatic-migration opt-out for controlled deployment
      pipelines without changing the migrate-then-seed default used by the examples.
- [x] Generic CMS repositories use `DbContext.Set<T>()` only after model registration,
      key, naming, search, ordering, relationship, and conversion requirements are
      defined.

Illustrative registration target:

```csharp
services.AddHypnoTechPostgreSql<DemoAdminDbContext>(connectionString);

services.UseEntityFrameworkCoreForCms();
services.UseEntityFrameworkCoreForCommon();
services.UseEntityFrameworkCoreForEcommerce();
services.UseEntityFrameworkCoreForMedia(configuration);
```

`AddHypnoTechSqlServer<DemoAdminDbContext>` is the equivalent SQL Server selection; the
module registrations and repositories do not change.

### Provider sequence

- [x] Re-check the current EF Core provider compatibility matrix at implementation time.
- [x] Build the provider-neutral relational layer first.
- [x] Prefer PostgreSQL as the first Linux-friendly target if an aligned provider exists
      for the chosen EF Core major version.
- [x] Add SQL Server with the same contract suite.
- [x] Defer MySQL/MariaDB from v1.1; evaluate Oracle versus Pomelo licensing, support,
      and EF-major compatibility only when a concrete consumer justifies it.
- [x] Defer NHibernate from v1.1. Keep repository contracts and provider suites capable
      of accepting a later NHibernate implementation without creating an ORM-neutral
      query abstraction or coupling Website mappings to Admin.
- [x] Run contract tests against real containerised databases, not only EF's in-memory
      provider.

### Exit gate

- [ ] The demo applications can select MongoDB or a supported EF provider through
      startup registration.
- [x] Each host brings in only its selected database driver.
- [x] All supported providers pass the common contract suite.
- [x] MongoDB remains supported and its existing persisted schema is unchanged.
- [x] Provider limitations and migration ownership are documented.

## Cross-cutting release rules

These apply throughout all phases:

- Preserve public namespaces, assembly identities, routes, static asset paths, and
  persisted names unless a deliberate breaking change and migration are approved.
- Separate mechanical moves, dependency updates, behaviour changes, and formatting
  changes into reviewable commits.
- Add or update tests before refactoring behaviour that is not already characterised.
- Use async APIs for genuine I/O and avoid fake async for CPU-only work.
- Propagate cancellation tokens through async request paths.
- Do not claim compatibility with an environment that CI or a recorded manual test does
  not exercise.
- Do not edit generated frontend bundles directly.
- Do not introduce CDNs for required runtime assets.
- Do not use floating npm or NuGet versions in build and test automation.
- Do not copy private-site material into examples, screenshots, fixtures, or docs.
- Do not expand payment-provider scope implicitly while touching ecommerce code.
- Update this document whenever scope, sequence, status, or a material decision changes.

## Open questions

| ID | Question | Needed by | Status |
| --- | --- | --- | --- |
| Q-001 | What exact engine commit is deployed in production? | Phase 0 | Answered: `95c9f6a` |
| Q-002 | What exact MongoDB server version is deployed? | Phase 0 | Answered: 4.0.3 |
| Q-003 | Is PolyForm Noncommercial plus separate commercial licensing acceptable? | Phase 1 | Answered: accepted |
| Q-004 | What copyright name and contact should appear in licence files? | Phase 1 | Answered: `Copyright © 2026 Christopher Clayton`; commercial contact through `github.com/camelofcode` |
| Q-005 | Must unchanged private sites ever consume reorganised public `main`, or may they remain pinned to the support branch? | Phase 3 | Answered: remain pinned |
| Q-006 | What neutral theme should the public demo use? | Phase 4 | Answered: Lantern House, a fictional independent arts venue |
| Q-007 | Should external contributions be accepted before a contributor-licensing process exists? | Phase 7 | Answered: no external contributions will be accepted |
| Q-008 | Which relational provider should be implemented first once the provider matrix is rechecked? | Phase 9 | Answered: PostgreSQL first, followed by SQL Server |
| Q-009 | How long should synchronous compatibility methods remain after public controllers move to async APIs? | Phase 6 | Answered: remove before v1.0; production remains pinned to `support/legacy-production` |
| Q-010 | Which verified Montserrat release should replace the 18 corrupted baseline font files, or should Montserrat be removed? | Phase 1 | Answered: remove them; all 18 were corrupted and unreferenced |
| Q-011 | Is the legacy PayPal/Braintree NuGet provenance gap acceptable until the deferred payment work, given that binaries are restored rather than vendored? | Phase 1 | Answered: accept as a documented risk until existing-PayPal work; no package binaries are committed |

## Risks and mitigations

| Risk | Impact | Mitigation | Status |
| --- | --- | --- | --- |
| MongoDB 4.0.3 compatibility regresses after a driver change | Production failure | Preserve driver 3.4.3 on support branch and run the exact 4.0.3 contract target before changes | Resolved for v1.0; the 4.0.3/8.0/9.0 contract matrix is green in required hosted CI |
| Secrets exist in compact Git history | Credential exposure on publication | Full-history scan, rotation, clean history, final pre-publication scan | Resolved for publication; the final 35-commit history and exact candidate-tree scans are clean |
| Third-party assets lack clear notices | Copyright/licensing problem | Inventory files and dependencies; preserve licences; add notices | Resolved for publication; unknown legacy package metadata is stated explicitly |
| Tracked Montserrat fonts are corrupted | Broken fonts or an unreviewed visual change | Remove only after confirming no authored/generated CSS reference and retain recovery through the baseline refs/bundle | Resolved; owner approved removal of all 18 unreferenced files |
| Legacy PayPal packages declare no licence metadata | Unclear provenance and public maintenance risk | Do not misstate the licence; document that packages are restored, then resolve provenance during later existing-PayPal work | Accepted known risk; binaries are not committed and provider replacement remains out of scope |
| Vendored CMS jQuery 3.3.1 predates jQuery 3.5.0 DOM-manipulation XSS fixes and is invisible to the npm lockfile | Public security alerts or exploitable DOM handling | Deliberately upgrade the self-hosted CMS copy to a tested supported release; retain exact version provenance and full browser coverage | Resolved with exact jQuery 3.7.1 and Knockout 3.5.3 dependencies; npm audit and all 11 hosted release checks, including Chromium, Firefox, and WebKit, are green on PR #15 and merged `main` |
| Physical moves break private site references | Consumer builds fail | Pin production to support branch; temporary smoke builds; optional shims | Open |
| Static registries make tests order-dependent | Flaky test suite | Serial collections and explicit reset of mutable public registries; make BSON and model registration idempotent | Resolved for current registrations; tests remain serial where global mappings are intentionally mutated |
| Example becomes a disguised private site | Privacy and portfolio risk | New theme, models, prose, seed data, and imagery | Resolved; Lantern House is wholly original and the example tree contains no private-site references |
| Playwright suite becomes slow or brittle | CI friction | Keep five journey-based tests, parallelise CI by browser, and retain engine-specific traces on failure | Mitigated; Chromium, Firefox, and WebKit are green in required hosted CI, with WebKit's approximately 20-minute cold job given a 40-minute hard ceiling for download variance |
| Async conversion changes routes or behaviour | Public regression | Characterisation tests first; preserve action names/routes; convert in bounded slices | Resolved for Phase 6; the full 77-test suite passes after sync removal |
| Async APIs are wrappers over blocking work | Misleading API and thread-pool pressure | Require native provider async APIs; prohibit `Task.Run` and blocking waits in request paths | Resolved; architecture guards reject blocking waits and synchronous public I/O contracts |
| EF providers support different EF major versions | Package conflicts or delayed providers | EF/Relational/SQL Server 10.0.14 and Npgsql EF 10.0.3 are pinned; both pass the same real-database contracts | Resolved for current PostgreSQL/SQL Server targets |
| Repository skills become decorative or stale | Misleading AI story | Start with two workflow-backed skills; test commands; evolve from use | Open |

## Decision log

| Date | ID | Decision | Status | Rationale |
| --- | --- | --- | --- | --- |
| 2026-11-06 | D-001 | All public-readiness work is confined to the engine repository. | Accepted | Esoteric and SelfHypnosis must not be touched or published. |
| 2026-11-06 | D-002 | Treat the private sites as external production consumers. | Accepted | Engine changes require compatibility consideration without site commits. |
| 2026-11-06 | D-003 | Additional ecommerce payment providers remain out of scope. | Accepted | They have a separate existing plan and are unrelated to public readiness. |
| 2026-11-06 | D-004 | Use this file as the living roadmap and delivery record. | Accepted | Keeps planned, completed, and outstanding work visible in the repository. |
| 2026-11-06 | D-005 | Preserve production with a tag and support branch before public work. | Accepted | Allows public `main` to evolve without forcing production dependency upgrades. |
| 2026-11-06 | D-006 | Keep self-hosted JavaScript and use Linguist vendor/generated classification. | Accepted | Fixes misleading statistics without weakening runtime reliability. |
| 2026-11-06 | D-007 | Use xUnit v3, Testcontainers, and a focused Playwright suite. | Accepted | Matches current experience and provides layered verification. |
| 2026-11-06 | D-008 | Use a provider-neutral EF implementation with thin driver packages. | Accepted | Avoids duplicated repositories and unnecessary database drivers. |
| 2026-11-06 | D-009 | Add an async-I/O modernisation phase before the v1.0 public release. | Accepted | Public CMS and other request paths should use genuine async provider APIs while preserving compatibility and avoiding fake async. |
| 2026-11-06 | D-010 | Complete async contracts while MongoDB is still the only DAL. | Accepted | Prevents building a synchronous EF Core DAL and then repeating the async conversion. |
| 2026-11-06 | D-011 | Build the example admin and website before the Playwright suite. | Accepted | Browser tests need a stable public example as their system under test. |
| 2026-11-06 | D-012 | Introduce `main` during Phase 0 and retain frozen `master` temporarily. | Accepted | Establishes the intended PR target early without breaking an unknown checkout that still pulls `master`. |
| 2026-11-06 | D-013 | Do not accept unsolicited external code contributions until source licensing and contributor terms are explicit. | Superseded by D-017 | The temporary restriction became a permanent no-contributions policy. |
| 2026-11-06 | D-014 | Identify Christopher Clayton as the sole engine author/copyright holder and remove the nonexistent HypnoTech company identity from project metadata. | Accepted | Anamaria Miehs made limited CSS contributions, does not claim engine authorship/copyright, and will retain appropriate credit in the separate production client sites. |
| 2026-11-06 | D-015 | Remove the 18 corrupted, unreferenced Montserrat font files rather than guessing at a replacement release. | Accepted | No engine CSS references them, so removal avoids publishing broken third-party binaries without an intended rendering change. |
| 2026-11-06 | D-016 | Licence public engine source under PolyForm Noncommercial 1.0.0 and offer separate written commercial licences. | Accepted | Allows noncommercial use, modification, and distribution while Christopher Clayton retains commercial use and licensing rights as copyright holder. |
| 2026-11-06 | D-017 | Do not accept external code or documentation contributions. | Accepted | Keeps the engine under a single copyright holder and avoids contributor rights complicating separate commercial licensing. |
| 2026-11-06 | D-018 | Treat the missing licence metadata in the restored legacy PayPal/Braintree NuGet packages as an explicit, non-blocking publication risk until later existing-PayPal work. | Accepted | The engine repository does not redistribute those binaries, the uncertainty is disclosed, and replacing payment providers remains out of scope. |
| 2026-11-06 | D-019 | Use xUnit v3's Microsoft Testing Platform v2 runner and Microsoft's MTP coverage extension rather than the legacy VSTest/coverlet collector path. | Accepted | .NET 10 supports the unified runner, xUnit 4 defaults to MTP v2, and the current coverlet collector does not support MTP v2. |
| 2026-11-06 | D-020 | Fix the four Phase 2 MongoDB characterisation failures while retaining MongoDB.Driver 3.4.3 and testing MongoDB 4.0.3, 8.0.32, and 9.0.4. | Accepted | Empty searches, null child IDs, boundary moves, and repeated startup should be safe; no persisted format or production version floor changes. |
| 2026-11-06 | D-021 | Prefer a separately authorised blue/green logical BSON migration to a pinned Podman-hosted MongoDB release over successive in-place production upgrades. | Accepted for future planning | Keeps the existing native deployment unchanged until cutover, preserves a simple rollback point, and retains the same low-cost capacity class; the container route proceeds only if rehearsal shows no meaningful regression. |
| 2026-11-06 | D-022 | Keep unchanged production consumers pinned to `support/legacy-production` while public `main` adopts the functional source layout. | Accepted | Avoids private-site edits and path shims while the preserved branch remains an exact, buildable production dependency. |
| 2026-11-06 | D-023 | Use Lantern House, a fictional independent arts venue, for the public example. | Accepted | It can demonstrate editorial, programme, media, and ecommerce features with wholly original synthetic content and branding. |
| 2026-11-06 | D-024 | Keep the Phase 4 checkout as a fail-closed pricing demonstration and defer a reusable mock provider to the payment plan. | Accepted | Basket and postage calculations remain demonstrable, while payment/completion/cancellation routes return 410 before provider, order, email, download-code, or fulfilment code can run. |
| 2026-11-07 | D-025 | Make public `main` async-only for repository and I/O-bound service contracts before v1.0. | Accepted | Both production sites are pinned to the preserved support branch, so duplicate sync implementations add maintenance risk without protecting an in-solution caller. CPU-only APIs stay synchronous and Serilog's required sync contract remains queued. |
| 2026-11-07 | D-026 | Run public CI only on standard GitHub-hosted runners and keep retained artifacts small and short-lived. | Accepted | Standard public runners are free and unlimited, while larger runners are billed and artifact storage has a separate allowance. |
| 2026-11-07 | D-027 | Publish the first stable public baseline as `v1.0.0`, not `v0.1.0`. | Accepted | The engine is production-proven and extensively tested; v1 establishes the supported public contract while the legacy production API remains on its support branch. |
| 2026-11-07 | D-028 | Implement EF Core for PostgreSQL and SQL Server first; defer MySQL and NHibernate. | Accepted | One shared EF repository layer plus thin drivers demonstrates portability without doubling ORM mappings, repositories, transactions, and tests before a consumer needs them. |
| 2026-11-07 | D-029 | Keep Admin/CMS and public Website persistence mappings independent. | Accepted | The public host must remain free to use focused read models and a different DAL or ORM rather than inheriting edit-oriented Admin mappings. |
| 2026-11-07 | D-030 | Defer public NuGet publication until a dedicated package audit and local-feed consumer rehearsal pass. | Accepted | nuget.org versions are immutable; the GitHub v1 source release should not force premature package IDs or dependency boundaries. |
| 2026-11-07 | D-031 | Use Dependabot for security fixes only across NuGet, npm, and GitHub Actions. | Accepted | The pinned repository is maintained by one developer; routine version PRs create review and CI churn without a concrete upgrade need, while automated vulnerability updates remain valuable and are never auto-merged. |
| 2026-11-07 | D-032 | Keep CodeQL non-blocking and scan authored C#, JavaScript, and Actions through a pinned advanced workflow. | Accepted | Default setup produced 104 mostly duplicated alerts from third-party and generated JavaScript and cannot apply a repository-local exclusion file in this personal repository. The advanced workflow retains the default query suite while excluding only `lib` trees and generated `wwwroot/js` outputs. |
| 2026-11-07 | D-033 | Define persistence behaviour once in inherited provider-neutral suites while retaining storage-format compatibility in provider-specific tests. | Accepted | Plain contract models and public async interfaces prevent BSON or EF accidents becoming universal requirements; the Mongo adapter proves the same facts on 4.0.3, 8.0.32, and 9.0.4. |
| 2026-11-07 | D-034 | Make EF Core startup apply migrations before seeding by default, with an explicit opt-out for controlled deployment pipelines. | Accepted | A newly configured HypnoTech host should become usable without a manual migration command, while operators who separate schema deployment retain a supported path. |
| 2026-11-07 | D-035 | Pin EF Core/Relational/SQL Server at 10.0.14 and Npgsql EF/Npgsql at 10.0.3 for the first relational implementation. | Accepted | These stable .NET 10-compatible releases allow one driver-free repository layer; real PostgreSQL and SQL Server contracts prove the aligned graph rather than relying only on declared version ranges. |
| 2026-11-07 | D-036 | Keep application-level `IHypnoTechStartupSeeder` distinct from the CMS `ISeedRepository`. | Accepted | The startup hook owns migrate-then-seed ordering, while the existing repository remains the lower-level operation for loading a named JSON seed into an empty mapped store. |

## Work log

Add new rows chronologically. Include concrete verification evidence rather than only
stating that work was done.

| Date | Phase | Work completed | Verification/evidence | Commit or PR |
| --- | --- | --- | --- | --- |
| 2026-11-06 | Planning | Audited engine repository structure, Git baseline, solution, project dependencies, frontend asset layout, CMS dynamic controller/repository design, private consumer references, and current test/CI/documentation gaps. | Clean `master` at `95c9f6a`; 21 .NET projects; 10 commits; no tags/tests/workflows/root licence; source byte and path analysis recorded above. | Uncommitted planning audit |
| 2026-11-06 | Planning | Researched current guidance for Codex repository instructions/skills, GitHub Linguist and Actions, software licensing constraints, EF Core provider packaging, MongoDB driver compatibility, Testcontainers, and Playwright. | Findings incorporated into phase gates and open questions. | Uncommitted planning audit |
| 2026-11-06 | Planning | Created this living public-readiness plan and tracking structure. | File review and Markdown structure check. | This change |
| 2026-11-06 | Phase 0 | Recorded the live engine and database baseline. | Both live sites use engine commit `95c9f6a` and MongoDB Server 4.0.3; GitHub repository was private with `master` as default at capture. | This change |
| 2026-11-06 | Phase 0 | Created the portable Git bundle backup outside the engine repository. | `git bundle verify` reports complete history for `master` and `dotnet10-upgrade-plan`; size 6.0 MB; SHA-256 `8b963e6c86e214acca9532912bbf0b4b29ff5c579f2705d7f045cadd60f17cc6`. | Backup file, not committed |
| 2026-11-06 | Phase 0 | Verified authenticated GitHub CLI and Git access before remote changes. | Active `camelofcode` account; token scopes include `repo` and `workflow`; repository permissions report `admin` and `push`; authenticated `git ls-remote` succeeded. | This change |
| 2026-11-06 | Planning | Audited current asynchronous usage and added a late v1.0 async-I/O modernisation phase. | Of 287 tracked C# files, only three contained actual `await`; primary blocking I/O areas and compatibility rules are recorded in Phase 6. | This change |
| 2026-11-06 | Planning | Made example-to-Playwright and Mongo-async-to-EF ordering explicit. | Ordering constraints and phase prerequisites recorded; decisions D-010 and D-011 accepted. | This change |
| 2026-11-06 | Phase 0 | Published immutable baseline refs and introduced the future default branch. | Remote tag `production-baseline-2026-11-06`, `support/legacy-production`, and `main` point to `95c9f6a`; GitHub default changed to `main`; frozen `master` retained. | This change |
| 2026-11-06 | Phase 0 | Created and published `public-readiness` from the preserved baseline. | Local branch tracks `origin/public-readiness`; roadmap is its first change. | This change |
| 2026-11-06 | Phase 0 | Captured and verified the production build/toolchain baseline. | .NET SDK 10.0.401; Node 24.18.0; npm 11.16.0; 21-project Release build succeeded with zero warnings/errors; all four frontend outputs current; fingerprints recorded in `docs/baselines/production-2026-11-06.md`. | This change |
| 2026-11-06 | Phase 0 | Confirmed the verified Git bundle was copied to durable private backup storage. | User confirmed the external backup; the workspace copy remains available for now. | External backup, not committed |
| 2026-11-06 | Phase 1 | Scanned all reachable Git history and the current working tree for secrets, then performed supplemental path-only configuration/credential review. | Gitleaks 8.30.1 checksum verified; initial 10 commits/~26.94 MB, working tree/~85.07 MB, and post-foundation 11 commits/~26.99 MB scanned with no leaks; command and scope recorded in `docs/audits/publication-history-2026-11-06.md`. | This change |
| 2026-11-06 | Phase 1 | Audited bundled browser assets, fonts, npm build dependencies, and all unique direct/transitive NuGet packages. | `THIRD_PARTY_NOTICES.md` records versions, licences, locations, unknown snapshot versions, and the PayPal/Braintree metadata gap; existing notices retained and common MIT/OFL texts added. | This change |
| 2026-11-06 | Phase 1 | Added public-facing repository documentation, compatibility guidance, Linguist classification, and conservative formatting defaults. | Local `git check-attr` marks vendored/generated samples correctly; authored bytes outside those paths are ~396 KB C# versus ~75 KB JavaScript plus ~8.6 KB build tooling; no runtime asset was moved to a CDN. | This change |
| 2026-11-06 | Phase 1 | Added compact `AGENTS.md` and the initial verification and CMS compatibility-review skills. | Both `SKILL.md` files pass the skill-creator `quick_validate.py` validator; triggers and output requirements are explicit. | This change |
| 2026-11-06 | Phase 1 | Identified pre-existing corruption in every tracked Montserrat font rather than silently replacing unknown binaries. | All 18 `.ttf` files fail `fc-query`; font-table bytes contain UTF-8 replacement sequences and match the preserved baseline. | This change |
| 2026-11-06 | Phase 1 | Ran the repository verification workflow across the Phase 1 foundation change. | `git diff --check` passed; the 21-project Release build passed with zero warnings/errors; all four frontend outputs were current; both skills validated. | This change |
| 2026-11-06 | Phase 1 | Centralised engine authorship and copyright metadata as `Christopher Clayton` / `Copyright © 2026 Christopher Clayton`, removing the proposed-but-never-formed HypnoTech company and engine co-author metadata. | Root `Directory.Build.props` is authoritative; all 21 projects evaluate to the same author/company/copyright values; Release build passed with zero warnings/errors. | This change |
| 2026-11-06 | Phase 1 | Removed all corrupted Montserrat font snapshots after confirming they were unreferenced. | No `Montserrat` or frontend-font path reference exists in authored/generated engine CSS; frontend freshness checks pass; all 18 files remain recoverable from the preserved baseline and private bundle. | This change |
| 2026-11-06 | Phase 1 | Added the accepted source and commercial-licensing model and made the no-contributions policy explicit. | Root `LICENSE.md` is byte-for-byte identical to the official PolyForm Noncommercial 1.0.0 download; `NOTICE`, `COMMERCIAL-LICENSE.md`, README, package metadata, and contribution guidance identify Christopher Clayton and the separate commercial route; a test NuGet package contains the licence, notice, and README. | This change |
| 2026-11-06 | Phase 2 | Added four xUnit v3/Microsoft Testing Platform projects, centrally pinned test dependencies, informational coverage, and a repository-local characterisation-test authoring skill. | Skill validator passes; 25-project Release build passes with zero warnings/errors; the CMS report covers 629/2577 lines (24.41%) with no threshold. | This change |
| 2026-11-06 | Phase 2 | Added dynamic CMS mapping, controller, route, form, service, upload, Common helper, and architecture characterisation tests using synthetic fixtures and isolated static state. | Focused CMS, Common, and architecture suites run independently and as part of the solution. | This change |
| 2026-11-06 | Phase 2 | Added Testcontainers repository contracts for production MongoDB 4.0.3, supported MongoDB 8.0.32, and current MongoDB 9.0.4, running through the rootless Podman API socket without `podman-docker`. | All three images start and the suite covers CRUD, paging, sorting, search, ordering, BSON, children, list/gallery IDs, select lists, seed, DI, legacy documents, and repeated registration; initial failure modes were captured before correction. | This change |
| 2026-11-06 | Phase 2 | Corrected the four failures exposed by the initial MongoDB contracts without upgrading the driver or changing persisted data. | Empty-field search returns no rows; null child IDs remain unset; ordering boundaries are no-ops; repeated Common/Ecommerce/Media startup is idempotent. All behaviours pass against MongoDB 4.0.3, 8.0.32, and 9.0.4. | This change |
| 2026-11-06 | Future operations planning | Added a non-executable production MongoDB migration runbook covering logical BSON transfer, a pinned Podman container, rehearsal, verification, cutover, and rollback. | Vendor upgrade, dump/restore, platform, Podman, and production guidance reviewed; no server connection or production change was made. | This change |
| 2026-11-06 | Phase 3 | Reorganised all engine source projects under `Cms`, `Common`, `Ecommerce`, `Media`, `Persistence`, and `Frontend` without changing namespaces, assembly names, APIs, behaviour, or asset bytes. | Mechanical commit changes 827 files and is overwhelmingly detected as 100% renames; only solution, project-reference, npm-workspace, builder, architecture-test path, and documentation references changed content. | `5e17d2a` and this change |
| 2026-11-06 | Phase 3 | Completed aggregate verification and isolated compatibility smoke builds. | The 25-project Release build passed with 0 warnings/errors after suppressing only `NU1900` from the unavailable NuGet audit service; all four frontend outputs are current; two complete Podman-backed runs passed 46/46 tests; all five static-web-asset base paths remain `_content/HypnoTech.*`; temporary Esoteric and SelfHypnosis copies compiled against `support/legacy-production` with 0 warnings/errors and `SkipFrontendBuild=true`, leaving the real private repositories untouched. A separate historical Esoteric npm 6 asset attempt reached the frontend step after compiling every .NET assembly, then failed on its existing lockfile parser (`browserslist`) rather than an engine source error. | This change |
| 2026-11-06 | Phase 4 | Added the original Lantern House example models, admin, public website, seed data, self-hosted artwork/audio/video, Podman configuration, and developer guide. | The documented root-level seed command starts the real admin host, populates a fresh MongoDB 8.0 database, hashes `HypnoTechDemo!2026` with bcrypt, and exits; authenticated admin and representative public/detail/media routes return successfully. | This change |
| 2026-11-06 | Phase 4 | Demonstrated dynamic CMS, navigation/configuration, Media, and Ecommerce behavior without allowing a real transaction. | Admin exposes generated Articles, Artists, and Events controllers plus Media/Ecommerce settings; the public site uses database-backed navigation, server-calculated basket/postage/tracked totals, and original responsive rendering; every provider/payment lifecycle route returns HTTP 410 before MVC. | This change |
| 2026-11-06 | Phase 4 | Added an isolated Testcontainers example suite and stable future browser-test hooks. | Nine xUnit v3 tests pass through rootless Podman against MongoDB 8.0.32, covering host seeding, representative collections, bcrypt-only credential storage, blocked payment routes, and allowed basket/checkout calculation routes. Desktop and 390 px browser checks passed, including the mobile menu. | This change |
| 2026-11-06 | Phase 4 | Completed aggregate verification and the final error-page smoke check. | The 29-project Release build passes with zero warnings/errors; all four frontend outputs are current; the full Podman-backed solution passes 55/55 tests including MongoDB 4.0.3, 8.0.32, and 9.0.4; `git diff --check` passes; and an unknown public route returns the original example 404 page with HTTP 404 rather than 500. | This change |
| 2026-11-06 | Phase 4 | Added a one-command Linux launcher for manual review. | `./examples/run-demo.sh` starts the pinned MongoDB Podman container, waits for readiness, seeds safely, launches both hosts, prints URLs and credentials, retains scoped logs, and stops the .NET processes on Ctrl+C while preserving the named demo database for the next run. | This change |
| 2026-11-06 | Phase 4 | Corrected the example host integration to exercise the platform audio, cart, and checkout tag helpers; enabled CMS-managed YouTube video with an original accessible lightbox; made synthetic uploaded fixtures available through one shared admin/public upload store; and removed the last visible obsolete company copyright from CMS layouts. | The 29-project Release build succeeds; all four frontend outputs are current; 55/55 Release tests pass against rootless Podman including MongoDB 4.0.3, 8.0.32, and 9.0.4; live browser checks confirm the admin image preview, audio playback state, YouTube lightbox, cart count/details, and UK checkout total; uploaded SVG and MP4 endpoints return HTTP 200/206; payment remains HTTP 410; and Ctrl+C leaves both application ports closed. | This change |
| 2026-11-06 | Phase 4 | Added the platform's progressive partial-page navigation to Lantern House, kept the audio player and basket counter outside the replaceable content region, exposed CMS tracks through the shared `player:playTrack` event, and retained checkout as an intentional full-document boundary. | Integration coverage proves normal Articles and Checkout requests return full HTML while opted-in Articles and Cart requests return the JSON fragment/metadata contract. Live browser checks started a CMS track, navigated Media → Stories → Shop → product → Cart with playback remaining in its Pause state, populated the cart after partial insertion, then crossed the separate Checkout document boundary. | This change |
| 2026-11-06 | Phase 4 | Added public guides for fluent CMS model/admin setup and frontend integration, and moved Lantern House's authored host JS/SCSS into its own frontend workspace with committed development/minified output. | `npm run check:frontend` reports all five asset projects current; `git diff --check` passes; the 29-project Release build succeeds with no compile errors (NuGet's temporarily unreachable vulnerability feed emits only `NU1900`); and the complete Podman-backed Release suite passes 55/55 against MongoDB 4.0.3, 8.0.32, and 9.0.4. | This change |
| 2026-11-06 | Phase 4 | Replaced Lantern House's cart-page-first interaction with a persistent slide-out basket, retained `/Cart` as a progressive-enhancement fallback, and corrected the example's application boundary so only Admin owns fluent edit mappings and the CMS MongoDB repository. Website now uses focused public read repositories over `HypnoTech.MongoDb` and has no CMS/admin project dependency. | Compiled-reference and rendered-page integration coverage prove the public Models/Website projects do not reference CMS web assemblies and the public host retains the core frontend bundle while omitting the CMS bundle. All five frontend outputs are current; the Release build succeeds; and the complete Podman-backed Release suite passes 56/56. Live browser checks confirmed in-place basket opening, add/remove state shared with direct `/Cart`, player/basket mutual exclusion, a full-document checkout boundary, and no CMS script or TinyMCE console error. | This change |
| 2026-11-06 | Phase 5 | Added a focused Playwright for .NET/xUnit suite with a shared Testcontainers MongoDB 8.0 fixture, real Admin and Website processes on dynamic ports, isolated browser contexts, and a one-command rootless-Podman runner. | Five Chromium journeys cover authentication/authorization, dynamic CMS validation and CRUD/search/order/duplicate/remove behaviour, rich text, an uploaded image reaching the public site, Media/player/gallery content, server-calculated basket/postage totals, the checkout safety boundary, and mobile navigation. Repeated focused runs pass 5/5 without browser errors. | This change |
| 2026-11-06 | Phase 5 | Used the new browser-error gate to find and correct unconditional TinyMCE startup on the CMS login layout. | Authenticated CMS pages still initialise TinyMCE; layouts without the TinyMCE core now safely skip its initialiser and plugins. All five generated frontend outputs are current, and the login/logout journey records no console or page errors. | This change |
| 2026-11-06 | Phase 5 | Completed aggregate verification and marked the browser phase complete. | The 30-project Release build has no compiler errors or warnings (NuGet's unavailable vulnerability feed emitted only `NU1900`); all five frontend outputs are current; `git diff --check` passes; and the complete concurrent rootless-Podman solution passes 61/61, including MongoDB 4.0.3, 8.0.32, 9.0.4, and the five Chromium journeys. | This change |
| 2026-11-06 | Phase 5 | Generalised the Playwright runner and artifacts across Chromium, Firefox, and WebKit, and made the eventual CI matrix explicit. | Firefox passes 5/5 natively; WebKit passes 5/5 in Playwright's supported Ubuntu image. The WebKit run exposed that its stricter localhost handling rejected the CMS `Secure` cookie over HTTP, so the harness now serves Admin over test-only ephemeral HTTPS without changing production cookie policy. Phase 7 now requires all three engines on Ubuntu and plans non-blocking Windows/macOS portability coverage where hosted-runner container support permits it. | This change |
| 2026-11-06 | Phase 6 | Converted the complete CMS request surface and its Common-backed admin dependencies to native async I/O while retaining the existing synchronous repository/service API for compatibility. | Dynamic CRUD, ordering, children, select lists, seeding, uploads, settings, users, authentication, accounts, password reset, backups, email/SMTP, uploaded-file reads, and navbar-cache HTTP calls now propagate `RequestAborted` to MongoDB, streams, files, SMTP, and HTTP. Pure mapping, validation, hashing, image transformation, metadata, and render-only actions remain synchronous. Sync members keep native sync implementations rather than blocking on async. | This change |
| 2026-11-06 | Phase 6 | Added async characterisation coverage and a blocking-wait architecture guard for the converted layers. | Release build passes with zero warnings/errors; CMS unit tests pass 25/25; MongoDB contracts pass async CRUD/paging/ordering/children/select-list/seed behavior on 4.0.3, 8.0.32, and 9.0.4; the complete Chromium/Podman solution passes 68/68 including the final architecture guard. | This change |
| 2026-11-06 | Phase 6 | Converted Ecommerce catalogue, basket, checkout, postage, order, download-code, country, email, tag-helper, and settings-controller request paths to native async repository and provider APIs. | Existing routes, responses, calculation rules, flash messages, BSON names, and payment-provider scope remain unchanged. Constructor-time MongoDB reads were removed. PayPal v1 calls are awaited by all engine request paths; its two retained synchronous compatibility members are the documented exception because the obsolete SDK exposes no synchronous transport. | This change |
| 2026-11-06 | Phase 6 | Added Ecommerce async repository characterisation and extended the blocking-wait guard to the Ecommerce source tree. | The catalogue, postage, order lifecycle, download-code depletion, and cancellation contracts pass against MongoDB 4.0.3, 8.0.32, and 9.0.4 without contacting a payment provider. The architecture suite passes with only the explicit PayPal v1 compatibility adapter allowlisted. | This change |
| 2026-11-06 | Phase 6 | Added an in-process PayPal REST double without a new test dependency and completed aggregate verification for the Ecommerce async slice. | A local Kestrel server characterises OAuth, create success, create validation failure, execute success, request totals, and response mapping without contacting live or sandbox PayPal. All 30 projects build in Release with zero warnings/errors; CMS unit tests pass 25/25, Common unit tests 10/10, architecture tests 3/3, MongoDB contracts 16/16 across all three targets, demo integration tests 13/13, and the real Chromium/Podman journeys 5/5. | This change |
| 2026-11-06 | Phase 6 | Converted Media audio, video, and photo-album repositories and services, the player tag helper, and the public example Media controller to native async MongoDB request paths. | Existing synchronous APIs remain native compatibility members; collection/BSON names, album date ordering, URL lookup, audio enrichment, feature gates, DI, player JSON/markup, routes, and partial-navigation behavior are unchanged. The architecture guard now covers Media. | This change |
| 2026-11-06 | Phase 6 | Added Media sync/async repository and service characterisation and completed aggregate verification for the slice. | All 30 projects build in Release with zero warnings/errors; CMS unit tests pass 25/25, Common unit tests 10/10, architecture tests 3/3, MongoDB contracts 18/18 across 4.0.3, 8.0.32, and 9.0.4, demo integration tests 14/14, and the Chromium/Podman journeys 5/5 including Media navigation, player, gallery, and video behavior. | This change |
| 2026-11-06 | Phase 6 | Closed the remaining public-example async gap across Articles, Artists, Events, Home, Media, Shop, focused read repositories, and artist-based audio enrichment. | Every database-backed Lantern House action now propagates `RequestAborted`; independent Home and Media reads run concurrently; an additive Media enrichment async hook preserves sync-only consumer compatibility; render-only Cart remains synchronous. | This change |
| 2026-11-06 | Phase 6 | Added the `author-async-request-paths` repository skill and extended async verification to the example request surface. | The skill records controller inheritance, generated CMS, public/admin dependency boundaries, provider-neutral DAL, native async, cancellation, concurrency, and retained-sync conventions. Its validator passes; the 30-project Release build has zero warnings/errors; focused architecture tests pass 3/3; focused demo integration tests pass 15/15; and the complete Podman/Chromium solution passes 76/76. | This change |
| 2026-11-06 | Phase 6 | Completed the final Frontend async slice and removed the server-wide synchronous-I/O escape hatch. | Uploaded image/media actions and navbar startup/refresh now propagate cancellation through native async file and MongoDB APIs; partial JSON uses async response writes; host asset hashes are prepared once during registration; Kestrel and IIS no longer enable synchronous I/O. Routes, range responses, navbar order, asset URLs, and partial JSON remain unchanged. | This change |
| 2026-11-06 | Phase 6 | Isolated the synchronous Serilog sink contract behind pinned `Serilog.Sinks.Async` 2.1.0 and closed the Phase 6 exit gate. | A bounded 1,000-event warning queue drops rather than blocks when full; dependency/licence and compatibility documentation are current. All five frontend outputs are current; the 30-project Release build passes with zero warnings/errors; architecture tests pass 4/4; MongoDB contracts pass 19/19 across 4.0.3, 8.0.32, and 9.0.4; demo integration passes 17/17; and the complete Podman/Chromium solution passes 80/80. | This change |
| 2026-11-07 | Phase 6 | Removed the duplicate synchronous repository and I/O-bound service surface from public `main`. | CMS, Common, Ecommerce, Media, Frontend, email, PayPal, MongoDB helpers, public-example repositories, audio enrichment, and demo seeding are async-only; seeding now runs as an async hosted service. CPU-only APIs remain synchronous, and Serilog `Emit` remains the sole framework-mandated queued sync-I/O contract. Production sites remain untouched on `support/legacy-production`. | This change |
| 2026-11-07 | Phase 6 | Updated repository guidance and added an architecture guard against synchronous public I/O contracts. | The async authoring skill validates; Release builds with 0 warnings/errors; architecture tests pass 5/5; and the complete rootless-Podman/Chromium solution passes 77/77. Three compatibility-only tests were removed while behavioral coverage continues through async characterisation tests. | This change |
| 2026-11-07 | Phase 7 preparation | Strengthened test-boundary guidance and added the `test-external-http-integrations` skill. | The guidance now defines when to add unit, integration, architecture, and browser coverage; prefers observable outcomes over mock-call trivia; and standardises safe local protocol stubs without live/sandbox traffic. All five repository skills validate, the 30-project Release build passes with 0 warnings/errors, and demo integration tests pass 15/15 through rootless Podman. | This change |
| 2026-11-07 | Phase 7 preparation | Added a measured code-style and static-analysis gate to the Phase 7 plan. | Built-in Roslyn analyzers will be baselined and selected diagnostics enforced before considering warnings-as-errors; `dotnet format` remains optional unless a timed no-restore CI run proves fast and stable. Formatting churn must remain separate from behavioural changes. | This change |
| 2026-11-07 | Phase 7 | Added the GitHub Actions release-candidate workflow, public CI documentation, and a `main`-branch README status badge. | The workflow uses immutable official-action SHAs and standard Ubuntu, Windows, and Intel macOS labels; PyYAML and actionlint 1.7.12 accept it; frontend freshness passes; coverage-producing unit/architecture suites pass 40/40; MongoDB and demo integration suites pass 32/32 through rootless Podman; and the Chromium journeys pass 5/5 locally. | This change |
| 2026-11-07 | Phase 7 | Re-ran the publication security gate against the complete reachable history and an archive of the exact committed candidate tree. | Gitleaks 8.30.1 found no leaks across 35 commits/~27.8 MB or the tracked tree/~20.7 MB. Four matches from a broad developer-directory scan were confined to ignored Playwright package output under `bin/` and are absent from Git and the publication tree. | This change |
| 2026-11-07 | Phase 7 | Published the repository with `main` as its default branch and opened the public CI pull request. | The repository is public at `camelofcode/HypnoTech`; `support/legacy-production` remains available; PR #12 merged after its required hosted checks passed; and the publication scan preceded the visibility change. | This change |
| 2026-11-07 | Phase 7 | Completed the first successful hosted release-candidate run on standard free public runners. | Run `42321468198` passed all 11 jobs: frontend freshness, Release build, 40 unit/architecture tests with coverage, 19 MongoDB contracts across 4.0.3/8.0/9.0, 15 example integration tests, five journeys in each of Chromium/Firefox/WebKit, and informative Ubuntu/Windows/macOS builds. Container-backed jobs use Ubuntu 22.04 because Ubuntu 24.04's hosted kernel is incompatible with the pinned MongoDB images; WebKit's cold dependency install made its successful job approximately 20 minutes. | This change |
| 2026-11-07 | Phase 7 | Established the v1 formatting and analyzer baseline. | A dedicated mechanical commit normalised 59 C# files with no non-whitespace source change beyond removing five UTF-8 BOMs; `dotnet format --verify-no-changes` is clean and takes roughly nine seconds. Minimum/Recommended analysis identified 18/233 legacy advisories; the clean .NET 10 Default warning set is enforced as errors while the wider backlog remains documented. | This change |
| 2026-11-07 | Phase 7 | Added the public architecture/presentation and release documentation. | README feature/audience text and authentic local-demo screenshots show the Website and authenticated generated CMS; the architecture diagram records the Admin/Website boundary; limitations, code-quality, NuGet packaging, and v1.0.0 release notes are explicit. | This change |
| 2026-11-07 | Phase 7 | Hardened public repository governance. | Eight Linux release checks are required on `main` for administrators; force-push/deletion are disabled; secret scanning, push protection, Dependabot security updates, private vulnerability reporting, accurate topics, and a portfolio description are enabled. Routine version-update PRs are disabled across all ecosystems; security updates are never auto-merged. | GitHub repository settings plus this change |
| 2026-11-07 | Phase 7 | Audited the CodeQL and vendored-browser-library boundary before enabling code scanning. | CodeQL is available but not configured or merge-blocking. The actual bundles use CMS jQuery 3.3.1, public jQuery 3.5.1, and Knockout 3.5.1; upstream evidence shows Knockout remains maintained at 3.5.3, while the CMS jQuery snapshot predates the 3.5.0 XSS fixes and must be resolved before v1.0. | This change |
| 2026-11-07 | Phase 7 | Replaced the obsolete vendored jQuery and Knockout core snapshots with exact, integrity-locked npm development dependencies while preserving committed self-hosted bundles. | Clean `npm ci` reports zero vulnerabilities and enforces the fail-closed install-script allowlist; all five frontend bundles are current; the 30-project Release build has zero warnings/errors; formatting is clean; the complete Podman/Chromium solution passes 77/77 and Firefox passes 5/5. Hosted WebKit is the remaining authoritative browser gate. | This change |
| 2026-11-07 | Phase 7 | Corrected a WebKit-only CMS browser-test race exposed by two post-merge `main` runs. | Traces showed the public Articles request starting before the edit POST completed. The journey now waits for the exact server-rendered Added/Updated success alert after each save; the complete stateful Chromium suite passes repeatedly and again after the dependency upgrades. | This change |
| 2026-11-07 | Phase 7 | Merged the release candidate after full hosted verification. | PR #15 and the resulting `main` push both passed all 11 CI jobs, including exact frontend dependency installation, Release build/formatting, the layered unit/integration suites, the MongoDB 4.0/8.0/9.0 matrix, Chromium/Firefox/WebKit journeys, and Ubuntu/Windows/macOS portability builds. GitHub now reports C# as the primary repository language. | PR #15 |
| 2026-11-07 | Phase 7 | Triaged the first non-blocking CodeQL default-setup scan and hardened authored request paths. | The initial scan correctly exposed third-party/generated JavaScript noise plus authored findings: uploaded-file traversal/logging, missing antiforgery validation, and unsafe CMS list URL construction. The implementation now confines upload reads, avoids logging request-derived paths, validates login/basket/checkout posts, supplies tokens through the public Ajax contract, and encodes CMS navigation values. Focused Common tests pass 12/12, Chromium journeys pass 5/5, and the runtime-aware Podman wrapper passes the complete 79-test solution. | This change |
| 2026-11-07 | Phase 7 | Passed the complete documented release sequence from a fresh Linux clone of the final PR commit. | Restore and exact `npm ci` succeed with zero npm vulnerabilities; Release build has zero warnings/errors; formatting and all five generated bundles are current; the Chromium journey passes 5/5; and the runtime-aware rootless-Podman wrapper passes all 79 solution tests. | PR #19 |
| 2026-11-07 | Phase 7 | Established a clean authored-source CodeQL baseline. | The pinned advanced workflow runs the default query suite across C#, JavaScript/TypeScript, and Actions while excluding only third-party `lib` trees and committed `wwwroot/js` build outputs. All three hosted analyses pass and PR #19 has zero open code-scanning alerts after the authored findings were fixed. CodeQL remains informative rather than merge-required for v1.0. | PR #19 |
| 2026-11-07 | Phase 7 | Corrected the end-to-end public host to use test-only HTTPS after antiforgery hardening exposed WebKit's strict secure-cookie handling. | Chromium and Firefox permit `Secure` cookies on plain localhost, but WebKit correctly withheld the public antiforgery cookie over HTTP. The fixture now serves both Admin and Website with the same ephemeral certificate, preserving production cookie policy and testing the intended HTTPS deployment boundary. | PR #19 |
| 2026-11-07 | Phase 7 | Published the first stable public release and closed the Phase 7 exit gate. | PR #19 merged as `ceec378`; the exact `main` commit passed all 11 CI jobs and the C#, JavaScript/TypeScript, and Actions CodeQL analyses. Obsolete default-setup analyses were removed, the one containment-guard false positive was dismissed with rationale, and the repository has zero open code-scanning alerts. Annotated tag `v1.0.0` and the GitHub Release were published with the prepared release notes. | [`v1.0.0`](https://github.com/camelofcode/HypnoTech/releases/tag/v1.0.0) |
| 2026-11-07 | Phase 8 | Added inherited provider-neutral CMS, Common, Ecommerce, and Media repository contracts and removed their duplicated Mongo assertions. | Plain CMS models plus every current module repository interface exercise CRUD, generated/nested IDs, children, select lists, idempotent seeding, users/accounts, config, countries, email templates, logs, backups, navbar, catalogue, postage, orders, download codes, Media queries, search, paging, sorting, ordering, and cancellation. MongoDB passes 31/31 contract and storage checks on 4.0.3, 8.0.32, and 9.0.4; the 31-project Release build and formatting gate are clean, and the complete rootless-Podman/Chromium solution passes 93/93. | This change |
| 2026-11-07 | Phase 9 | Added the driver-free EF Core foundation, CMS/Common/Ecommerce/Media repository implementations, and thin PostgreSQL and SQL Server provider packages. | The same 29 inherited contract/lifecycle facts pass against PostgreSQL 18.1 and SQL Server 2022 CU22 GDR1, including explicit ordered persistence for nested CMS lists and galleries. Startup migrates before ordered seeders on both providers and exposes an explicit opt-out. An architecture guard rejects concrete drivers in all five shared/module EF projects; SQL Server alone pins the patched Pkcs 9.0.22 transitive servicing release. The 39-project Release build and formatting gate are clean, and the aggregate rootless-Podman/Chromium suite passes 124/124, including unchanged MongoDB 4.0.3/8.0/9.0 behavior. | This change |
| 2026-11-07 | Phase 9 | Corrected a relational contract-ordering failure exposed by the first hosted CI run. | The CMS relational fixture now composes Common, Ecommerce, and Media mappings before the dynamic CMS contributor, matching the documented Admin context and making CMS-visible module registrations valid regardless of test-class ordering. The 39-project Release build and formatting gate are clean, and two consecutive PostgreSQL/SQL Server runs pass 29/29. | PR #22 |

## Immediate next actions

1. Add provider-specific Lantern House Admin contexts and migrations; keep the public
   Website context independent.
2. Make the demo's persistence choice explicit and prove MongoDB, PostgreSQL, and SQL
   Server startup paths without making one host bring in every driver.
3. Complete aggregate MongoDB/relational/example/browser verification and close the
   Phase 9 exit gate.
