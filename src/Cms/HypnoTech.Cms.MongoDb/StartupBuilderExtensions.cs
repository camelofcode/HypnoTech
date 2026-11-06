using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Cms.MongoDb.Repositories;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HypnoTech.Cms.MongoDb
{
    public static class StartupBuilderExtensions
    {
        public static void UseMongoDbForCms(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetSection("Mongo").Get<MongoOptions>() != null)
            {
                services.Configure<MongoOptions>(configuration.GetSection("Mongo"));
            }

            services.AddTransient(typeof(ICmsRepository<>), typeof(CmsRepository<>));
            services.AddTransient(typeof(ISelectListRepository), typeof(SelectListRepository));
            services.AddTransient(typeof(ISeedRepository), typeof(SeedRepository));
        }
    }
}
