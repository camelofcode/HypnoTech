using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace HypnoTech.Common.Extensions
{
    public static class ConfigurationExtensions
    {
        public static bool IsDal(this IConfiguration configuration, string dal)
        {
            return configuration.GetSection("Dal").Value.ToLower() == dal;
        }

        public static string UploadPath(this IConfiguration configuration)
        {
            var configuredPath = configuration.GetSection("UploadPath").Value;
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                throw new InvalidOperationException("The UploadPath configuration value is required.");
            }

            var expandedPath = Environment.ExpandEnvironmentVariables(configuredPath.Trim());
            if (expandedPath == "~" ||
                (expandedPath.Length > 1 && expandedPath[0] == '~' &&
                 (expandedPath[1] == '/' || expandedPath[1] == '\\')))
            {
                var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (string.IsNullOrWhiteSpace(homePath))
                {
                    throw new InvalidOperationException("UploadPath uses '~', but the current user's home directory could not be resolved.");
                }

                expandedPath = expandedPath.Length == 1
                    ? homePath
                    : Path.Combine(homePath, expandedPath.Substring(2));
            }

            return Path.GetFullPath(expandedPath);
        }
    }
}
