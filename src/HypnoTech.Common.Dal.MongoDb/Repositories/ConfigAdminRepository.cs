using System.Linq;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class ConfigAdminRepository : BaseMongoRepository, IConfigAdminRepository
    {
        public ConfigAdminRepository(IOptions<MongoOptions> config) : base(config, "config")
        {
        }

        public Config GetConfig()
        {
            return GetAll<Config>().FirstOrDefault();
        }

        public void SetConfig(Config config)
        {
            Collection.ReplaceOne(new BsonDocument(), config.ToBsonDocument());
        }
    }
}