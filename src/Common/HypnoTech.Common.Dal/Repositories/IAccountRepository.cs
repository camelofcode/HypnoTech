using HypnoTech.Common.Models.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IAccountRepository
    {
        AdminUser GetUserByName(string name);
        Task<AdminUser> GetUserByNameAsync(string name, CancellationToken cancellationToken = default);

        string UpdateAccount(AdminUser user);
        Task<string> UpdateAccountAsync(AdminUser user, CancellationToken cancellationToken = default);

        void ChangePassword(string id, string password);
        Task ChangePasswordAsync(string id, string password, CancellationToken cancellationToken = default);
    }
}
