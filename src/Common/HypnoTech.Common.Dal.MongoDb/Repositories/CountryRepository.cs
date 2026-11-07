using System.Collections.Generic;
using System.Linq;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class CountryRepository : BaseMongoRepository, ICountryRepository
    {
        public CountryRepository(IOptions<MongoOptions> config) : base(config, "countries")
        {
        }

        public async Task<List<Country>> GetCountriesAsync(CancellationToken cancellationToken = default)
        {
            return (await GetAllAsync<Country>("displayText", cancellationToken: cancellationToken)).ToList();
        }

        public Task<Country> GetCountryByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return GetOneAsync<Country>(new BsonDocument("code", code), cancellationToken);
        }
    }
}
