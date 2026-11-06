using HypnoTech.Cms.Areas.Settings.ViewModels;
using HypnoTech.Common;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Security;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index()
        {
            var model = new UsersViewModel
            {
                Users = await _userRepository.GetAllUsersAsync(HttpContext.RequestAborted)
            };

            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var model = new EditUserViewModel(
                await _userRepository.GetUserForEditAsync(id, HttpContext.RequestAborted))
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
        public async Task<IActionResult> Add(AddUserViewModel model)
        {
            var message = model.Validate();
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return RedirectToAction("Add");
            }

            message = await _userRepository.AddUserAsync(
                model.ToAdminUser(),
                HttpContext.RequestAborted);
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
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            var message = model.Validate();
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return RedirectToAction("Edit");
            }

            message = await _userRepository.UpdateUserAsync(
                model.ToAdminUser(),
                HttpContext.RequestAborted);
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
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var message = model.Validate();
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return RedirectToAction("Edit", new { id = model.Id });
            }

            var user = await _userRepository.ChangePasswordAsync(
                model.Id,
                PasswordHelper.HashPassword(model.Password),
                HttpContext.RequestAborted);

            _flashMessage.Confirmation($"Password changed for user '{user.Username}'");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(string id)
        {
            var user = await _userRepository.GetUserForEditAsync(id, HttpContext.RequestAborted);
            await _userRepository.RemoveUserAsync(id, HttpContext.RequestAborted);

            _flashMessage.Confirmation($"Removed user '{user.Username}'");
            return RedirectToAction("Index");
        }
    }
}
