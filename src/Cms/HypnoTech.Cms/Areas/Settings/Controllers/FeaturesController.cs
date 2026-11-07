using System.Collections.Generic;
using HypnoTech.Cms.Areas.Settings.ViewModels;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Common.Security;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Areas.Settings.Controllers
{
    [Area("Settings")]
    [RoleAuthorize("features")]
    public class FeaturesController : Controller
    {
        private readonly INavBarAdminRepository _navBarRepository;
        private readonly IFlashMessage _flashMessage;
        private readonly IApiService _apiService;

        public FeaturesController(INavBarAdminRepository navBarRepository, IFlashMessage flashMessage, IApiService apiService)
        {
            _navBarRepository = navBarRepository;
            _flashMessage = flashMessage;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            return View(new FeaturesViewModel
            {
                Links = await _navBarRepository.GetNavBarLinksAsync(HttpContext.RequestAborted)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromForm] List<NavBarLink> links)
        {
            await _navBarRepository.UpdateNavBarLinksAsync(links, HttpContext.RequestAborted);
            await _apiService.ClearSiteNavbarCacheAsync(HttpContext.RequestAborted);
            _flashMessage.Confirmation("Features Updated");
            return Ok();
        }
    }
}
