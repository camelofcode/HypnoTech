using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Services.Interfaces
{
    public interface IForgottenPasswordService
    {
        bool CreatePasswordResetIfUserExists(string nameOrEmail);
        Task<bool> CreatePasswordResetIfUserExistsAsync(string nameOrEmail, CancellationToken cancellationToken = default);
        bool CheckCode(string code);
        Task<bool> CheckCodeAsync(string code, CancellationToken cancellationToken = default);
        void ResetPassword(string code, string password);
        Task ResetPasswordAsync(string code, string password, CancellationToken cancellationToken = default);
    }
}
