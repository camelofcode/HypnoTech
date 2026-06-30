using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace HypnoTech.Cms.Helpers
{
    public class LinkHelper : IViewContextAware
    {
        private ViewContext _context;

        public string GetActiveClass(string page)
        {
            if (page == (string) _context.RouteData.Values["Controller"])
            {
                return "active";
            }

            return string.Empty;
        }

        public void Contextualize(ViewContext viewContext)
        {
            _context = viewContext;
        }
    }
}
