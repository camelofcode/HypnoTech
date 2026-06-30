using System.Collections.Generic;
using System.Linq;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class CountryRepository : BaseMongoRepository, ICountryRepository
    {
        public CountryRepository(IOptions<MongoOptions> config) : base(config, "countries")
        {
        }

        public List<Country> GetCountries()
        {
            return GetAll<Country>("displayText").ToList();
        }

        public Country GetCountryByCode(string code)
        {
            return GetOne<Country>(new BsonDocument("code", code));
        }
    }
}
