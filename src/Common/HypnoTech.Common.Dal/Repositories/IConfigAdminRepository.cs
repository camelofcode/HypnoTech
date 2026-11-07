using HypnoTech.Common.Models.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IConfigAdminRepository
    {
        Task<Config> GetConfigAsync(CancellationToken cancellationToken = default);
        Task SetConfigAsync(Config config, CancellationToken cancellationToken = default);
    }
}
