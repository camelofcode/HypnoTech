# Known limitations

HypnoTech 1.1 is a stable public release, not a claim that every planned provider,
platform, or payment integration is complete.

## Persistence

- MongoDB remains the production-proven provider. Version 1.1 adds PostgreSQL and SQL
  Server through the shared EF Core layer, including provider selection and host-owned
  migrations in the public example.
- MongoDB Server 4.0.3 remains a tested production-compatibility constraint, while
  8.0.32 is the recommended supported target and 9.0.4 is exercised as the current
  compatibility target.
- EF Core PostgreSQL and SQL Server pass the common provider contract suite against
  real containers. MySQL and NHibernate are explicitly deferred.
- Production database migration is operational work outside the engine release; see
  the [MongoDB migration plan](operations/mongodb-production-migration.md).

## Ecommerce

- The existing PayPal v1 integration is legacy code retained for compatibility.
- Additional payment providers and a reusable mock provider remain deferred in the
  separate ecommerce payment plan.
- The public example is calculation-only and deliberately blocks every real payment,
  order, email, download-code, and fulfilment side effect.

## Platform and deployment

- Linux is the supported production and required CI platform.
- Windows and Intel macOS receive informative build/unit-test smoke coverage, but are
  not production support claims.
- Runtime frontend assets are committed and self-hosted. Node/npm are required only to
  regenerate or verify authored frontend changes.
- Public NuGet packages are not yet available; consumers currently build from source.

## API and compatibility

- Public `main` is async-only for repository and I/O-bound service contracts. The
  preserved `support/legacy-production` branch retains the historic synchronous API.
- Version 1.0 established the stable public contract and version 1.1 remains backward-
  compatible with it. Neither release promises source compatibility with the legacy
  branch.
- The public Website remains intentionally independent of Admin/CMS mappings. A host
  may use separate read models or a different DAL for its public rendering needs.
