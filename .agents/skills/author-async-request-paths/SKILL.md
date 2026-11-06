---
name: author-async-request-paths
description: Add or revise HypnoTech ASP.NET Core controllers, services, repository contracts, or persistence implementations. Use when creating request paths or changing I/O so module boundaries, controller conventions, native async APIs, cancellation, and v0.1 sync compatibility remain consistent.
---

# Author HypnoTech request paths

Keep genuine I/O asynchronous from the HTTP action to the underlying provider without
making CPU-only work artificially asynchronous.

## Establish the boundary

1. Read `AGENTS.md`, `docs/architecture.md`, `docs/compatibility.md`, and the current
   phase in `PUBLIC_READINESS_PLAN.md`.
2. Run `cms-compatibility-review` before changing a public repository contract,
   persistence behavior, dynamic CMS path, route, BSON mapping, or collection name.
3. Trace the complete path: controller or tag helper → service → provider-neutral
   repository contract → provider implementation → external I/O.
4. Check every call site, DI registration, test fake, and custom host implementation
   before changing an interface.

## Preserve HypnoTech architecture

- Put provider-neutral repository interfaces in the module's contract or `*.Dal`
  project and MongoDB implementations in `*.Dal.MongoDb`.
- MongoDB repositories normally inherit `BaseMongoRepository`; use its native async
  helpers or MongoDB Driver async APIs rather than duplicating connection setup.
- Public website content pages that participate in partial navigation inherit
  `SpaController` and return `AjaxPartialView`. Keep deliberate full-document
  boundaries such as Checkout unchanged.
- Configure mapped admin content through `FormClassMapping<T>` and the generated
  `CmsController<T>` pipeline. Do not add hand-written CRUD controllers merely to
  bypass that convention.
- Keep public website projects independent of `HypnoTech.Cms` and
  `HypnoTech.Cms.MongoDb`; give them focused read repositories over neutral models and
  persistence infrastructure.

## Implement real async I/O

- Name task-returning I/O methods with the `Async` suffix and accept an optional
  `CancellationToken` as the final parameter.
- Pass `HttpContext.RequestAborted` from controllers and propagate that same token
  through services, repositories, MongoDB, HTTP, SMTP, file, and stream operations.
- Use provider APIs such as `FirstOrDefaultAsync`, `ToListAsync`, `InsertOneAsync`,
  `ReplaceOneAsync`, `DeleteOneAsync`, `CountDocumentsAsync`, `CopyToAsync`, and
  `ReadAsStringAsync` as appropriate.
- Await independent I/O concurrently with `Task.WhenAll` when the operations do not
  depend on one another or share unsafe mutable state. Do not use `.Result` to collect
  completed tasks; await them.
- Do not use `Task.Run` to disguise blocking I/O, and do not use `.Result`, `.Wait()`,
  or `GetAwaiter().GetResult()` in request paths.
- Leave pure rendering, mapping, validation, hashing, in-memory projection, metadata,
  and other CPU-only work synchronous unless it naturally participates in an async
  method that also performs I/O.

## Retain compatibility deliberately

- Existing public synchronous members remain available through v0.1 unless a breaking
  change is explicitly authorised.
- Keep retained sync members natively synchronous. Never implement them by blocking on
  async members.
- Make the async member the path used by engine controllers, tag helpers, services,
  and examples.
- For an additive interface member that existing third-party implementations cannot
  immediately implement, consider a default compatibility implementation only when it
  preserves source and binary consumers. Ensure first-party implementations with I/O
  override it using native async calls.
- Preserve routes, verbs, response and view types, validation, flash messages,
  ordering, collection names, and BSON names during an async-only conversion.

## Verify the complete path

1. Add focused tests for async success and cancellation at the narrowest useful layer.
2. Extend the blocking-wait architecture scope when adding a new request-path area.
3. Run affected unit and repository contracts, including MongoDB 4.0.3 when persistence
   changes, then the relevant example integration or Playwright journey.
4. Run `verify-engine-change` and record the change, checks, skips, and remaining work
   in `PUBLIC_READINESS_PLAN.md`.
