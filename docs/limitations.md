# Known limitations

HypnoTech 1.0 is a stable public baseline, not a claim that every planned provider,
platform, or payment integration is complete.

## Persistence

- MongoDB is the only implemented persistence provider in 1.0.
- MongoDB Server 4.0.3 remains a tested production-compatibility constraint, while
  8.0.32 is the recommended supported target and 9.0.4 is exercised as the current
  compatibility target.
- EF Core PostgreSQL and SQL Server implementations are planned only after the common
  provider contract suite is complete. MySQL and NHibernate are explicitly deferred.
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
- Version 1.0 does not promise source compatibility with that legacy branch; it defines
  the new stable public contract from which semantic versioning proceeds.
- The public Website remains intentionally independent of Admin/CMS mappings. A host
  may use separate read models or a different DAL for its public rendering needs.
