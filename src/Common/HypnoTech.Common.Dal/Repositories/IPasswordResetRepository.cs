using HypnoTech.Common.Models.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IPasswordResetRepository
    {
        string CreateResetPasswordCode(AdminUser user);
        Task<string> CreateResetPasswordCodeAsync(AdminUser user, CancellationToken cancellationToken = default);
        PasswordReset GetCode(string code);
        Task<PasswordReset> GetCodeAsync(string code, CancellationToken cancellationToken = default);
        void RemoveCode(string code);
        Task RemoveCodeAsync(string code, CancellationToken cancellationToken = default);
    }
}
