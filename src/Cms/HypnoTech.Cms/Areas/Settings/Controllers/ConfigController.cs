using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Common.Security;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Areas.Settings.Controllers
{
    [Area("Settings")]
    [RoleAuthorize("config")]
    public class ConfigController : Controller
    {
        private readonly IConfigAdminRepository _configAdminRepository;

        public ConfigController(IConfigAdminRepository configAdminRepository)
        {
            _configAdminRepository = configAdminRepository;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _configAdminRepository.GetConfigAsync(HttpContext.RequestAborted));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Config config)
        {
            await _configAdminRepository.SetConfigAsync(config, HttpContext.RequestAborted);
            return Ok();
        }
    }
}
