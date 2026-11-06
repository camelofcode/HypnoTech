using HypnoTech.Cms.MongoDb;
using HypnoTech.Cms.Startup;
using HypnoTech.Common.Dal.MongoDb;
using HypnoTech.Demo.Models;
using HypnoTech.Ecommerce.Dal.MongoDb;
using HypnoTech.Ecommerce.Startup;
using HypnoTech.Media.Dal.MongoDb;
using HypnoTech.Media.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HypnoTech.Demo.Admin;

public sealed class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        DemoCmsMappings.Register();
        services.AddEcommerceAdmin(Configuration);
        services.AddMediaAdmin(Configuration);
        services.AddCms(Configuration, _ => { });

        services.UseMongoDbForCms(Configuration);
        services.UseMongoDbForCommon(Configuration);
        services.UseMongoDbForEcommerce(Configuration);
        services.UseMongoDbForMedia(Configuration);

        DemoModelMappings.Map();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        app.ConfigureCms(environment, Configuration);

        if (Configuration.GetValue("Demo:SeedOnStartup", true))
        {
            DemoSeeder.Seed(app.ApplicationServices, environment);
        }
    }
}
