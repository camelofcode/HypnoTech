# Compatibility

This document distinguishes configurations that have actually been exercised from
future support goals. Version numbers are not support claims unless a recorded build or
test covers the combination.

## Verified public-readiness baseline

| Component | Verified version | Evidence/meaning |
| --- | --- | --- |
| Engine | commit `95c9f6a` | Revision used by both live sites when the baseline was captured |
| .NET SDK | 10.0.401 | Built all 21 projects in Release with zero warnings and errors |
| Target framework | .NET 10 (`net10.0`) | All current projects target this framework |
| Operating system | Linux | Development verification and production platform |
| Node.js | 24.18.0 | Ran the root frontend freshness check |
| npm | 11.16.0 | Lockfile install/build tool version recorded by `packageManager` |
| MongoDB.Driver | 3.4.3 | Current package version in every MongoDB implementation |
| MongoDB Server | 4.0.3 | Current live-site server version |

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
must first add an integration test for 4.0.3 and define a separate, reversible migration
plan before changing this pair.

MongoDB 4.0.3 is an old production constraint, not a recommendation for new
deployments. Later CI may test a supported-current MongoDB version as an additional
target, but that does not replace the production-baseline test.

## Platform policy

Linux is the required development/CI and production platform for the first public
release. Windows previously worked as a development platform but is not a release gate.
A future Windows smoke job may be informative and non-blocking.

## Not yet verified

The repository does not yet contain a runnable public example or automated unit,
MongoDB integration, or browser tests. Consequently, no broader runtime support claim
is made yet. Those gates are tracked in `PUBLIC_READINESS_PLAN.md` and this document
must be updated as the matrix gains verified configurations.
