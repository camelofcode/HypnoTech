using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<List<NavBarLink>> GetEnabledNavBarLinksAsync(
            CancellationToken cancellationToken = default)
        {
            return (await GetAllAsync<NavBarLink>(cancellationToken: cancellationToken))
                .Where(p => p.Enabled)
                .OrderBy(p => p.Order)
                .ToList();
        }
    }
}
