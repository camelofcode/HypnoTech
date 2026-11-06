using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public static void Init(INavBarRepository repository)
        {
            Assembly ass = Assembly.GetEntryAssembly();

            var navigationLinks = ass.GetTypes().SelectMany(t => t.GetMethods()).Where(m => m.GetCustomAttributes(typeof(NavigationLinkAttribute), false).Length > 0).ToList();

            var enabledLinks = GetEnabledLinks(repository);

            foreach (var enabledLink in enabledLinks)
            {
                enabledLink.Url = navigationLinks.FirstOrDefault(p => p.GetNavigationLinkId() == enabledLink.Key)?.GetUrl();
            }

            CachedLinks = enabledLinks;
        }

        private static List<NavBarLink> GetEnabledLinks(INavBarRepository repository)
        {
            try
            {
                return repository.GetEnabledNavBarLinks();
            }
            catch
            {
                Log.Logger.Warning("NavBar could not be loaded from Data Store");
                return new List<NavBarLink>();
            }
        }
    }
}
