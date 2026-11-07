---
name: release-engine
description: Prepare and publish a numbered HypnoTech GitHub source release. Use when choosing a release version, creating release notes, closing a release phase, tagging verified main, or publishing a GitHub Release; do not use for NuGet publication or production deployment.
---

# Release the HypnoTech engine

Make the release reproducible from an immutable, verified `main` commit. A release is
not complete merely because its implementation PR merged.

## Establish the candidate

1. Read `AGENTS.md`, `PUBLIC_READINESS_PLAN.md`, `docs/compatibility.md`, and the latest
   file under `docs/releases/`.
2. Inspect the current tags, GitHub Releases, merged changes since the latest tag, and
   the checks on current `main`. Choose the SemVer increment from the public API and
   behavior change; do not infer it from the age of production consumers.
3. Confirm the proposed tag and GitHub Release do not already exist. Never replace,
   move, or force-push a published tag.
4. Start a release-preparation branch from current `origin/main` with a clean working
   tree. Do not tag this branch or its PR commit.

## Prepare the repository

- Set the shared `VersionPrefix` in `Directory.Build.props` and verify that MSBuild
  resolves `Version`, `VersionPrefix`, and `PackageVersion` to the intended value.
- Add `docs/releases/v<version>.md` with highlights, compatibility boundaries,
  verification evidence, distribution, deferred scope, and licensing.
- Reconcile README status, compatibility/limitations documentation, the roadmap phase
  status, next action, and work log. Remove stale “in progress” claims.
- Preserve the MongoDB baseline, private-client boundary, and Admin/Website persistence
  independence unless the release explicitly and safely changes them.
- Do not publish NuGet packages, deploy production, migrate a database, or edit a
  private client repository as an implied part of a GitHub source release.

Use `verify-engine-change` for local checks. Report any skipped check accurately. Open
a PR and let the complete hosted release gate rerun even when the preparation diff is
documentation and version metadata only.

## Publish only from verified main

After the release-preparation PR is merged:

1. Fetch `main` and identify its exact commit SHA. Confirm the PR is merged and every
   required CI and CodeQL check on the candidate is successful.
2. Confirm the checked-in version and release notes at that SHA match the intended tag.
3. Create an annotated `v<version>` tag on that exact `main` commit and push only that
   tag. Stop if the local or remote tag already exists with any target.
4. Create the GitHub Release for the existing tag, titled `HypnoTech <version>`, using
   the tracked release-notes file verbatim. Do not auto-generate unrelated changelog
   noise or silently mark a prerelease as stable.
5. Verify the remote tag target, published release URL, title, notes, and latest-release
   status. Then record publication evidence in the roadmap; use a follow-up docs-only PR
   when that record cannot be part of the immutable released commit.

External mutations still require the user's authorization. If checks fail, the tag
already exists, the version is inconsistent, or `main` moves before tagging, stop and
re-establish the candidate rather than guessing or rewriting release history.
