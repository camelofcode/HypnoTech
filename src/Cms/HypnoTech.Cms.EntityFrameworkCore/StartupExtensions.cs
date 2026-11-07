using HypnoTech.Cms.Contracts.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HypnoTech.Cms.EntityFrameworkCore
{
    public static class StartupExtensions
    {
        public static IServiceCollection UseEntityFrameworkCoreForCms(
            this IServiceCollection services)
        {
            services.TryAddTransient(typeof(ICmsRepository<>), typeof(CmsRepository<>));
            services.TryAddTransient<ISelectListRepository, SelectListRepository>();
            services.TryAddTransient<ISeedRepository, SeedRepository>();
            return services;
        }
    }
}
