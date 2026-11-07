using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Models;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.MongoDb;

namespace HypnoTech.Cms.MongoDb.Repositories
{
    public class SelectListRepository : ISelectListRepository
    {
        private readonly IMongoDatabase _database;

        public SelectListRepository(IOptions<MongoOptions> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            _database = client.GetDatabase(options.Value.Database);
        }

        public Task<List<object>> GetSelectListItemsAsync(
            Type type,
            CancellationToken cancellationToken = default)
        {
            var collection = ModelRegistry.Registrations.FirstOrDefault(p => p.Type == type)?.DataName;

            MethodInfo method = typeof(SelectListRepository).GetMethod(nameof(GetAllAsync));
            MethodInfo generic = method?.MakeGenericMethod(type);
            var result = generic?.Invoke(null, new object[] { _database, collection, cancellationToken });
            return (Task<List<object>>)result;
        }

        public static async Task<List<object>> GetAllAsync<T>(
            IMongoDatabase database,
            string table,
            CancellationToken cancellationToken)
        {
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(table);
            var documents = await collection.Find(new BsonDocument()).ToListAsync(cancellationToken);
            var items = documents.Select(p => BsonSerializer.Deserialize<T>(p));

            return items.Cast<object>().ToList();
        }
    }
}
