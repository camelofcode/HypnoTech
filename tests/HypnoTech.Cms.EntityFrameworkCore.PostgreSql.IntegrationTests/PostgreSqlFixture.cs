using System;
using System.Threading.Tasks;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace HypnoTech.Cms.EntityFrameworkCore.PostgreSql.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime, IRelationalDatabaseFixture
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.1-alpine")
        .WithDatabase("hypnotech")
        .WithUsername("hypnotech")
        .WithPassword("hypnotech-tests")
        .Build();

    public string ProviderName => "Entity Framework Core / PostgreSQL";

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
        await using var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
        await command.ExecuteNonQueryAsync();

        var builder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = databaseName
        };
        return builder.ConnectionString;
    }

    public async Task DropDatabaseAsync(string connectionString)
    {
        var target = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = target.Database;
        target.Database = "hypnotech";

        await using var connection = new NpgsqlConnection(target.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)";
        await command.ExecuteNonQueryAsync();
    }

    public DbContextOptionsBuilder<TContext> Configure<TContext>(
        DbContextOptionsBuilder<TContext> builder,
        string connectionString,
        string migrationsAssembly = null)
        where TContext : DbContext
    {
        builder.UseNpgsql(connectionString, options =>
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
        builder.UseNpgsql(connectionString, options =>
        {
            if (migrationsAssembly != null)
            {
                options.MigrationsAssembly(migrationsAssembly);
            }
        });
        return builder;
    }
}
