using HypnoTech.Common.Models;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HypnoTech.Ecommerce.Dal.EntityFrameworkCore
{
    public static class StartupExtensions
    {
        public static IServiceCollection UseEntityFrameworkCoreForEcommerce(
            this IServiceCollection services)
        {
            ModelRegistry.Register(typeof(MerchItem), "merches", "Merch");
            ModelRegistry.Register(typeof(MerchCategoryType), "merchcategorytypes", "Merch Categories");
            ModelRegistry.Register(typeof(PostageProfile), "postageprofiles", "Postage Profiles");
            ModelRegistry.Register(typeof(DownloadCodeCollection), "downloadcodes", "Download Codes");

            services.TryAddTransient<IMerchCategoryTypeRepository, MerchCategoryTypeRepository>();
            services.TryAddTransient<IMerchRepository, MerchRepository>();
            services.TryAddTransient<IPostageProfileRepository, PostageProfileRepository>();
            services.TryAddTransient<IOrderRepository, OrderRepository>();
            services.TryAddTransient<IDownloadCodeRepository, DownloadCodeRepository>();
            return services;
        }
    }
}
