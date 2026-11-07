using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace HypnoTech.EntityFrameworkCore.SqlServer
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddHypnoTechSqlServer<TContext>(
            this IServiceCollection services,
            string connectionString,
            Action<SqlServerDbContextOptionsBuilder> configureProvider = null,
            Action<HypnoTechEntityFrameworkCoreOptions> configureLifecycle = null)
            where TContext : DbContext
        {
            return services.AddHypnoTechEntityFrameworkCore<TContext>(
                options => options.UseSqlServer(connectionString, providerOptions =>
                {
                    configureProvider?.Invoke(providerOptions);
                }),
                configureLifecycle);
        }
    }
}
