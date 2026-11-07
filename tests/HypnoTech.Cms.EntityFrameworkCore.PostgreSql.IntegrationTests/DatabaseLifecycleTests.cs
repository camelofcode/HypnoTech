using System;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace HypnoTech.Cms.EntityFrameworkCore.PostgreSql.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class DatabaseLifecycleTests
{
    private readonly IRelationalDatabaseFixture[] _fixtures;

    public DatabaseLifecycleTests(PostgreSqlFixture postgreSql, SqlServerFixture sqlServer)
    {
        _fixtures = [postgreSql, sqlServer];
    }

    [Fact]
    public async Task Startup_applies_migrations_before_running_seeders_by_default()
    {
        foreach (var fixture in _fixtures)
        {
            var connectionString = await fixture.CreateDatabaseAsync();
            try
            {
                await using var provider = CreateServiceProvider(fixture, connectionString);
                var hostedService = Assert.Single(provider.GetServices<IHostedService>());

                await hostedService.StartAsync(TestContext.Current.CancellationToken);

                var seeder = provider.GetRequiredService<LifecycleSeeder>();
                Assert.Equal(1, seeder.Calls);
                await using var context = await provider
                    .GetRequiredService<IDbContextFactory<LifecycleDbContext>>()
                    .CreateDbContextAsync(TestContext.Current.CancellationToken);
                Assert.Equal(1, await context.Records.CountAsync(TestContext.Current.CancellationToken));
                Assert.Contains(
                    InitialLifecycleMigration.MigrationId,
                    await context.Database.GetAppliedMigrationsAsync(
                        TestContext.Current.CancellationToken));
            }
            finally
            {
                await fixture.DropDatabaseAsync(connectionString);
            }
        }
    }

    [Fact]
    public async Task Startup_migrations_and_seeding_can_be_disabled_for_controlled_deployments()
    {
        foreach (var fixture in _fixtures)
        {
            var connectionString = await fixture.CreateDatabaseAsync();
            try
            {
                await using var provider = CreateServiceProvider(
                    fixture,
                    connectionString,
                    options =>
                    {
                        options.ApplyMigrationsAtStartup = false;
                        options.SeedAtStartup = false;
                    });
                var hostedService = Assert.Single(provider.GetServices<IHostedService>());

                await hostedService.StartAsync(TestContext.Current.CancellationToken);

                Assert.Equal(0, provider.GetRequiredService<LifecycleSeeder>().Calls);
                await using var context = await provider
                    .GetRequiredService<IDbContextFactory<LifecycleDbContext>>()
                    .CreateDbContextAsync(TestContext.Current.CancellationToken);
                Assert.Empty(await context.Database.GetAppliedMigrationsAsync(
                    TestContext.Current.CancellationToken));
            }
            finally
            {
                await fixture.DropDatabaseAsync(connectionString);
            }
        }
    }

    private static ServiceProvider CreateServiceProvider(
        IRelationalDatabaseFixture fixture,
        string connectionString,
        Action<HypnoTechEntityFrameworkCoreOptions> configureLifecycle = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<LifecycleSeeder>();
        services.AddSingleton<IHypnoTechStartupSeeder>(provider =>
            provider.GetRequiredService<LifecycleSeeder>());
        services.AddHypnoTechEntityFrameworkCore<LifecycleDbContext>(
            options => fixture.Configure(
                options,
                connectionString,
                typeof(InitialLifecycleMigration).Assembly.GetName().Name),
            configureLifecycle);
        return services.BuildServiceProvider();
    }
}

public sealed class LifecycleDbContext : DbContext
{
    public LifecycleDbContext(DbContextOptions<LifecycleDbContext> options)
        : base(options)
    {
    }

    public DbSet<LifecycleRecord> Records => Set<LifecycleRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LifecycleRecord>(entity =>
        {
            entity.ToTable("LifecycleRecords");
            entity.HasKey(record => record.Id);
            entity.Property(record => record.Id).ValueGeneratedNever();
        });
    }
}

public sealed class LifecycleRecord
{
    public int Id { get; set; }
}

[DbContext(typeof(LifecycleDbContext))]
[Migration(MigrationId)]
public sealed class InitialLifecycleMigration : Migration
{
    public const string MigrationId = "202611070001_InitialLifecycle";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LifecycleRecords",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LifecycleRecords", record => record.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "LifecycleRecords");
    }
}

public sealed class LifecycleSeeder : IHypnoTechStartupSeeder
{
    private readonly IDbContextFactory<LifecycleDbContext> _contextFactory;

    public LifecycleSeeder(IDbContextFactory<LifecycleDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public int Order => 0;

    public int Calls { get; private set; }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        Calls++;
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        if (!await context.Records.AnyAsync(cancellationToken))
        {
            context.Records.Add(new LifecycleRecord { Id = 1 });
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
