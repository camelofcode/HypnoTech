using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace HypnoTech.Common.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseHypnoTechConfiguration(this IHostBuilder builder, string siteName, string[] args, Action<LoggerConfiguration, HostBuilderContext, string> persistentLogger = null)
        {
            return builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;

                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                config.AddEnvironmentVariables();

                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            }).UseSerilog((context, config) =>
            {
                config.ReadFrom.Configuration(context.Configuration)
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.With()
                    .WriteTo.Console();

                persistentLogger?.Invoke(config, context, siteName);
            });
        }
    }
}
