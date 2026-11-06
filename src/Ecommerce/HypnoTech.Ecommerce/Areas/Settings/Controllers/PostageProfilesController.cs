using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HypnoTech.Common.Security;
using HypnoTech.Ecommerce.Areas.Settings.Models;
using HypnoTech.Ecommerce.Areas.Settings.Validation;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace HypnoTech.Ecommerce.Areas.Settings.Controllers
{
    [Area("Settings")]
    [RoleAuthorize("postageprofiles")]
    public class PostageProfilesController : Controller
    {
        private readonly IPostageProfileRepository _repository;
        private readonly IFlashMessage _flashMessage;

        public PostageProfilesController(IPostageProfileRepository repository, IFlashMessage flashMessage)
        {
            _repository = repository;
            _flashMessage = flashMessage;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PostageProfilesViewModel
            {
                Profiles = await _repository.GetProfilesAsync(HttpContext.RequestAborted)
            };

            return View(model);
        }

        public IActionResult Add()
        {
            var model = new PostageProfile();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(PostageProfile profile)
        {
            var message = profile.Validate();
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return View(profile);
            }

            await _repository.AddProfileAsync(profile, HttpContext.RequestAborted);
            _flashMessage.Confirmation($"Postage Profile '{profile.Name}' added.");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(string id)
        {
            return View(await _repository.GetProfileForEditAsync(
                id,
                HttpContext.RequestAborted));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostageProfile profile)
        {
            var message = profile.Validate();
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return RedirectToAction("Edit", new { id = profile.Id });
            }

            await _repository.UpdateProfileAsync(profile, HttpContext.RequestAborted);
            _flashMessage.Confirmation($"Postage Profile '{profile.Name}' updated.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(string id)
        {
            var profile = await _repository.GetProfileForEditAsync(
                id,
                HttpContext.RequestAborted);
            var message = await _repository.RemoveProfileAsync(
                id,
                HttpContext.RequestAborted);
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
            }
            else
            {
                _flashMessage.Confirmation($"Postage Profile '{profile.Name}' removed.");
            }
            return RedirectToAction("Index");
        }
    }
}
