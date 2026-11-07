# Entity Framework Core providers

HypnoTech's relational persistence is split into driver-free repository modules and
thin database-provider packages. An application references the four module packages it
uses, plus exactly one concrete provider package:

- `HypnoTech.EntityFrameworkCore.PostgreSql`, or
- `HypnoTech.EntityFrameworkCore.SqlServer`.

The shared and module EF projects do not reference Npgsql, SQL Server, MySQL, or any
other database driver. Selecting SQL Server therefore does not bring Npgsql into the
application, and selecting PostgreSQL does not bring `Microsoft.Data.SqlClient` into
it.

## Register a provider and modules

For PostgreSQL:

```csharp
services.AddHypnoTechPostgreSql<AdminDbContext>(
    configuration.GetConnectionString("HypnoTech"));

services.UseEntityFrameworkCoreForCommon();
services.UseEntityFrameworkCoreForEcommerce();
services.UseEntityFrameworkCoreForMedia(configuration);
services.UseEntityFrameworkCoreForCms();
```

For SQL Server, only the provider registration and provider project change:

```csharp
services.AddHypnoTechSqlServer<AdminDbContext>(
    configuration.GetConnectionString("HypnoTech"));
```

`UseEntityFrameworkCoreForMedia` retains the existing `Media:AudioEnabled`,
`Media:YouTubeVideosEnabled`, and `Media:PhotoAlbumsEnabled` feature flags when it
registers CMS-visible models. The other module methods register the same async
repository interfaces used by the MongoDB implementations.

An application should install one HypnoTech EF provider for one combined context. A
host that deliberately needs multiple independent contexts should compose separate
service providers or introduce its own typed repository factories; the unkeyed
`IHypnoTechDbContextFactory` represents one HypnoTech persistence context.

## Define the Admin context

The Admin host owns its edit-oriented model and migrations. Apply module contributors
before the dynamic CMS contributor so explicitly mapped module types take precedence
over the CMS registry fallback:

```csharp
public sealed class AdminDbContext(DbContextOptions<AdminDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddHypnoTechCommonModel();
        modelBuilder.AddHypnoTechEcommerceModel();
        modelBuilder.AddHypnoTechMediaModel();
        modelBuilder.AddHypnoTechCmsModel();
    }
}
```

`AddHypnoTechCmsModel` reads the host's `FormClassMapping` registrations. Consequently,
site model types, logical table names, nested lists/galleries, child references,
searchable fields, and ordering all contribute to the EF model without putting site
types in the engine.

The public Website should not reuse this context merely for convenience. It remains
free to own focused read models, a separate EF context and migrations, a different DAL,
or a different ORM. It does not need to reference CMS/Admin assemblies.

## Migrations and startup seeding

Pending migrations are applied during host startup by default. Only after migration
success does HypnoTech run registered `IHypnoTechStartupSeeder` routines in ascending
`Order`. A migration or seed failure fails startup.

`IHypnoTechStartupSeeder` is the application-level lifecycle hook. It does not replace
the CMS `ISeedRepository`: a startup seeder can call `ISeedRepository.SeedAsync` several
times to populate named JSON files after the schema is current.

Applications own migrations because their CMS types are part of the model. Keep
PostgreSQL and SQL Server migrations in provider-specific Admin host projects; do not
pretend one generated migration is portable between providers.

Deployments that apply migrations and data separately can opt out explicitly:

```csharp
services.AddHypnoTechPostgreSql<AdminDbContext>(
    connectionString,
    configureLifecycle: options =>
    {
        options.ApplyMigrationsAtStartup = false;
        options.SeedAtStartup = false;
    });
```

The default remains migrate-then-seed, so normal local and small-site deployments do
not require a manual migration command.

## Tested database targets

The inherited persistence contracts run against real containers, not EF's in-memory
provider:

- PostgreSQL 18.1 (`postgres:18.1-alpine`);
- SQL Server 2022 CU22 GDR1
  (`mcr.microsoft.com/mssql/server:2022-CU22-GDR1-ubuntu-22.04`).

The same CMS, Common, Ecommerce, and Media assertions run for both providers. Separate
lifecycle assertions prove migrate-before-seed defaults and the controlled-deployment
opt-out on both engines.
