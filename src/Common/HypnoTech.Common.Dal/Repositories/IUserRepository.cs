using System.Collections.Generic;
using HypnoTech.Common.Models.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IUserRepository
    {
        Task<AdminUser> GetUserForLoginAsync(string username, CancellationToken cancellationToken = default);
        Task<List<string>> GetUserRolesAsync(string username, CancellationToken cancellationToken = default);
        Task<List<AdminUser>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<AdminUser> GetUserForEditAsync(string id, CancellationToken cancellationToken = default);
        Task<string> AddUserAsync(AdminUser user, CancellationToken cancellationToken = default);
        Task<string> UpdateUserAsync(AdminUser user, CancellationToken cancellationToken = default);
        Task<AdminUser> ChangePasswordAsync(string id, string password, CancellationToken cancellationToken = default);
        Task<AdminUser> ChangePasswordByEmailAsync(string email, string password, CancellationToken cancellationToken = default);
        Task RemoveUserAsync(string id, CancellationToken cancellationToken = default);
        Task<AdminUser> GetUserByUsernameOrEmailAsync(string nameOrEmail, CancellationToken cancellationToken = default);
    }
}
