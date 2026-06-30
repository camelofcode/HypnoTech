using HypnoTech.Common.Extensions;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace HypnoTech.MongoDb
{
    public static partial class SerilogSinks
    {
        public static void MongoDb(LoggerConfiguration config, HostBuilderContext context, string siteName)
        {
            var connectionString = $"mongodb://{context.Configuration["Mongo:Server"]}:{context.Configuration["Mongo:Port"]}/{context.Configuration["Mongo:Database"]}";

            config
                .Enrich.WithSiteName(siteName)
                .WriteTo.MongoDBCapped(connectionString,
                restrictedToMinimumLevel: LogEventLevel.Warning,
                cappedMaxSizeMb: 50,
                cappedMaxDocuments: 1000,
                collectionName: "serilog");
        }
    }
}
