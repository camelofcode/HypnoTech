---
name: verify-engine-change
description: Select and run proportionate HypnoTech verification from the current diff. Use after engine code, frontend, build, test, example, dependency, or repository-structure changes, and before handing off or committing public-readiness work.
---

# Verify an engine change

Verify what changed, not merely what is convenient to run.

## Inspect

1. Read `AGENTS.md`, `docs/compatibility.md`, and the relevant phase of
   `PUBLIC_READINESS_PLAN.md`.
2. Inspect `git status --short` and the complete diff. Separate authored source,
   generated output, dependency, documentation, and structure changes.
3. Identify compatibility-sensitive paths: public APIs, routes, mappings, MongoDB
   names, frontend source/output, project references, and host registration.

## Select checks

Use the smallest relevant checks during iteration, then the required aggregate checks:

- Documentation-only: inspect the diff, validate links/paths and Markdown structure,
  and verify that factual version/status claims match repository evidence.
- C# or project files: run `dotnet build HypnoTech.slnx -c Release`; once tests exist,
  run the affected test project(s), then the required full test selection.
- Frontend source, vendored assets, build tooling, package manifests, or generated
  output: run `npm run check:frontend`. If output is stale, run
  `npm run build:frontend`, review the generated diff, and re-run the check.
- npm dependency changes: use `npm ci`, never a floating install, before the frontend
  check.
- MongoDB behaviour: run the focused unit/contract tests and the production-baseline
  MongoDB 4.0.3 integration target when those suites exist.
- Runnable web/example behaviour: run the relevant integration and Playwright journey
  when those suites exist.
- Physical project moves: verify project references and build the solution; use only
  approved read-only or temporary consumer compatibility checks.

Do not claim a missing test, example, browser suite, database service, or platform as
passed. State that it is unavailable and identify the roadmap gate that will add it.

## Report

Return a compact verification record containing:

1. changed areas and risks;
2. every command run and its result;
3. checks deliberately skipped and why;
4. remaining gaps or manual checks;
5. whether generated assets and the working tree are in the expected state.

Do not repair unrelated failures without authorisation. Diagnose them and distinguish
pre-existing failures from regressions introduced by the change.
