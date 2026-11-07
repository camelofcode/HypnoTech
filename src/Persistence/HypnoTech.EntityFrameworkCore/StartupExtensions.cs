using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HypnoTech.EntityFrameworkCore
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddHypnoTechEntityFrameworkCore<TContext>(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> configureContext,
            Action<HypnoTechEntityFrameworkCoreOptions> configureLifecycle = null)
            where TContext : DbContext
        {
            services.AddDbContextFactory<TContext>(configureContext);
            services.TryAddSingleton<IHypnoTechDbContextFactory, HypnoTechDbContextFactory<TContext>>();

            if (configureLifecycle != null)
            {
                services.Configure(configureLifecycle);
            }
            else
            {
                services.AddOptions<HypnoTechEntityFrameworkCoreOptions>();
            }

            services.AddHostedService<HypnoTechDatabaseHostedService<TContext>>();
            return services;
        }
    }
}
