using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Common.Security;
using Microsoft.AspNetCore.Mvc;

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

        public IActionResult Index()
        {
            return View(_configAdminRepository.GetConfig());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(Config config)
        {
            _configAdminRepository.SetConfig(config);
            return Ok();
        }
    }
}
