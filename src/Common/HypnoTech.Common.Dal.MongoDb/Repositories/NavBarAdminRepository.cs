using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class NavBarAdminRepository : BaseMongoRepository, INavBarAdminRepository
    {
        public NavBarAdminRepository(IOptions<MongoOptions> config) : base(config, "navbar")
        {
        }

        public async Task<List<NavBarLink>> GetNavBarLinksAsync(CancellationToken cancellationToken = default)
        {
            return (await GetAllAsync<NavBarLink>(cancellationToken: cancellationToken))
                .OrderBy(p => p.Order)
                .ToList();
        }

        public async Task UpdateNavBarLinksAsync(List<NavBarLink> links, CancellationToken cancellationToken = default)
        {
            await Collection.DeleteManyAsync(new BsonDocument(), cancellationToken);
            await Collection.InsertManyAsync(
                links.Select(p => p.ToBsonDocument()),
                cancellationToken: cancellationToken);
        }
    }
}
