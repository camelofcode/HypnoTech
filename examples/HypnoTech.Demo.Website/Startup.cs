using System;
using HypnoTech.Common.Dal.EntityFrameworkCore;
using HypnoTech.Common.Dal.MongoDb;
using HypnoTech.Demo.Models;
using HypnoTech.Demo.Website.Dal;
using HypnoTech.Demo.Website.Persistence;
using HypnoTech.Demo.Website.Services;
using HypnoTech.Ecommerce.Dal.EntityFrameworkCore;
using HypnoTech.Ecommerce.Dal.MongoDb;
using HypnoTech.Ecommerce.Frontend.Startup;
using HypnoTech.EntityFrameworkCore.PostgreSql;
using HypnoTech.EntityFrameworkCore.SqlServer;
using HypnoTech.Frontend.Startup;
using HypnoTech.Media.Dal.EntityFrameworkCore;
using HypnoTech.Media.Dal.MongoDb;
using HypnoTech.Media.Frontend;
using HypnoTech.MongoDb;
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
        DemoModelMappings.Map();

        services.AddHttpContextAccessor();
        services.UseHypnoTech(Environment);
        services.AddMediaFrontend<DemoAudioTrackEnrichmentService>(Configuration);
        services.AddEcommerce(Configuration);

        switch (Configuration.GetValue("Demo:Provider", "MongoDb"))
        {
            case "MongoDb":
                DemoMongoModelMappings.Map();
                services.Configure<MongoOptions>(Configuration.GetSection("Mongo"));
                services.AddTransient<IArticleRepository, MongoArticleRepository>();
                services.AddTransient<IArtistRepository, MongoArtistRepository>();
                services.AddTransient<IEventRepository, MongoEventRepository>();
                services.UseMongoDbForCommon(Configuration);
                services.UseMongoDbForEcommerce(Configuration);
                services.UseMongoDbForMedia(Configuration);
                break;

            case "PostgreSql":
                var postgresConnectionString = Configuration.GetConnectionString("PostgreSql") ??
                                               throw new InvalidOperationException(
                                                   "ConnectionStrings:PostgreSql is required for the PostgreSQL demo provider.");
                services.AddHypnoTechPostgreSql<PostgreSqlDemoWebsiteDbContext>(
                    postgresConnectionString,
                    configureLifecycle: lifecycle =>
                    {
                        // The Admin owns the schema and runs migrations before seeding.
                        lifecycle.ApplyMigrationsAtStartup = false;
                        lifecycle.SeedAtStartup = false;
                    });
                services.AddTransient<IArticleRepository, EntityFrameworkCoreArticleRepository>();
                services.AddTransient<IArtistRepository, EntityFrameworkCoreArtistRepository>();
                services.AddTransient<IEventRepository, EntityFrameworkCoreEventRepository>();
                services.UseEntityFrameworkCoreForCommon();
                services.UseEntityFrameworkCoreForEcommerce();
                services.UseEntityFrameworkCoreForMedia(Configuration);
                break;

            case "SqlServer":
                var sqlServerConnectionString = Configuration.GetConnectionString("SqlServer") ??
                                                throw new InvalidOperationException(
                                                    "ConnectionStrings:SqlServer is required for the SQL Server demo provider.");
                services.AddHypnoTechSqlServer<SqlServerDemoWebsiteDbContext>(
                    sqlServerConnectionString,
                    configureLifecycle: lifecycle =>
                    {
                        // The Admin owns the schema and runs migrations before seeding.
                        lifecycle.ApplyMigrationsAtStartup = false;
                        lifecycle.SeedAtStartup = false;
                    });
                services.AddTransient<IArticleRepository, EntityFrameworkCoreArticleRepository>();
                services.AddTransient<IArtistRepository, EntityFrameworkCoreArtistRepository>();
                services.AddTransient<IEventRepository, EntityFrameworkCoreEventRepository>();
                services.UseEntityFrameworkCoreForCommon();
                services.UseEntityFrameworkCoreForEcommerce();
                services.UseEntityFrameworkCoreForMedia(Configuration);
                break;

            default:
                throw new InvalidOperationException(
                    "Demo:Provider must be MongoDb, PostgreSql, or SqlServer.");
        }
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
