using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace HypnoTech.EntityFrameworkCore.PostgreSql
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddHypnoTechPostgreSql<TContext>(
            this IServiceCollection services,
            string connectionString,
            Action<NpgsqlDbContextOptionsBuilder> configureProvider = null,
            Action<HypnoTechEntityFrameworkCoreOptions> configureLifecycle = null)
            where TContext : DbContext
        {
            return services.AddHypnoTechEntityFrameworkCore<TContext>(
                options => options.UseNpgsql(connectionString, providerOptions =>
                {
                    configureProvider?.Invoke(providerOptions);
                }),
                configureLifecycle);
        }
    }
}
