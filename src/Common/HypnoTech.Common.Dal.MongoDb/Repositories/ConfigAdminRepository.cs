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
    public class ConfigAdminRepository : BaseMongoRepository, IConfigAdminRepository
    {
        public ConfigAdminRepository(IOptions<MongoOptions> config) : base(config, "config")
        {
        }

        public async Task<Config> GetConfigAsync(CancellationToken cancellationToken = default)
        {
            return (await GetAllAsync<Config>(cancellationToken: cancellationToken)).FirstOrDefault();
        }

        public async Task SetConfigAsync(Config config, CancellationToken cancellationToken = default)
        {
            await Collection.ReplaceOneAsync(
                new BsonDocument(),
                config.ToBsonDocument(),
                cancellationToken: cancellationToken);
        }
    }
}
