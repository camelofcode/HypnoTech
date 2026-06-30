using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IAccountRepository
    {
        AdminUser GetUserByName(string name);

        string UpdateAccount(AdminUser user);

        void ChangePassword(string id, string password);
    }
}
