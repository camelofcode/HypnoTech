using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Cms.EntityFrameworkCore.PostgreSql.IntegrationTests;

public interface IRelationalDatabaseFixture
{
    string ProviderName { get; }

    Task<string> CreateDatabaseAsync();

    Task DropDatabaseAsync(string connectionString);

    DbContextOptionsBuilder<TContext> Configure<TContext>(
        DbContextOptionsBuilder<TContext> builder,
        string connectionString,
        string migrationsAssembly = null)
        where TContext : DbContext;

    DbContextOptionsBuilder Configure(
        DbContextOptionsBuilder builder,
        string connectionString,
        string migrationsAssembly = null);
}
