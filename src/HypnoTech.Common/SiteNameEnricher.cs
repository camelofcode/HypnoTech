using System.Runtime.CompilerServices;
using Serilog.Core;
using Serilog.Events;

namespace HypnoTech.Common
{
    public class SiteNameEnricher : ILogEventEnricher
    {
        internal static string SiteName { get; set; }

        private LogEventProperty _cachedProperty;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(GetLogEventProperty(propertyFactory));
        }

        private LogEventProperty GetLogEventProperty(ILogEventPropertyFactory factory)
        {
            return _cachedProperty ??= CreateProperty(factory);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static LogEventProperty CreateProperty(ILogEventPropertyFactory factory)
        {
            return factory.CreateProperty("SiteName", SiteName);
        }
    }
}
