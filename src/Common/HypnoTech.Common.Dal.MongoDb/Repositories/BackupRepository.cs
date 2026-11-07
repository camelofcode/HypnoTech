using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class BackupRepository : IBackupRespository
    {
        private readonly IMongoDatabase _database;

        public BackupRepository(IOptions<MongoOptions> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            _database = client.GetDatabase(options.Value.Database);
        }

        public async Task<dynamic> GetAllDataAsync(
            List<string> includedTypes,
            CancellationToken cancellationToken = default)
        {
            var expando = new ExpandoObject();
            MethodInfo method = typeof(BackupRepository).GetMethod(nameof(GetAllAsObjectAsync));

            foreach (var type in ModelRegistry.Registrations)
            {
                if (method != null && includedTypes.Contains(type.DataName))
                {
                    MethodInfo generic = method.MakeGenericMethod(type.Type);
                    var task = (Task<object>)generic.Invoke(
                        null,
                        new object[] { _database, type.DataName, cancellationToken });
                    AddProperty(expando, type.DataName, await task);
                }
            }

            return expando;
        }

        public static async Task<object> GetAllAsObjectAsync<T>(
            IMongoDatabase database,
            string table,
            CancellationToken cancellationToken)
        {
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(table);
            var documents = await collection.Find(new BsonDocument()).ToListAsync(cancellationToken);
            return documents.Select(p => BsonSerializer.Deserialize<T>(p)).ToList();
        }

        private static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
            {
                expandoDict[propertyName] = propertyValue;
            }
            else
            {
                expandoDict.Add(propertyName, propertyValue);
            }
        }
    }



}
