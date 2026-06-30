using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public void Seed<T>(string collectionName, string fileName)
        {
            var dbClient = new MongoClient(_options.ConnectionString);
            var db = dbClient.GetDatabase(_options.Database);

            var collection = db.GetCollection<BsonDocument>(collectionName);
            if (collection == null)
            {
                db.CreateCollection(collectionName);
                collection = db.GetCollection<BsonDocument>(collectionName);
            }

            if (!collection.Find(new BsonDocument()).Any())
            {
                ICollection<T> documentsToInsert = JsonConvert.DeserializeObject<ICollection<T>>(File.ReadAllText($@"SeedData/{fileName}.json"));
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
                    collection.InsertOne(document.ToBsonDocument());
                }
            }
        }
    }
}
