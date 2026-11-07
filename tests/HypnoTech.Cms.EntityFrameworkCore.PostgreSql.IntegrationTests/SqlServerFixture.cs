using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Xunit;

namespace HypnoTech.Cms.EntityFrameworkCore.PostgreSql.IntegrationTests;

public sealed class SqlServerFixture : IAsyncLifetime, IRelationalDatabaseFixture
{
    private readonly MsSqlContainer _container = new MsSqlBuilder(
            "mcr.microsoft.com/mssql/server:2022-CU22-GDR1-ubuntu-22.04")
        .Build();

    public string ProviderName => "Entity Framework Core / SQL Server";

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
    }

    public ValueTask DisposeAsync()
    {
        return _container.DisposeAsync();
    }

    public async Task<string> CreateDatabaseAsync()
    {
        var databaseName = $"hypnotech_{Guid.NewGuid():N}";
        await using var connection = new SqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE [{databaseName}]";
        await command.ExecuteNonQueryAsync();

        var builder = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = databaseName
        };
        return builder.ConnectionString;
    }

    public async Task DropDatabaseAsync(string connectionString)
    {
        var target = new SqlConnectionStringBuilder(connectionString);
        var databaseName = target.InitialCatalog;
        target.InitialCatalog = "master";

        await using var connection = new SqlConnection(target.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            $"IF DB_ID(N'{databaseName}') IS NOT NULL BEGIN " +
            $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            $"DROP DATABASE [{databaseName}]; END";
        await command.ExecuteNonQueryAsync();
    }

    public DbContextOptionsBuilder<TContext> Configure<TContext>(
        DbContextOptionsBuilder<TContext> builder,
        string connectionString,
        string migrationsAssembly = null)
        where TContext : DbContext
    {
        builder.UseSqlServer(connectionString, options =>
        {
            if (migrationsAssembly != null)
            {
                options.MigrationsAssembly(migrationsAssembly);
            }
        });
        return builder;
    }

    public DbContextOptionsBuilder Configure(
        DbContextOptionsBuilder builder,
        string connectionString,
        string migrationsAssembly = null)
    {
        builder.UseSqlServer(connectionString, options =>
        {
            if (migrationsAssembly != null)
            {
                options.MigrationsAssembly(migrationsAssembly);
            }
        });
        return builder;
    }
}
