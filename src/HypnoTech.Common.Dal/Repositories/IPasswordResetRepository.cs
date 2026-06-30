using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IPasswordResetRepository
    {
        string CreateResetPasswordCode(AdminUser user);
        PasswordReset GetCode(string code);
        void RemoveCode(string code);
    }
}
