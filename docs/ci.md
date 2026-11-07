# Continuous integration

The `CI` GitHub Actions workflow is the automated release gate for HypnoTech. It runs
for pull requests, pushes to `main`, and explicit manual dispatches. Superseded runs on
the same pull request are cancelled.

## Required Linux checks

The required jobs use the standard `ubuntu-24.04` GitHub-hosted runner:

- `Frontend assets` installs the pinned Node.js/npm toolchain with `npm ci` and rejects
  stale committed browser bundles.
- `Release build` restores and builds the complete solution in Release configuration.
- `Unit and architecture tests` runs the container-free suites and retains Cobertura
  coverage for seven days.
- `MongoDB compatibility` runs the repository contracts against the pinned production
  4.0.3, supported 8.0.32, and current 9.0.4 images.
- `Example integration` starts MongoDB 8.0.32 and tests the real Lantern House admin
  host, seed data, public rendering boundaries, and payment safety boundary.
- `Playwright (chromium|firefox|webkit)` runs the five browser journeys independently
  in each engine. A failed journey retains its trace, screenshot, browser errors, and
  application logs for seven days.

The integration and browser jobs use Testcontainers through the Docker service already
provided by the standard Ubuntu runner. They do not require a repository secret and
never call a live or sandbox payment provider.

## Portability checks

`Platform smoke` builds the solution and runs the CMS, Common, and architecture suites
on standard `ubuntu-24.04`, `windows-2025`, and `macos-15-intel` runners. The matrix is
informative and non-blocking for v0.1 because Linux is the only production platform
currently claimed.

## Cost and security controls

The workflow deliberately uses only standard GitHub-hosted runner labels. GitHub
documents those runners as free and unlimited for public repositories; larger runner
labels and custom hosted images are not used. See the
[GitHub-hosted runners reference](https://docs.github.com/en/actions/reference/runners/github-hosted-runners)
and [GitHub Actions billing](https://docs.github.com/en/billing/concepts/product-billing/github-actions).

Workflow permissions are limited to read-only repository contents, checkout does not
persist credentials, third-party actions are pinned to immutable commit SHAs, jobs have
timeouts, and failure/coverage artifacts expire after seven days. Artifact storage has
a separate allowance from public runner compute, so successful browser runs upload
nothing.

## Local equivalents

The main local commands are:

```bash
npm ci
npm run check:frontend
dotnet restore HypnoTech.slnx
dotnet build HypnoTech.slnx -c Release --no-restore
dotnet test tests/HypnoTech.Cms.UnitTests -c Release
dotnet test tests/HypnoTech.Common.UnitTests -c Release
dotnet test tests/HypnoTech.ArchitectureTests -c Release
dotnet test tests/HypnoTech.Cms.MongoDb.IntegrationTests -c Release
dotnet test tests/HypnoTech.Demo.IntegrationTests -c Release
./tests/run-end-to-end.sh --all-browsers
```

Rootless Podman setup and focused browser options are documented in
[`tests/README.md`](../tests/README.md).
