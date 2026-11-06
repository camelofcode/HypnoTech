using System.Collections.Generic;
using System.Linq;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace HypnoTech.Ecommerce.Dal.MongoDb.Repositories
{
    public class DownloadCodeRepository : BaseMongoRepository, IDownloadCodeRepository
    {
        private readonly object _lock = new object();

        public DownloadCodeRepository(IOptions<MongoOptions> config) : base(config, "downloadcodes")
        {
        }

        public AssignDownloadCodesResponse AssignDownloadCodes(Order order)
        {
            lock (_lock)
            {
                var assigned = new List<AssignedDownloadCode>();
                var expired = new List<string>();

                foreach (var codeId in order.DownloadCodeIds.Distinct())
                {
                    var codeCollection = GetByObjectId<DownloadCodeCollection>(codeId);

                    var code = codeCollection.Codes.FirstOrDefault(p => !p.Assigned);
                    if (code != null)
                    {
                        code.AssignedTo = order.EmailAddress;
                        assigned.Add(new AssignedDownloadCode
                        {
                            Code = code.Code,
                            Name = codeCollection.Name,
                            RedemptionUrl = codeCollection.RedemptionUrl
                        });

                        Collection.ReplaceOne(new BsonDocument("_id", new ObjectId(codeCollection.Id)), codeCollection.ToBsonDocument());

                        if (codeCollection.UnusedCodes == 0)
                        {
                            expired.Add(codeCollection.Name);
                        }
                    }
                }

                return new AssignDownloadCodesResponse
                {
                    AssignedDownloadCodes = assigned,
                    ExpiredCollections = expired                
                };
            }
        }

        public List<DownloadCodeCollection> GetAllCodeCollections()
        {
            return GetAll<DownloadCodeCollection>().ToList();
        }

        public void AddCodeCollection(DownloadCodeCollection codes)
        {
            codes.Id = ObjectId.GenerateNewId().ToString();
            Collection.InsertOne(codes.ToBsonDocument());
        }

        public DownloadCodeCollection GetDownloadCodeCollection(string id)
        {
            return GetByObjectId<DownloadCodeCollection>(id);
        }

        public string AssignDownloadCode(string collectionId, string emailAddress)
        {
            var codeCollection = GetByObjectId<DownloadCodeCollection>(collectionId);

            var code = codeCollection.Codes.FirstOrDefault(p => !p.Assigned);
            if (code != null)
            {
                code.AssignedTo = emailAddress;
                Collection.ReplaceOne(new BsonDocument("_id", new ObjectId(codeCollection.Id)), codeCollection.ToBsonDocument());
                return code.Code;
            }

            return string.Empty;
        }
    }
}
