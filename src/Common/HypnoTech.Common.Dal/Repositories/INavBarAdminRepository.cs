using System.Collections.Generic;
using HypnoTech.Common.Models.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface INavBarAdminRepository
    {
        List<NavBarLink> GetNavBarLinks();
        Task<List<NavBarLink>> GetNavBarLinksAsync(CancellationToken cancellationToken = default);

        void UpdateNavBarLinks(List<NavBarLink> links);
        Task UpdateNavBarLinksAsync(List<NavBarLink> links, CancellationToken cancellationToken = default);
    }
}
