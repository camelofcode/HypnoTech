# Persistence provider contracts

HypnoTech persistence implementations share observable behaviour through an inherited
xUnit contract suite. The suite is deliberately provider-neutral: its models contain no
BSON, Entity Framework Core, SQL Server, or PostgreSQL attributes, and its assertions
use only the public async repository interfaces.

The executable contracts are in `tests/HypnoTech.Persistence.ContractTests`. A provider
test project supplies the relevant contract context, creates its real repository
implementations, and inherits the CMS, Common, Ecommerce, and Media abstract suites.
The MongoDB adapters run every inherited fact against MongoDB 4.0.3, 8.0.32, and 9.0.4.
The EF adapters run the same facts against PostgreSQL 18.1 and SQL Server 2022 CU22
GDR1 through Testcontainers.

## Current contract

Every provider must preserve these behaviours:

- CRUD round trips preserve mapped values and generate a non-empty identifier.
- Equivalent adds are not idempotent: each add creates a distinct record and ID.
- Removing an already removed record is an idempotent no-op.
- Default and explicit sorting, paging, and page counts are stable.
- Search is case-insensitive, treats its input literally, and spans all searchable
  members; a model with no searchable members returns an empty page for a search.
- Ordered movement swaps adjacent values and treats collection boundaries as no-ops.
- Nested list and gallery values receive non-empty, unique identifiers, and edits
  preserve their declared order while replacing removed values.
- Registered CMS children populate through their opaque reference ID, while a null
  reference leaves the child unset.
- Registered select-list models round trip through their logical data store.
- Seed data populates an empty logical store once and does not overwrite existing data.
- An already-cancelled token cancels provider I/O.

The module suites additionally cover every current Common repository interface, the
Ecommerce catalogue/postage/order/download-code lifecycle, and all Media query
interfaces. Read ordering, uniqueness rules, edit preservation, status transitions,
code depletion, lookup semantics, and paging are asserted through public interfaces.

The contract uses opaque string identifiers. Their physical representation is owned by
the provider; MongoDB may use `ObjectId` and a relational implementation need not.

## Provider-specific compatibility

The common suite does not turn implementation details into engine requirements.
MongoDB-only tests continue to own:

- BSON element and collection names;
- `ObjectId` representation;
- deserialisation of legacy documents with omitted optional members;
- legacy child lookup from a raw BSON reference; and
- raw MongoDB document compatibility around the shared child, seed, and select-list
  semantics.

Logical store names remain part of the historic seed, model-registry, and child-mapping
APIs. A relational provider may map them to entity sets or tables; it must not require
MongoDB collection or `ObjectId` behaviour.

## Concurrency and transactions

The existing public interfaces do not expose versions or concurrency tokens. Their
current compatibility rule is therefore last-committed-write-wins. Optimistic
concurrency can be added only as a deliberate later API change with conflict semantics
that callers can observe and handle.

Each single-record mutation must be atomic. An ordered move is one logical operation:
new relational providers must update both rows in a transaction. The MongoDB baseline
performs two best-effort replacements because changing that behaviour could require a
replica-set transaction and would alter production assumptions. This is an explicit
provider difference, not a relational design target.

## EF Core startup lifecycle

EF Core hosts will apply pending migrations automatically during startup by default,
then run seed operations. Seeding must never run against an older schema. Migration or
seed failure must fail startup rather than serving against a partially prepared
database.

The shared EF package exposes an explicit opt-out for deployments that apply
migrations as a separate controlled release step. The default examples and normal
HypnoTech registration path will remain migrate-then-seed with no manual command
required. Provider integration tests prove this ordering against real PostgreSQL and
SQL Server containers; EF's in-memory provider is not accepted as evidence.

See [entity-framework-core.md](entity-framework-core.md) for package selection, context
composition, migration ownership, and complete startup registration examples.

## Adding a provider

1. Reference `HypnoTech.Persistence.ContractTests` from the provider integration test
   project.
2. Implement the provider context and return a fresh disposable database for each fact.
3. Inherit the relevant abstract contract suite; do not copy its assertions.
4. Add provider-specific storage compatibility tests separately.
5. Run the complete shared suite against a real supported database version.

Do not weaken a shared assertion to accommodate a provider accident. Record and review
the semantic difference first; either fix the provider or make the exception explicit
in this document and the public-readiness decision log.
