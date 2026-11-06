using HypnoTech.Common.Models.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IConfigAdminRepository
    {
        Config GetConfig();
        Task<Config> GetConfigAsync(CancellationToken cancellationToken = default);

        void SetConfig(Config config);
        Task SetConfigAsync(Config config, CancellationToken cancellationToken = default);
    }
}
