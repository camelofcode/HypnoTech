# Public NuGet packaging plan

Public NuGet distribution remains deferred after the GitHub source releases. The goal
is to let future consumers use exact package versions instead of project references
without accidentally publishing incomplete, inseparable, or misleading packages.

## Hosting decision

Publish public packages to **nuget.org**. It is the default ecosystem feed and requires
no extra source or GitHub authentication for consumers. GitHub Packages remains an
option for future private/internal packages, but adds unnecessary feed configuration
for these public libraries.

Use nuget.org Trusted Publishing from GitHub Actions. The release workflow will request
a short-lived OIDC credential rather than storing a long-lived NuGet API key.

## Package audit

1. Inventory every runtime project and its public API/dependency graph.
2. Mark examples, tests, authored frontend workspaces, and build-only projects
   `IsPackable=false`.
3. Decide whether the current project granularity is appropriate for consumers; retain
   separate module/provider packages where it prevents unwanted dependencies.
4. Assign stable package IDs and confirm each ID is available on nuget.org.
5. Add package descriptions, tags, repository/project URLs, licence/readme metadata,
   symbols, deterministic build metadata, and Source Link.
6. Confirm project references become correct package dependencies and do not pull
   unrelated CMS, Ecommerce, Media, MongoDB, or future relational providers.

## Rehearsal and verification

1. Run `dotnet pack` in Release configuration into an isolated artifacts directory.
2. Inspect every `.nupkg` and `.snupkg`, including licence, notice, README, content
   files, dependencies, target frameworks, and symbol/source information.
3. Restore the packages into fresh synthetic Admin and Website consumers through a
   local folder feed.
4. Build and run representative CMS, frontend, Media, Ecommerce, and persistence paths
   without project references back into the repository.
5. Publish a prerelease such as `x.y.z-rc.1` only after the local-feed consumers pass,
   using the actual planned package version in place of `x.y.z`.
6. Remember that nuget.org versions are immutable: an incorrect version can be
   unlisted but not replaced.

## Release automation

Add a dedicated tag-triggered workflow only after the package audit passes. It should:

- require the same source commit to have passed the normal CI gate;
- pack once with the version derived from the release tag;
- retain package artifacts for manual inspection;
- use a protected GitHub `release` environment and nuget.org Trusted Publishing;
- publish only an explicit allow-list of reviewed runtime packages;
- fail rather than skip duplicate or unexpected package versions;
- attach the reviewed packages to the matching GitHub Release as secondary artifacts.

The repository owner must create or select a nuget.org account, enable appropriate
account security, and register the trusted-publisher policy for the final workflow.

## Consumer migration

Private production sites remain pinned to `support/legacy-production` until separately
tested against the async public API. A future migration should replace project
references with exact package versions, run each site's own build/browser checks, and
deploy independently. Publishing packages does not itself authorize or perform that
migration.
