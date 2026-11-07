# Compatibility

This document distinguishes configurations that have actually been exercised from
future support goals. Version numbers are not support claims unless a recorded build or
test covers the combination.

## Verified public-readiness baseline

| Component | Verified version | Evidence/meaning |
| --- | --- | --- |
| Engine | commit `95c9f6a` | Revision used by both live sites when the baseline was captured |
| .NET SDK | 10.0.401 | Compiled all 30 source, example, and test projects in Release; the final offline audit-feed check emitted only `NU1900` and no compiler warning or error |
| Target framework | .NET 10 (`net10.0`) | All current projects target this framework |
| Operating system | Linux | Development verification and production platform |
| Node.js | 24.18.0 | Ran the root frontend freshness check |
| npm | 11.16.0 | Lockfile install/build tool version recorded by `packageManager` |
| MongoDB.Driver | 3.4.3 | Current package version in every MongoDB implementation |
| MongoDB Server | 4.0.3 | Current live-site version; repository contracts pass against the exact container image |
| Supported MongoDB target | 8.0.32 | Same repository contracts pass against the pinned supported release |
| Current MongoDB target | 9.0.4 | Same repository contracts pass against the pinned current major release |
| EF Core / Relational / SQL Server provider | 10.0.14 | Release build and inherited relational contracts |
| Npgsql EF provider / Npgsql | 10.0.3 | PostgreSQL contracts against `postgres:18.1-alpine` |
| SQL Server | 2022 CU22 GDR1 | SQL Server contracts against the exact Microsoft container tag |
| Container runtime | Podman 5.8.9 | Testcontainers exercised through the rootless Docker-compatible API socket |

The complete captured fingerprints are in
[`docs/baselines/production-2026-11-06.md`](baselines/production-2026-11-06.md).

The relational targets are v1.1 development evidence, not claims about the unchanged
v1.0 production sites. PostgreSQL and SQL Server run the same CMS, Common, Ecommerce,
Media, and migrate-before-seed assertions. The SQL Server adapter directly pins the
patched `System.Security.Cryptography.Pkcs` 9.0.22 servicing release because the
current SQL client dependency floor otherwise resolves the vulnerable 9.0.11 release.

## Toolchain policy

`global.json` requests .NET SDK 10.0.100 and allows the latest compatible feature band;
10.0.401 is the version verified during Phase 0. `package.json` requires Node.js 22 or
newer and pins npm 11.16.0 as the package manager. All npm build dependencies are exact
versions and the root lockfile is authoritative. Automation must use `npm ci`. npm
install scripts are governed by a fail-closed allowlist; unreviewed scripts fail the
install, and the one current scripted dependency is explicitly denied.

Generated runtime assets are committed. Building the .NET projects therefore does not
require Node.js; Node/npm are required when modifying frontend source and in freshness
verification.

## MongoDB compatibility rule

MongoDB server and driver versions are independent. The only production combination
currently claimed is MongoDB.Driver 3.4.3 against MongoDB Server 4.0.3. Do not raise the
driver version or server compatibility floor on the support branch. Public development
now exercises 4.0.3 through Testcontainers, but must still define a separate,
reversible migration plan before changing this pair.

MongoDB 4.0.3 is an old production constraint, not a recommendation for new
deployments. The additional `mongo:8.0.32-noble` target is supported through October
2029, while `mongo:9.0.4` exercises the current major release. Neither replaces the
production-baseline test. These contracts verify the engine operations they cover;
they do not claim support for every MongoDB 9.0 feature.

## Platform policy

Linux is the required development/CI and production platform for the first public
release. The CI workflow also builds and runs the container-free suites on standard
Windows and Intel macOS runners as informative, non-blocking portability checks. Those
jobs are visibility, not support claims; only the required Linux jobs gate v1.0.

GitHub's current Ubuntu 24.04 runner uses Linux kernel 7.0. The pinned MongoDB 4.0.3,
8.0.32, and 9.0.4 compatibility containers include releases that reject Linux kernel
6.19 and newer, so container-backed CI is pinned to the standard Ubuntu 22.04 runner.
The required Release build and container-free suites remain on Ubuntu 24.04; this is a
host-kernel compatibility constraint, not a paid or custom runner.

## Async API boundary

The Phase 6 CMS, Common-backed admin, Ecommerce, Media, Frontend, and public-example
request paths use native async MongoDB, file, stream, SMTP, response, and HTTP APIs and
propagate the request cancellation token. Public repository and I/O-bound service
contracts on `main` are async-only; their duplicate synchronous members and concrete
implementations were removed before the first public release. The unchanged production
sites remain pinned to `support/legacy-production`, which preserves the historic API.
Kestrel and IIS synchronous I/O are not enabled.

PayPal v1 is the narrow documented exception. Its 2018 SDK exposes only a task-returning
HTTP transport and no cancellation-token overload. Engine request paths await that SDK
directly. Once a side-effecting PayPal call starts, the async path awaits it to completion
rather than abandoning a remote payment operation that the SDK cannot actually cancel.
No live or sandbox payment is made by the automated tests.

Serilog's `ILogEventSink.Emit` contract is synchronous. The MongoDB sink therefore keeps
its native synchronous driver implementation, but `Serilog.Sinks.Async` 2.1.0 invokes it
on a bounded background worker rather than on request threads. The warning-only queue is
capped at 1,000 events and drops new events instead of blocking when full; Serilog SelfLog
reports those drops. Normal host disposal flushes the queue during shutdown.

## Example application verification

The Lantern House public example has been exercised on Linux against MongoDB 8.0.32,
PostgreSQL 18.1, and SQL Server 2022 CU22 GDR1. The full Playwright browser matrix uses
MongoDB, while the dynamic CMS create/upload/edit/public-read/delete journey also runs
against both relational providers. The Playwright for .NET suite runs the real admin
and public hosts with a unique database and upload store, fresh browser contexts, and
headless Chromium, Firefox, and WebKit. Both hosts use test-only ephemeral HTTPS so
their secure authentication and antiforgery cookies are exercised consistently rather
than relaxed for automation. The five journeys cover authentication and authorization,
dynamic CMS validation/CRUD/search/ordering/duplication, a real image upload reaching
the public website, representative Media, server-calculated basket and postage totals,
the fail-closed checkout boundary, and a 390 px mobile navigation smoke test. Failures
retain a trace, screenshot, browser errors, and both application logs. Ubuntu is the
authoritative three-engine host; Fedora's native WebKit library ABIs are not supported
by Playwright.
