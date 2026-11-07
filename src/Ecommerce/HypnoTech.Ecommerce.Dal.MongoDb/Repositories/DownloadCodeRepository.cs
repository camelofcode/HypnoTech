using System.Collections.Generic;
using System.Linq;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Dal.MongoDb.Repositories
{
    public class DownloadCodeRepository : BaseMongoRepository, IDownloadCodeRepository
    {
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

        public DownloadCodeRepository(IOptions<MongoOptions> config) : base(config, "downloadcodes")
        {
        }

        public async Task<AssignDownloadCodesResponse> AssignDownloadCodesAsync(
            Order order,
            CancellationToken cancellationToken = default)
        {
            await _asyncLock.WaitAsync(cancellationToken);
            try
            {
                var assigned = new List<AssignedDownloadCode>();
                var expired = new List<string>();

                foreach (var codeId in order.DownloadCodeIds.Distinct())
                {
                    var codeCollection = await GetByObjectIdAsync<DownloadCodeCollection>(
                        codeId,
                        cancellationToken);
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

                        await Collection.ReplaceOneAsync(
                            new BsonDocument("_id", new ObjectId(codeCollection.Id)),
                            codeCollection.ToBsonDocument(),
                            cancellationToken: cancellationToken);

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
            finally
            {
                _asyncLock.Release();
            }
        }

        public async Task<List<DownloadCodeCollection>> GetAllCodeCollectionsAsync(CancellationToken cancellationToken = default) =>
            (await GetAllAsync<DownloadCodeCollection>(cancellationToken: cancellationToken)).ToList();

        public async Task AddCodeCollectionAsync(DownloadCodeCollection codes, CancellationToken cancellationToken = default)
        {
            codes.Id = ObjectId.GenerateNewId().ToString();
            await Collection.InsertOneAsync(
                codes.ToBsonDocument(),
                cancellationToken: cancellationToken);
        }

        public Task<DownloadCodeCollection> GetDownloadCodeCollectionAsync(string id, CancellationToken cancellationToken = default) =>
            GetByObjectIdAsync<DownloadCodeCollection>(id, cancellationToken);

        public async Task<string> AssignDownloadCodeAsync(
            string collectionId,
            string emailAddress,
            CancellationToken cancellationToken = default)
        {
            await _asyncLock.WaitAsync(cancellationToken);
            try
            {
                var codeCollection = await GetByObjectIdAsync<DownloadCodeCollection>(
                    collectionId,
                    cancellationToken);
                var code = codeCollection.Codes.FirstOrDefault(p => !p.Assigned);
                if (code == null)
                {
                    return string.Empty;
                }

                code.AssignedTo = emailAddress;
                await Collection.ReplaceOneAsync(
                    new BsonDocument("_id", new ObjectId(codeCollection.Id)),
                    codeCollection.ToBsonDocument(),
                    cancellationToken: cancellationToken);
                return code.Code;
            }
            finally
            {
                _asyncLock.Release();
            }
        }
    }
}
