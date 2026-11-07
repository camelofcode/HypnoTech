using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Frontend.Attributes;
using HypnoTech.Frontend.Extensions;
using Serilog;

namespace HypnoTech.Frontend.Caching
{
    public static class NavBarCache
    {
        public static List<NavBarLink> CachedLinks { get; private set; }

        static NavBarCache()
        {
            CachedLinks = new List<NavBarLink>();
        }

        public static async Task InitAsync(
            INavBarRepository repository,
            CancellationToken cancellationToken = default)
        {
            var navigationLinks = GetNavigationLinks();
            var enabledLinks = await GetEnabledLinksAsync(repository, cancellationToken);

            PopulateUrls(navigationLinks, enabledLinks);
            CachedLinks = enabledLinks;
        }

        private static List<MethodInfo> GetNavigationLinks()
        {
            Assembly ass = Assembly.GetEntryAssembly();
            return ass.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(NavigationLinkAttribute), false).Length > 0)
                .ToList();
        }

        private static void PopulateUrls(
            List<MethodInfo> navigationLinks,
            List<NavBarLink> enabledLinks)
        {
            foreach (var enabledLink in enabledLinks)
            {
                enabledLink.Url = navigationLinks.FirstOrDefault(p => p.GetNavigationLinkId() == enabledLink.Key)?.GetUrl();
            }
        }

        private static async Task<List<NavBarLink>> GetEnabledLinksAsync(
            INavBarRepository repository,
            CancellationToken cancellationToken)
        {
            try
            {
                return await repository.GetEnabledNavBarLinksAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                Log.Logger.Warning("NavBar could not be loaded from Data Store");
                return new List<NavBarLink>();
            }
        }
    }
}
