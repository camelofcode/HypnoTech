using System;
using System.Net.Http;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common.Dal.Repositories;
using Serilog;

namespace HypnoTech.Cms.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _client;

        public ApiService(IConfigAdminRepository config)
        {
            var handler = new HttpClientHandler();
            var siteConfig = config.GetConfig().MainSite;
            _client = new HttpClient(handler)
            {
                BaseAddress = new Uri($"{siteConfig.Url}api/")
            };
            _client.DefaultRequestHeaders.Add("X-Auth-Token", siteConfig.ApiKey);
        }

        public void ClearSiteNavbarCache()
        {
            try
            {
                _client.GetStringAsync("ClearNavbarCache").Wait();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error");
            }
        }
    }
}
