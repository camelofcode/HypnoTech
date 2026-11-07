using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface INavBarRepository
    {
        Task<List<NavBarLink>> GetEnabledNavBarLinksAsync(CancellationToken cancellationToken = default);
    }
}
