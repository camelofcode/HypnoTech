using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HypnoTech.Cms.Mvc.ViewModels;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common;
using HypnoTech.Common.Dal.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace HypnoTech.Cms.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILoginService _loginService;
        private readonly IFlashMessage _flashMessage;
        private readonly IAccountRepository _accountRepository;
        private readonly IUserRepository _userRepository;
        private readonly IForgottenPasswordService _forgottenPasswordService;

        public HomeController(ILoginService loginService, IFlashMessage flashMessage, IAccountRepository accountRepository, IUserRepository userRepository, IForgottenPasswordService forgottenPasswordService)
        {
            _loginService = loginService;
            _flashMessage = flashMessage;
            _accountRepository = accountRepository;
            _userRepository = userRepository;
            _forgottenPasswordService = forgottenPasswordService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string nameOrEmail)
        {

            if (await _forgottenPasswordService.CreatePasswordResetIfUserExistsAsync(
                    nameOrEmail,
                    HttpContext.RequestAborted))
            {
                return RedirectToAction("ForgotPasswordEmailSent");
            }

            _flashMessage.Danger("Your Username or Email was not recognised");
            return RedirectToAction("ForgotPassword");
        }

        [AllowAnonymous]
        public IActionResult ForgotPasswordEmailSent()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword(string id)
        {
            if (!await _forgottenPasswordService.CheckCodeAsync(id, HttpContext.RequestAborted))
            {
                return RedirectToAction("InvalidOrExpiredCode");
            }

            return View(new ForgottenPasswordViewModel { Code = id });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ForgottenPasswordViewModel model)
        {
            if (!await _forgottenPasswordService.CheckCodeAsync(
                    model.Code,
                    HttpContext.RequestAborted))
            {
                return RedirectToAction("InvalidOrExpiredCode");
            }

            var message = model.Validate();
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return RedirectToAction("ChangePassword", new { id = model.Code });
            }

            await _forgottenPasswordService.ResetPasswordAsync(
                model.Code,
                model.Password,
                HttpContext.RequestAborted);

            _flashMessage.Confirmation("Password Changed", "Your password has been changed. Please log in.");

            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult InvalidOrExpiredCode()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (await _loginService.VerifyUserAsync(
                    username,
                    password,
                    HttpContext.RequestAborted))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                };

                foreach (var role in await _loginService.GetRolesAsync(
                             username,
                             HttpContext.RequestAborted))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = true
                };

                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties);

                HttpContext.User = claimsPrincipal;

                return RedirectToAction("Index");
            }

            _flashMessage.Danger("Login Failed", "User name or password was incorrect");
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            _flashMessage.Confirmation("Logout Successful", "You have been logged out of the admin application");
            return RedirectToAction("Login");
        }


        public async Task<IActionResult> Account()
        {
            return View(await _accountRepository.GetUserByNameAsync(
                HttpContext.User.Identity.Name,
                HttpContext.RequestAborted));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<RedirectToActionResult> UpdateAccount(ChangeAccountDetailsViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Password) && await _loginService.VerifyUserAsync(
                    HttpContext.User.Identity.Name,
                    model.Password,
                    HttpContext.RequestAborted))
            {
                var existingUser = await _userRepository.GetUserForEditAsync(
                    model.Id,
                    HttpContext.RequestAborted);

                var message = model.Validate();
                if (!string.IsNullOrEmpty(message))
                {
                    _flashMessage.Danger(message);
                    return RedirectToAction("Account");
                }

                message = await _accountRepository.UpdateAccountAsync(
                    model.ToAdminUser(),
                    HttpContext.RequestAborted);
                if (!string.IsNullOrEmpty(message))
                {
                    _flashMessage.Danger(message);
                    return RedirectToAction("Account");
                }

                if (existingUser.Username != model.Username)
                {
                    await HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    _flashMessage.Confirmation("Username changed", "You have been logged out of the admin application. Please login with your new username");
                    return RedirectToAction("Login");
                }

                _flashMessage.Confirmation("Account Changed");
                return RedirectToAction("Account");

            }

            _flashMessage.Danger("Password entered did not match records.");
            return RedirectToAction("Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<RedirectToActionResult> ChangePassword(ChangeAccountPasswordViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Password) && await _loginService.VerifyUserAsync(
                    HttpContext.User.Identity.Name,
                    model.Password,
                    HttpContext.RequestAborted))
            {
                var message = model.Validate();
                if (!string.IsNullOrEmpty(message))
                {
                    _flashMessage.Danger(message);
                    return RedirectToAction("Account");
                }

                await _accountRepository.ChangePasswordAsync(
                    model.Id,
                    PasswordHelper.HashPassword(model.NewPassword),
                    HttpContext.RequestAborted);

                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                _flashMessage.Confirmation("Password changed", "You have been logged out of the admin application. Please login with your new password");
                return RedirectToAction("Login");
            }

            _flashMessage.Danger("Password entered did not match records.");
            return RedirectToAction("Account");
        }

    }
}
