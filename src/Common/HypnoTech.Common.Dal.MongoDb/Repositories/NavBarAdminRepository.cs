using System.Collections.Generic;
using System.Linq;
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

        public List<NavBarLink> GetNavBarLinks()
        {
            return GetAll<NavBarLink>().OrderBy(p => p.Order).ToList();
        }

        public void UpdateNavBarLinks(List<NavBarLink> links)
        {
            Collection.DeleteMany(new BsonDocument());
            Collection.InsertMany(links.Select(p => p.ToBsonDocument()));
        }
    }
}
