# HypnoTech

HypnoTech is a modular ASP.NET Core engine for content-managed websites. It combines
a convention-driven CMS with reusable Common, Ecommerce, Media, Frontend, and MongoDB
components. The repository is being prepared as a public portfolio project; the
production-compatible baseline is preserved separately while this branch gains tests,
examples, documentation, and modern asynchronous APIs.

The engine currently provides:

- dynamic CMS controllers, forms, tables, validation, uploads, and model registration;
- account, authentication, email, configuration, navigation, and logging components;
- ecommerce models, administration, cart/checkout support, and the legacy PayPal path;
- media models, administration, and frontend player support;
- swappable repository contracts with MongoDB implementations;
- self-hosted frontend assets with reproducible npm-based bundle generation.

## Status

This is pre-release software under active public-readiness work. It is not yet a
standalone application: the repository currently builds reusable projects, while a
neutral example admin and website are planned. Existing production users remain pinned
to the preserved baseline and MongoDB Server 4.0.3 compatibility remains a hard gate.

See [PUBLIC_READINESS_PLAN.md](PUBLIC_READINESS_PLAN.md) for the phased roadmap and
work record, [docs/architecture.md](docs/architecture.md) for the design, and
[docs/compatibility.md](docs/compatibility.md) for tested versions.

## Build from a fresh clone

Prerequisites:

- a .NET 10 SDK compatible with `global.json`;
- Node.js 22 or newer and npm 11.16.0 for rebuilding frontend assets.

```bash
dotnet restore HypnoTech.slnx
dotnet build HypnoTech.slnx -c Release --no-restore
npm ci
npm run check:frontend
```

Generated frontend assets are committed, so normal .NET consumers do not need npm.
Use npm when changing frontend source or verifying that committed bundles are current:

```bash
npm run build:frontend
npm run check:frontend
```

Tests and a runnable example will be added in later roadmap phases. Until then, the
Release build and frontend freshness check are the baseline verification commands.

## Repository layout

The solution already groups projects by module, although the physical projects remain
flat under `src/` until the dedicated structure-only phase:

- `src/HypnoTech.Cms*` — dynamic CMS, contracts, and MongoDB implementation;
- `src/HypnoTech.Common*` — shared services, models, security, email, and DAL contracts;
- `src/HypnoTech.Ecommerce*` — ecommerce domain, admin/frontend support, and DAL;
- `src/HypnoTech.Media*` — media domain, frontend support, and DAL;
- `src/HypnoTech.Frontend*` — general website helpers and frontend source;
- `src/HypnoTech.MongoDb` — shared MongoDB infrastructure;
- `tools/` — deterministic frontend build tooling.

`*.Frontend` runtime projects contain committed `wwwroot` output. Similarly named
frontend-asset directories contain the authored JavaScript/SCSS plus vendored browser
libraries. Do not edit generated `wwwroot` bundles directly.

## Scope boundaries

This repository contains only the reusable engine. Private Esoteric and SelfHypnosis
sites, their content, branding, seed data, and imagery are deliberately excluded.
Additional payment-provider work described in `ecommerce-payments-plan.md` is also
outside the current public-readiness programme.

## Security, contributions, and licensing

Report vulnerabilities using [SECURITY.md](SECURITY.md). Contribution expectations are
in [CONTRIBUTING.md](CONTRIBUTING.md), and dependency attributions are in
[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

HypnoTech is **source-available for noncommercial use** under the
[PolyForm Noncommercial License 1.0.0](LICENSE.md). It is not OSI-approved open-source
software because commercial use is restricted. Christopher Clayton retains the right
to use and license his own work commercially. Other commercial use requires a separate
written agreement; see [COMMERCIAL-LICENSE.md](COMMERCIAL-LICENSE.md).
