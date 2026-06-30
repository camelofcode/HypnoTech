using System.Collections.Generic;
using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IUserRepository
    {
        AdminUser GetUserForLogin(string username);
        List<string> GetUserRoles(string username);
        List<AdminUser> GetAllUsers();
        AdminUser GetUserForEdit(string id);
        string AddUser(AdminUser user);
        string UpdateUser(AdminUser user);
        AdminUser ChangePassword(string id, string password);
        AdminUser ChangePasswordByEmail(string email, string password);
        void RemoveUser(string id);
        AdminUser GetUserByUsernameOrEmail(string nameOrEmail);
    }
}
