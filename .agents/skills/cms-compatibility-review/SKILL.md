---
name: cms-compatibility-review
description: Review a proposed HypnoTech change for dynamic CMS and production-data compatibility before implementation. Use for mappings, generic forms/controllers/services, routes, registries, repository contracts or implementations, MongoDB BSON/collection names, paging, search, ordering, child population, DI registration, or public behaviour.
---

# Review CMS compatibility

Perform this review before implementation. It is a decision and test-design workflow,
not permission to modify code.

## Trace the affected behaviour

1. Read `AGENTS.md`, `docs/architecture.md`, `docs/compatibility.md`, and the relevant
   roadmap phase.
2. Trace the proposed change end to end where applicable:
   model/registry registration → `FormClassMapping` → generated controller and route →
   form/table/service behaviour → repository contract → MongoDB implementation and
   persisted BSON.
3. Find every affected call site and public extension method. Include reflection-driven
   and convention-driven consumers that a normal reference search may miss.
4. Identify static/global registries and test-order or duplicate-registration effects.

## Record compatibility decisions

For each affected surface, classify the intended result as:

- unchanged and protected by a characterisation test;
- additive and backward compatible;
- deliberately breaking with an approved migration and rollback plan; or
- unknown and therefore blocked pending evidence.

Explicitly review:

- route templates, controller/action names, verbs, redirects, view names, response
  types, validation, flash messages, and authorisation;
- public namespaces, assemblies, interfaces, generic constraints, and DI registration;
- collection names, BSON element names/types, IDs, nested/list representation, defaults,
  missing fields, existing documents, indexes, and seed behaviour;
- search escaping/case rules, sort defaults, stable paging, counts, child population,
  ordered moves, and multi-document atomicity;
- static asset paths and source/generated ownership when UI behaviour changes;
- compatibility with MongoDB.Driver 3.4.3 and MongoDB Server 4.0.3.

Never infer that a renamed C# member safely renames stored BSON. Never copy private-site
fixtures to make a test pass; create minimal synthetic models and documents.

## Produce the review

Return:

1. the traced surfaces and consumers;
2. compatibility classification for each surface;
3. a focused characterisation-test plan, including representative legacy BSON when
   persistence is affected;
4. migration and rollback requirements, if any;
5. unresolved evidence or decisions;
6. a clear recommendation: proceed, revise, or block.

If implementation is later authorised, keep the review conclusions visible in the
change and re-run the relevant checks through `verify-engine-change`.
