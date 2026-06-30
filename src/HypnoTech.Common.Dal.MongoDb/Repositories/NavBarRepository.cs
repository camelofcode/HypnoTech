using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class NavBarRepository : BaseMongoRepository, INavBarRepository
    {
        public NavBarRepository(IOptions<MongoOptions> config) : base(config, "navbar")
        {
        }

        public List<NavBarLink> GetEnabledNavBarLinks()
        {
            return GetAll<NavBarLink>().Where(p => p.Enabled).OrderBy(p => p.Order).ToList();
        }
    }
}
