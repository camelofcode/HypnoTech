using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public List<object> GetSelectListItems(Type type)
        {
            var collection = ModelRegistry.Registrations.FirstOrDefault(p => p.Type == type)?.DataName;

            MethodInfo method = typeof(SelectListRepository).GetMethod("GetAll");
            MethodInfo generic = method?.MakeGenericMethod(type);
            var result = generic?.Invoke(null, new object[] { _database, collection });
            return (List<object>)result;
        }

        public static List<object> GetAll<T>(IMongoDatabase database, string table)
        {
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(table);
            var items = collection.Find(new BsonDocument());
            var result = items.ToList().Select(p => BsonSerializer.Deserialize<T>(p)).ToList();

            List<object> objects = new List<object>();
            foreach (var item in result)
            {
                objects.Add(item);
            }

            return objects;
        }
    }
}
