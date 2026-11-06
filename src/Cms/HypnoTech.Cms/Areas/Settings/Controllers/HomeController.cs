using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Cms.Areas.Settings.Controllers
{
    [Area("Settings")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
