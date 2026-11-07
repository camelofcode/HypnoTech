# Code quality policy

HypnoTech uses compiler warnings, the .NET SDK analyzers, `dotnet format`, architecture
tests, and the behavioural test suites as complementary gates. The policy deliberately
avoids turning every advisory analyzer into release-blocking noise.

## Analyzer baseline

The v1 audit rebuilt the complete solution with .NET 10 `Minimum` and `Recommended`
analysis. `Minimum` reported 18 legacy API/performance findings; `Recommended` reported
233 advisories dominated by naming, globalization, maintainability, and
micro-performance suggestions. Treating either entire set as errors would create broad
public-API and behaviour churn unrelated to the release.

Normal builds therefore use the SDK's .NET 10 `Default` warning-level analysis and
treat every compiler or enabled analyzer warning as an error. This establishes a clean
correctness/security baseline while leaving the wider recommendations as an explicit,
tested modernization backlog rather than silently suppressing individual diagnostics.
`NU1900` remains a warning because a temporarily unavailable vulnerability feed must
not make an otherwise reproducible local build impossible; hosted dependency alerts and
CI still audit the dependency graph.

The analyzer level is pinned to .NET 10 so installing a newer SDK feature band cannot
silently introduce a new release gate. A framework upgrade must review and deliberately
raise that level.

## Formatting

The first public release applied one isolated whitespace/encoding baseline across the
solution. CI now verifies it with:

```bash
dotnet format HypnoTech.slnx --no-restore --verify-no-changes --verbosity minimal
```

The verification run takes roughly nine seconds on the recorded development machine.
Formatting changes should be kept out of behavioural commits; run the formatter only
on intended files during normal development.

## Dependency updates

Dependabot security updates are enabled for every supported ecosystem, but scheduled
version-update pull requests are deliberately disabled. NuGet, npm, and GitHub Actions
remain pinned until a maintainer chooses to review an upgrade; this keeps a stable
single-maintainer repository from producing routine bot churn while still surfacing
known vulnerabilities promptly.

Security updates are never auto-merged. They must pass the same required CI checks as
authored changes, retain pinned frontend output, and preserve the MongoDB 4.0.3
production contract unless a separately reviewed migration changes that baseline.

The npm 11 install-script allowlist is fail-closed. The only current dependency with an
install script, `@parcel/watcher`, is explicitly denied because HypnoTech does not need
its native watcher installation during reproducible builds. If a later dependency adds
an install script, `npm ci` fails until that exact package is reviewed and explicitly
allowed or denied in `package.json`.

## Code scanning

The non-blocking `CodeQL` workflow runs the default query suite for C#, authored
JavaScript/TypeScript, and GitHub Actions on pull requests, `main`, a weekly schedule,
and manual dispatches. Its configuration excludes third-party `lib` trees and committed
`wwwroot/js` build outputs; their exact dependencies and generated-source freshness are
covered by the dependency audit and frontend build instead. Authored frontend sources
remain in scope so an exclusion cannot hide application JavaScript.

CodeQL is deliberately separate from the required CI gate for the v1.0 baseline. New
authored-code alerts must still be triaged before a release; do not dismiss a finding
solely to obtain a green status.
