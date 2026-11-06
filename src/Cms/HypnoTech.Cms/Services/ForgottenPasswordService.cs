using HypnoTech.Cms.Models;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Emails;
using HypnoTech.Common.Models.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace HypnoTech.Cms.Services
{
    public class ForgottenPasswordService : IForgottenPasswordService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetRepository _passwordResetRepository;
        private readonly IEmailService _emailService;
        private readonly CmsOptions _options;

        public ForgottenPasswordService(IUserRepository userRepository, IPasswordResetRepository passwordResetRepository, IEmailService emailService, IOptions<CmsOptions> options)
        {
            _userRepository = userRepository;
            _passwordResetRepository = passwordResetRepository;
            _emailService = emailService;
            _options = options.Value;
        }

        public bool CreatePasswordResetIfUserExists(string nameOrEmail)
        {
            AdminUser user = _userRepository.GetUserByUsernameOrEmail(nameOrEmail);
            if (user == null)
            {
                return false;
            }

            var code = _passwordResetRepository.CreateResetPasswordCode(user);

            _emailService.SendEmail(user.Email, "PasswordReset", "PasswordReset", new PasswordResetEmailViewModel($"{_options.AdminUrl}/Home/ChangePassword/{code}", user.Username));

            return true;
        }

        public bool CheckCode(string code)
        {
            var reset = _passwordResetRepository.GetCode(code);
            return reset != null && reset.Valid;
        }

        public void ResetPassword(string code, string password)
        {
            var email = _passwordResetRepository.GetCode(code).Email;
            _userRepository.ChangePasswordByEmail(email, PasswordHelper.HashPassword(password));
            _passwordResetRepository.RemoveCode(code);
        }
    }
}
