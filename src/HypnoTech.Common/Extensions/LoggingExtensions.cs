using System;
using Serilog;
using Serilog.Configuration;

namespace HypnoTech.Common.Extensions
{
    public static class LoggingExtensions
    {
        public static LoggerConfiguration WithSiteName(this LoggerEnrichmentConfiguration enrich, string siteName)
        {
            if (enrich == null)
            {
                throw new ArgumentNullException(nameof(enrich));
            }

            SiteNameEnricher.SiteName = siteName;

            return enrich.With<SiteNameEnricher>();
        }
    }
}
