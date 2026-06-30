using System;
using System.Collections.Generic;
using System.Text;
using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface INavBarRepository
    {
        List<NavBarLink> GetEnabledNavBarLinks();
    }
}
