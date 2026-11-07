using HypnoTech.Common.Models.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IAccountRepository
    {
        Task<AdminUser> GetUserByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<string> UpdateAccountAsync(AdminUser user, CancellationToken cancellationToken = default);
        Task ChangePasswordAsync(string id, string password, CancellationToken cancellationToken = default);
    }
}
