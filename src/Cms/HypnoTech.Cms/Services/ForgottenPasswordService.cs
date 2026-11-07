using HypnoTech.Cms.Models;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Emails;
using HypnoTech.Common.Models.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task<bool> CreatePasswordResetIfUserExistsAsync(
            string nameOrEmail,
            CancellationToken cancellationToken = default)
        {
            AdminUser user = await _userRepository.GetUserByUsernameOrEmailAsync(
                nameOrEmail,
                cancellationToken);
            if (user == null)
            {
                return false;
            }

            var code = await _passwordResetRepository.CreateResetPasswordCodeAsync(
                user,
                cancellationToken);
            await _emailService.SendEmailAsync(
                user.Email,
                "PasswordReset",
                "PasswordReset",
                new PasswordResetEmailViewModel($"{_options.AdminUrl}/Home/ChangePassword/{code}", user.Username),
                cancellationToken);

            return true;
        }

        public async Task<bool> CheckCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            var reset = await _passwordResetRepository.GetCodeAsync(code, cancellationToken);
            return reset != null && reset.Valid;
        }

        public async Task ResetPasswordAsync(string code, string password, CancellationToken cancellationToken = default)
        {
            var reset = await _passwordResetRepository.GetCodeAsync(code, cancellationToken);
            await _userRepository.ChangePasswordByEmailAsync(
                reset.Email,
                PasswordHelper.HashPassword(password),
                cancellationToken);
            await _passwordResetRepository.RemoveCodeAsync(code, cancellationToken);
        }
    }
}
