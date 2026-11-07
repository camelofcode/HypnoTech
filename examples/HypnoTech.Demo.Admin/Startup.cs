using System;
using HypnoTech.Cms.EntityFrameworkCore;
using HypnoTech.Cms.MongoDb;
using HypnoTech.Cms.Startup;
using HypnoTech.Common.Dal.EntityFrameworkCore;
using HypnoTech.Common.Dal.MongoDb;
using HypnoTech.Demo.Admin.Persistence;
using HypnoTech.Demo.Models;
using HypnoTech.Ecommerce.Dal.EntityFrameworkCore;
using HypnoTech.Ecommerce.Dal.MongoDb;
using HypnoTech.Ecommerce.Startup;
using HypnoTech.EntityFrameworkCore;
using HypnoTech.EntityFrameworkCore.PostgreSql;
using HypnoTech.EntityFrameworkCore.SqlServer;
using HypnoTech.Media.Dal.EntityFrameworkCore;
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
        DemoModelMappings.Map();
        services.AddEcommerceAdmin(Configuration);
        services.AddMediaAdmin(Configuration);
        services.AddCms(Configuration, _ => { });

        switch (Configuration.GetValue("Demo:Provider", "MongoDb"))
        {
            case "MongoDb":
                DemoMongoModelMappings.Map();
                services.UseMongoDbForCms(Configuration);
                services.UseMongoDbForCommon(Configuration);
                services.UseMongoDbForEcommerce(Configuration);
                services.UseMongoDbForMedia(Configuration);
                if (Configuration.GetValue("Demo:SeedOnStartup", true))
                {
                    services.AddHostedService<DemoSeederHostedService>();
                }

                break;

            case "PostgreSql":
                var postgresConnectionString = Configuration.GetConnectionString("PostgreSql") ??
                                               throw new InvalidOperationException(
                                                   "ConnectionStrings:PostgreSql is required for the PostgreSQL demo provider.");
                services.AddHypnoTechPostgreSql<PostgreSqlDemoAdminDbContext>(
                    postgresConnectionString,
                    provider => provider.MigrationsAssembly(
                        typeof(PostgreSqlDemoAdminDbContext).Assembly.FullName));
                services.UseEntityFrameworkCoreForCommon();
                services.UseEntityFrameworkCoreForEcommerce();
                services.UseEntityFrameworkCoreForMedia(Configuration);
                services.UseEntityFrameworkCoreForCms();
                if (Configuration.GetValue("Demo:SeedOnStartup", true))
                {
                    services.AddSingleton<IHypnoTechStartupSeeder, DemoRelationalSeeder>();
                }

                break;

            case "SqlServer":
                var sqlServerConnectionString = Configuration.GetConnectionString("SqlServer") ??
                                                throw new InvalidOperationException(
                                                    "ConnectionStrings:SqlServer is required for the SQL Server demo provider.");
                services.AddHypnoTechSqlServer<SqlServerDemoAdminDbContext>(
                    sqlServerConnectionString,
                    provider => provider.MigrationsAssembly(
                        typeof(SqlServerDemoAdminDbContext).Assembly.FullName));
                services.UseEntityFrameworkCoreForCommon();
                services.UseEntityFrameworkCoreForEcommerce();
                services.UseEntityFrameworkCoreForMedia(Configuration);
                services.UseEntityFrameworkCoreForCms();
                if (Configuration.GetValue("Demo:SeedOnStartup", true))
                {
                    services.AddSingleton<IHypnoTechStartupSeeder, DemoRelationalSeeder>();
                }

                break;

            default:
                throw new InvalidOperationException(
                    "Demo:Provider must be MongoDb, PostgreSql, or SqlServer.");
        }
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        app.ConfigureCms(environment, Configuration);
    }
}
