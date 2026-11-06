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
| Container runtime | Podman 5.8.9 | Testcontainers exercised through the rootless Docker-compatible API socket |

The complete captured fingerprints are in
[`docs/baselines/production-2026-11-06.md`](baselines/production-2026-11-06.md).

## Toolchain policy

`global.json` requests .NET SDK 10.0.100 and allows the latest compatible feature band;
10.0.401 is the version verified during Phase 0. `package.json` requires Node.js 22 or
newer and pins npm 11.16.0 as the package manager. All npm build dependencies are exact
versions and the root lockfile is authoritative. Automation must use `npm ci`.

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
release. Windows previously worked as a development platform but is not a release gate.
A future Windows smoke job may be informative and non-blocking.

## Async API transition

The Phase 6 CMS, Common-backed admin, Ecommerce, and Media request paths use native
async MongoDB, file, stream, SMTP, and HTTP APIs and propagate the request cancellation
token. Existing synchronous repository and service members remain available through
v0.1 for source compatibility. They retain native synchronous implementations rather
than calling the new async members. Async members are the primary API used by engine
controllers, module tag helpers, and the public example. Removal or obsolescence of the
sync surface is deferred to a separately documented breaking release.

PayPal v1 is the narrow documented exception. Its 2018 SDK exposes only a task-returning
HTTP transport and no cancellation-token overload. Engine request paths await that SDK
directly; the two old synchronous `IPayPalService` members remain as an isolated blocking
compatibility adapter and are allowlisted by the architecture check. Once a side-effecting
PayPal call starts, the async path awaits it to completion rather than abandoning a remote
payment operation that the SDK cannot actually cancel. No live or sandbox payment is made
by the automated tests.

## Example application verification

The Lantern House public example has been exercised on Linux against the pinned
MongoDB 8.0.32 development target. The Playwright for .NET suite runs the real admin
and public hosts with a unique database and upload store, fresh browser contexts, and
headless Chromium, Firefox, and WebKit. The admin host uses test-only ephemeral HTTPS
so its secure authentication cookie is exercised consistently rather than relaxed for
automation. The five journeys cover authentication and authorization, dynamic CMS
validation/CRUD/search/ordering/duplication, a real image upload reaching the public
website, representative Media, server-calculated basket and postage totals, the
fail-closed checkout boundary, and a 390 px mobile navigation smoke test. Failures
retain a trace, screenshot, browser errors, and both application logs. Ubuntu is the
authoritative three-engine host; Fedora's native WebKit library ABIs are not supported
by Playwright.
