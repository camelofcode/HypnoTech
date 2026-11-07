using System.Collections.Generic;
using System.Linq;

namespace HypnoTech.Cms.Mvc
{
    public static class AdminCmsNavbarCache
    {
        public static List<AdminCmsLink> Links { get; set; }

        static AdminCmsNavbarCache()
        {
            Links = new List<AdminCmsLink>();
        }

        public static void Add(string text, string url, string icon, string controller, string role)
        {
            if (Links == null)
            {
                Links = new List<AdminCmsLink>();
            }

            if (Links.All(p => p.Text != text))
            {
                Links.Add(new AdminCmsLink
                {
                    Controller = controller,
                    Text = text,
                    Url = $"/Cms/{url}",
                    Icon = icon,
                    Role = role
                });
            }
        }
    }
}
