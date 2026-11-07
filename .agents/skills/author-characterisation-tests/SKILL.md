---
name: author-characterisation-tests
description: Add or revise HypnoTech unit, integration, architecture, or browser tests. Use for dynamic CMS mappings, generated controllers and routes, forms, services, repositories, MongoDB behaviour, regression coverage, test fixtures, or test infrastructure.
---

# Author HypnoTech characterisation tests

Protect observable behaviour before improving implementation. A surprising current
result is evidence to record, not permission to silently correct production code.

## Establish the test boundary

1. Read `AGENTS.md`, `docs/compatibility.md`, and any current plan explicitly governing
   the requested work.
2. For dynamic CMS or persistence behaviour, run `cms-compatibility-review` before
   changing production code.
3. Choose the narrowest layer that proves the behaviour:
   - unit tests for calculations, transformations, validation, branching, error
     mapping, cancellation propagation, mappings, forms, controllers, services, and
     pure helpers;
   - integration tests for MongoDB queries and BSON, DI composition, middleware,
     filesystem behaviour, SDK behaviour, and HTTP wire contracts;
   - architecture tests for dependency direction and rules that source inspection can
     prove reliably;
   - Playwright only for behaviour that requires a real browser or interaction across
     several application layers.
4. Use hand-written fakes when the dependency surface is small. Add a mocking library
   only when repeated test code shows a clear need.

## Decide when new coverage is required

- Add a focused regression test for every bug fix at the lowest layer that reproduces
  the defect.
- Add or revise tests when observable behaviour, a public contract, a route, persisted
  data, provider interaction, security boundary, or failure mode changes.
- A pure refactor does not need a contrived new test when existing coverage already
  proves the behaviour, but the relevant suite must still run.
- Documentation and mechanical-only changes need proportionate verification, not an
  unrelated test-count increase.
- Do not duplicate the same assertion at every layer. Add a broader test only when it
  protects integration that a narrow test cannot observe.

## Choose honest test doubles

- Fake small HypnoTech-owned interfaces directly in unit tests and model only the
  behaviour the test needs.
- Do not mock MongoDB Driver internals. Exercise repository behaviour against the real
  pinned MongoDB containers.
- Do not mock a third-party SDK's internal call chain. For outbound HTTP or SDK
  adapters, use `test-external-http-integrations` and exercise the real client against
  a local protocol stub.
- Assert returned state, persisted state, emitted requests, or other observable
  outcomes. Verify calls only when the interaction itself is the contract, such as
  preventing a payment request or sending exactly one side-effecting operation.

## Keep fixtures safe and deterministic

- Create minimal synthetic models, documents, names, files, and content. Never import
  private client-site data, configuration, branding, or assets.
- Give test-only mapped model types unique names. Clear or restore mutable public
  registries used by the test, and disable parallel execution where global state makes
  isolation impossible.
- Use a fresh temporary directory, database, and collection namespace for each fixture
  or test class. Clean only resources created by that test.
- Avoid time, ordering, random, network, and filesystem assumptions unless the test
  controls them. Assert complete externally meaningful results, not incidental private
  implementation details.
- Name tests `Method_or_scenario_expected_result`; use Arrange/Act/Assert spacing when
  it improves readability.

## Characterise the dynamic CMS

Cover the end-to-end convention when relevant:

`FormClassMapping` registration → member/form mapping → generated controller and route
→ service interaction → repository operation → persisted BSON.

Explicitly consider capability gates, verbs and antiforgery, view and redirect names,
flash text, validation, title and sort defaults, file/list/gallery handling, roles,
navigation, and duplicate registration. If current behaviour appears defective, write
a focused test or roadmap note first; fix it only in a separately reviewed change.

## Exercise MongoDB compatibility

- Always run repository contracts against the exact production baseline
  `mongo:4.0.3`, one pinned supported MongoDB release, and the pinned current major
  release.
- Keep MongoDB.Driver at the repository baseline unless dependency work is explicitly
  in scope.
- Assert collection and BSON element names, IDs, missing/legacy fields, search escaping
  and case rules, default and explicit sorting, paging counts, children, list/gallery
  IDs, ordered movement boundaries, seed behaviour, and repeated host registration as
  applicable.
- A missing container runtime is a reported prerequisite, never a passing or silently
  skipped compatibility check.

## Verify and record

1. Run the focused test project while iterating.
2. Repeat stateful or ordering-sensitive tests to expose leaked state.
3. Run the full unit/architecture selection, then all MongoDB targets when changed.
4. Generate coverage as information; do not add a high global threshold without a
   reviewed baseline.
5. Run `verify-engine-change`, then record commands, results, skips, discoveries, and
   remaining work in the current task or its explicitly active plan.
