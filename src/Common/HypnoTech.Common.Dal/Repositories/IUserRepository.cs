using System.Collections.Generic;
using HypnoTech.Common.Models.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IUserRepository
    {
        AdminUser GetUserForLogin(string username);
        Task<AdminUser> GetUserForLoginAsync(string username, CancellationToken cancellationToken = default);
        List<string> GetUserRoles(string username);
        Task<List<string>> GetUserRolesAsync(string username, CancellationToken cancellationToken = default);
        List<AdminUser> GetAllUsers();
        Task<List<AdminUser>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        AdminUser GetUserForEdit(string id);
        Task<AdminUser> GetUserForEditAsync(string id, CancellationToken cancellationToken = default);
        string AddUser(AdminUser user);
        Task<string> AddUserAsync(AdminUser user, CancellationToken cancellationToken = default);
        string UpdateUser(AdminUser user);
        Task<string> UpdateUserAsync(AdminUser user, CancellationToken cancellationToken = default);
        AdminUser ChangePassword(string id, string password);
        Task<AdminUser> ChangePasswordAsync(string id, string password, CancellationToken cancellationToken = default);
        AdminUser ChangePasswordByEmail(string email, string password);
        Task<AdminUser> ChangePasswordByEmailAsync(string email, string password, CancellationToken cancellationToken = default);
        void RemoveUser(string id);
        Task RemoveUserAsync(string id, CancellationToken cancellationToken = default);
        AdminUser GetUserByUsernameOrEmail(string nameOrEmail);
        Task<AdminUser> GetUserByUsernameOrEmailAsync(string nameOrEmail, CancellationToken cancellationToken = default);
    }
}
