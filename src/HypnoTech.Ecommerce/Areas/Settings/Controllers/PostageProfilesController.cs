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

        public IActionResult Index()
        {
            var model = new PostageProfilesViewModel
            {
                Profiles = _repository.GetProfiles()
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
        public IActionResult Add(PostageProfile profile)
        {
            var message = profile.Validate();
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return View(profile);
            }

            _repository.AddProfile(profile);
            _flashMessage.Confirmation($"Postage Profile '{profile.Name}' added.");
            return RedirectToAction("Index");
        }

        public IActionResult Edit(string id)
        {
            return View(_repository.GetProfileForEdit(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PostageProfile profile)
        {
            var message = profile.Validate();
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return RedirectToAction("Edit", new { id = profile.Id });
            }

            _repository.UpdateProfile(profile);
            _flashMessage.Confirmation($"Postage Profile '{profile.Name}' updated.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(string id)
        {
            var profile = _repository.GetProfileForEdit(id);
            var message = _repository.RemoveProfile(id);
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
