using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Services.Interfaces
{
    public interface IApiService
    {
        Task ClearSiteNavbarCacheAsync(CancellationToken cancellationToken = default);
    }
}
