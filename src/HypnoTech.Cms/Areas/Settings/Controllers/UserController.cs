using HypnoTech.Cms.Areas.Settings.ViewModels;
using HypnoTech.Common;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Security;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace HypnoTech.Cms.Areas.Settings.Controllers
{
    [Area("Settings")]
    [RoleAuthorize("users")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IFlashMessage _flashMessage;

        public UserController(IUserRepository userRepository, IFlashMessage flashMessage)
        {
            _userRepository = userRepository;
            _flashMessage = flashMessage;
        }

        public IActionResult Index()
        {
            var model = new UsersViewModel
            {
                Users = _userRepository.GetAllUsers()
            };

            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var model = new EditUserViewModel(_userRepository.GetUserForEdit(id))
            {
                AvailableRoles = RoleStore.GetAllRoles()
            };
            return View(model);
        }

        public IActionResult Add()
        {
            var model = new AddUserViewModel
            {
                AvailableRoles = RoleStore.GetAllRoles()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(AddUserViewModel model)
        {
            var message = model.Validate();
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return RedirectToAction("Add");
            }

            message = _userRepository.AddUser(model.ToAdminUser());
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return RedirectToAction("Add");
            }

            _flashMessage.Confirmation($"User '{model.Username}' added");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditUserViewModel model)
        {
            var message = model.Validate();
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return RedirectToAction("Edit");
            }

            message = _userRepository.UpdateUser(model.ToAdminUser());
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return RedirectToAction("Edit");
            }

            _flashMessage.Confirmation($"User '{model.Username}' updated");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var message = model.Validate();
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return RedirectToAction("Edit", new { id = model.Id });
            }

            var user = _userRepository.ChangePassword(model.Id, PasswordHelper.HashPassword(model.Password));

            _flashMessage.Confirmation($"Password changed for user '{user.Username}'");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(string id)
        {
            var user = _userRepository.GetUserForEdit(id);
            _userRepository.RemoveUser(id);

            _flashMessage.Confirmation($"Removed user '{user.Username}'");
            return RedirectToAction("Index");
        }
    }
}