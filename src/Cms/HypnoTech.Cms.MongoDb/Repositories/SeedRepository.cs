using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Cms.FormMemberImplementations;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace HypnoTech.Cms.MongoDb.Repositories
{
    public class SeedRepository : ISeedRepository
    {
        private readonly MongoOptions _options;

        public SeedRepository(IOptions<MongoOptions> options)
        {
            _options = options.Value;
        }

        public async Task SeedAsync<T>(
            string collectionName,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            var dbClient = new MongoClient(_options.ConnectionString);
            var db = dbClient.GetDatabase(_options.Database);
            var collection = db.GetCollection<BsonDocument>(collectionName);

            if (await collection.CountDocumentsAsync(
                    new BsonDocument(),
                    cancellationToken: cancellationToken) != 0)
            {
                return;
            }

            var json = await File.ReadAllTextAsync(
                $@"SeedData/{fileName}.json",
                cancellationToken);
            ICollection<T> documentsToInsert = JsonConvert.DeserializeObject<ICollection<T>>(json);
            foreach (T document in documentsToInsert)
            {
                if (document.GetType().GetProperty("Id")?.GetValue(document) == null)
                {
                    document.GetType().GetProperty("Id")?.SetValue(document, ObjectId.GenerateNewId().ToString());
                }

                var mapping = FormClassMapping.GetMapping<T>();
                if (mapping != null)
                {
                    foreach (var member in mapping.MemberMappings.Where(p => p is GeneratedMemberMapping<T>))
                    {
                        (member as GeneratedMemberMapping<T>)?.Generate(document);
                        document.GetType().GetProperty(member.MemberName)?.SetValue(document, member.GetPropertyValue(document));
                    }
                }
                await collection.InsertOneAsync(
                    document.ToBsonDocument(),
                    cancellationToken: cancellationToken);
            }
        }
    }
}
