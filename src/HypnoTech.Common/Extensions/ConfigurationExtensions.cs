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
            return configuration.GetSection("UploadPath").Value;
        }
    }
}
