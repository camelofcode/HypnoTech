using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Services.Interfaces
{
    public interface IForgottenPasswordService
    {
        Task<bool> CreatePasswordResetIfUserExistsAsync(string nameOrEmail, CancellationToken cancellationToken = default);
        Task<bool> CheckCodeAsync(string code, CancellationToken cancellationToken = default);
        Task ResetPasswordAsync(string code, string password, CancellationToken cancellationToken = default);
    }
}
