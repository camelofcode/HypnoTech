using HypnoTech.Common.Emails;
using HypnoTech.Ecommerce.Frontend.Services;
using HypnoTech.Ecommerce.Frontend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HypnoTech.Ecommerce.Frontend.Startup
{
    public static class StartupBuilderExtensions
    {
        public static void AddEcommerce(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IBasketService, BasketService>();
            services.AddTransient<ICheckoutService, CheckoutService>();
            services.AddTransient<IPayPalService, PayPalService>();
            services.UseEmails();
            services.Configure<ECommerceOptions>(configuration.GetSection("ecommerce"));
        }
    }
}
