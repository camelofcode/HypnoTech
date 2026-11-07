using System.Collections.Generic;
using HypnoTech.Common.Models.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface INavBarAdminRepository
    {
        Task<List<NavBarLink>> GetNavBarLinksAsync(CancellationToken cancellationToken = default);
        Task UpdateNavBarLinksAsync(List<NavBarLink> links, CancellationToken cancellationToken = default);
    }
}
