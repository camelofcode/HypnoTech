using System.Linq;
using HypnoTech.Cms.MongoDb;
using HypnoTech.Common.Dal.MongoDb;
using HypnoTech.Demo.Models;
using HypnoTech.Demo.Website.Services;
using HypnoTech.Ecommerce.Dal.MongoDb;
using HypnoTech.Ecommerce.Frontend.Startup;
using HypnoTech.Frontend.Startup;
using HypnoTech.Media.Dal.MongoDb;
using HypnoTech.Media.Frontend;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HypnoTech.Demo.Website;

public sealed class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public IConfiguration Configuration { get; }

    public IWebHostEnvironment Environment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        DemoCmsMappings.Register();
        DemoModelMappings.Map();

        services.AddHttpContextAccessor();
        services.UseHypnoTech(Environment);
        services.AddMvc().ConfigureApplicationPartManager(manager =>
        {
            var cmsAssemblyName = typeof(HypnoTech.Cms.Startup.StartupBuilderExtensions).Assembly.GetName().Name;
            var cmsParts = manager.ApplicationParts
                .Where(part => part.Name == cmsAssemblyName)
                .ToList();
            foreach (var part in cmsParts)
            {
                manager.ApplicationParts.Remove(part);
            }
        });
        services.AddMediaFrontend<DemoAudioTrackEnrichmentService>(Configuration);
        services.AddEcommerce(Configuration);

        services.UseMongoDbForCms(Configuration);
        services.UseMongoDbForCommon(Configuration);
        services.UseMongoDbForEcommerce(Configuration);
        services.UseMongoDbForMedia(Configuration);
    }

    public void Configure(IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (DemoPaymentSafety.IsRealPaymentPath(context.Request.Path))
            {
                context.Response.StatusCode = StatusCodes.Status410Gone;
                await context.Response.WriteAsync(
                    "Real payment routes are disabled in the public HypnoTech demo. No payment was attempted.");
                return;
            }

            await next();
        });

        app.ConfigureHypnoTech(Environment, Configuration);
    }

}
