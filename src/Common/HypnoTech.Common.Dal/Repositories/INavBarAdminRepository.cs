using System.Collections.Generic;
using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface INavBarAdminRepository
    {
        List<NavBarLink> GetNavBarLinks();

        void UpdateNavBarLinks(List<NavBarLink> links);
    }
}
