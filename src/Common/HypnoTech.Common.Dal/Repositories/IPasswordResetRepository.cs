using HypnoTech.Common.Models.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IPasswordResetRepository
    {
        Task<string> CreateResetPasswordCodeAsync(AdminUser user, CancellationToken cancellationToken = default);
        Task<PasswordReset> GetCodeAsync(string code, CancellationToken cancellationToken = default);
        Task RemoveCodeAsync(string code, CancellationToken cancellationToken = default);
    }
}
