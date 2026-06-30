using Microsoft.Extensions.Configuration;

namespace HypnoTech.Cms.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string UploadPath(this IConfiguration configuration)
        {
            return configuration.GetSection("UploadPath").Value;
        }
    }
}
