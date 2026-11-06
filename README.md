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
- Docker or Podman with its Docker-compatible API socket for MongoDB integration tests.

```bash
dotnet restore HypnoTech.slnx
dotnet build HypnoTech.slnx -c Release --no-restore
dotnet test HypnoTech.slnx -c Release --no-build
npm ci
npm run check:frontend
```

Generated frontend assets are committed, so normal .NET consumers do not need npm.
Use npm when changing frontend source or verifying that committed bundles are current:

```bash
npm run build:frontend
npm run check:frontend
```

The unit, architecture, and MongoDB characterisation suites are under `tests/`. The
MongoDB suite runs the same repository contracts against production MongoDB 4.0.3 and
pinned MongoDB 8.0 and 9.0 releases. Podman setup, focused commands, and coverage
instructions are in [`tests/README.md`](tests/README.md). A runnable public example and
browser tests remain later roadmap phases.

## Repository layout

Source projects are grouped physically by functional area:

- `src/Cms/` — dynamic CMS, contracts, MongoDB implementation, and frontend source;
- `src/Common/` — shared services, models, security, email, and DAL contracts;
- `src/Ecommerce/` — ecommerce domain, admin/frontend support, DAL, and frontend source;
- `src/Media/` — media domain, frontend support, DAL, and frontend source;
- `src/Frontend/` — general website helpers and shared frontend source;
- `src/Persistence/` — shared MongoDB infrastructure;
- `tests/` — xUnit v3 unit, architecture, and Testcontainers integration tests;
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
