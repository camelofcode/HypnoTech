using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common.Dal.Repositories;
using Serilog;

namespace HypnoTech.Cms.Services
{
    public class ApiService : IApiService
    {
        private readonly IConfigAdminRepository _config;

        public ApiService(IConfigAdminRepository config)
        {
            _config = config;
        }

        public void ClearSiteNavbarCache()
        {
            try
            {
                using var client = BuildClient(_config.GetConfig().MainSite);
                using var request = new HttpRequestMessage(HttpMethod.Get, "ClearNavbarCache");
                using var response = client.Send(request);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error");
            }
        }

        public async Task ClearSiteNavbarCacheAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var config = await _config.GetConfigAsync(cancellationToken);
                using var client = BuildClient(config.MainSite);
                await client.GetStringAsync("ClearNavbarCache", cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error");
            }
        }

        private static HttpClient BuildClient(HypnoTech.Common.Models.Settings.MainSiteConfig siteConfig)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri($"{siteConfig.Url}api/")
            };
            client.DefaultRequestHeaders.Add("X-Auth-Token", siteConfig.ApiKey);
            return client;
        }
    }
}
