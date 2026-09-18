using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace HypnoTech.Common.Security
{
    public static class StartupExtensions
    {
        public static void UseSecureResponseHeaders(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.Use((context, next) =>
            {
                context.Response.Headers["X-Frame-Options"] = "deny";

                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

                //https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";

                if (context.Request.Headers.ContainsKey("Origin"))
                {
                    var origin = context.Request.Headers["Origin"].FirstOrDefault();
                    context.Response.Headers["Access-Control-Allow-Origin"] =
                    HostUrls(configuration).Contains(origin, StringComparer.CurrentCultureIgnoreCase)
                    ? origin
                    : $"https://{context.Request.Host}";
                }
                else if (context.Request.Headers.ContainsKey("Referer"))
                {
                    var referer = $"https://{new Uri(context.Request.Headers["Referer"].First()).Host}";
                    context.Response.Headers["Access-Control-Allow-Origin"] =
                    HostUrls(configuration).Contains(referer, StringComparer.CurrentCultureIgnoreCase)
                    ? referer
                    : $"https://{context.Request.Host}";
                }
                else
                {
                    context.Response.Headers["Access-Control-Allow-Origin"] = $"https://{context.Request.Host}";
                }

                context.Response.Headers["Access-Control-Allow-Methods"] = "POST, GET, OPTIONS";

                var csp = configuration["ContentSecurityPolicy"];
                if (string.IsNullOrEmpty(csp))
                {
                    csp = BuildContentSecurityPolicy(configuration);
                }

                context.Response.Headers["Content-Security-Policy"] = csp;

                context.Response.Headers["X-UA-Compatible"] = "IE=EDGE";

                return next();
            });
        }

        private static string BuildContentSecurityPolicy(IConfiguration configuration)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"default-src 'self' {string.Join(" ", HostUrls(configuration))} {AppendDirective(configuration, "default-src")};");
            sb.Append($"img-src 'self' data: {string.Join(" ", HostUrls(configuration))} {AppendDirective(configuration, "img-src")};");
            sb.Append($"style-src 'self' 'unsafe-inline' {string.Join(" ", HostUrls(configuration))} {AppendDirective(configuration, "style-src")};");
            sb.Append($"script-src 'self' 'unsafe-inline' 'unsafe-eval' {string.Join(" ", HostUrls(configuration))} {AppendDirective(configuration, "script-src")};");

            var additionalDirectives = new[] {"child-src", "connect-src", "font-src", "frame-src", "manifest-src", "media-src", "object-src", "prefetch-src", "worker-src" };
            foreach (var additionalDirective in additionalDirectives)
            {
                var directive = BuildDirective(configuration, additionalDirective);
                if (!string.IsNullOrEmpty(directive))
                {
                    sb.Append(directive);
                }
            }
            return sb.ToString();
        }

        private static List<string> HostUrls(IConfiguration configuration)
        {
            var hostUrl = configuration["HostURL"];
            if (hostUrl.Contains("localhost"))
            {
                return new List<string> { "http://localhost", "https://localhost" };
            }

            return new List<string> { hostUrl };
        }

        private static string AppendDirective(IConfiguration config, string directive)
        {
            var directiveValue = config[$"ContentSecurityPolicy:{directive}"];
            if (!string.IsNullOrEmpty(directiveValue))
            {
                return directiveValue;
            }

            return string.Empty;
        }

        private static string BuildDirective(IConfiguration config, string directive)
        {
            var directiveValue = config[$"ContentSecurityPolicy:{directive}"];
            if (!string.IsNullOrEmpty(directiveValue))
            {
                return $"{directive} 'self' {string.Join(" ", HostUrls(config))}  {directiveValue};";
            }

            return string.Empty;
        }
    }
}
