using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Dtos.Responses;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace HypnoTech.MongoDb
{
    public abstract class BaseMongoRepository
    {
        protected MongoClient Client;
        protected IMongoDatabase Database;
        protected IMongoCollection<BsonDocument> Collection;

        protected BaseMongoRepository(IOptions<MongoOptions> config, string collection)
        {
            Client = new MongoClient(config.Value.ConnectionString);
            Database = Client.GetDatabase(config.Value.Database);
            Collection = Database.GetCollection<BsonDocument>(collection);
        }

        protected T GetByObjectId<T>(string objectId)
        {
            return GetOne<T>(new BsonDocument("_id", new BsonObjectId(new ObjectId(objectId))));
        }

        protected Task<T> GetByObjectIdAsync<T>(string objectId, CancellationToken cancellationToken = default)
        {
            return GetOneAsync<T>(
                new BsonDocument("_id", new BsonObjectId(new ObjectId(objectId))),
                cancellationToken);
        }

        protected T GetOne<T>()
        {
            return GetOne<T>(new BsonDocument());
        }

        protected T GetOne<T>(FilterDefinition<BsonDocument> filter)
        {
            var document = Collection.Find(filter).FirstOrDefault();
            if (document == null)
            {
                return default(T);
            }
            return BsonSerializer.Deserialize<T>(document);
        }

        protected async Task<T> GetOneAsync<T>(
            FilterDefinition<BsonDocument> filter,
            CancellationToken cancellationToken = default)
        {
            var document = await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            if (document == null)
            {
                return default(T);
            }
            return BsonSerializer.Deserialize<T>(document);
        }

        protected ICollection<T> GetAll<T>(string orderBy = null, bool descending = false)
        {
            var collection = Collection.Find(new BsonDocument());
            collection = Order(collection, orderBy, descending);
            return collection.ToList().Select(p => BsonSerializer.Deserialize<T>(p)).ToList();
        }

        protected async Task<ICollection<T>> GetAllAsync<T>(
            string orderBy = null,
            bool descending = false,
            CancellationToken cancellationToken = default)
        {
            var collection = Collection.Find(new BsonDocument());
            collection = Order(collection, orderBy, descending);
            var documents = await collection.ToListAsync(cancellationToken);
            return documents.Select(p => BsonSerializer.Deserialize<T>(p)).ToList();
        }

        protected PagedListResponse<T> GetPaged<T>(PagedRequest request, string orderBy = null, bool descending = false) where T: class
        {
            var collection = Collection.Find(new BsonDocument()).Skip(request.Skip).Limit(request.PageSize);
            collection = Order(collection, orderBy, descending);
            
            var items = collection.ToList().Select(p => BsonSerializer.Deserialize<T>(p)).ToList();

            return new PagedListResponse<T>
            {
                CurrentPage = request.Page,
                Items = items,
                TotalPages = (int)Math.Ceiling((Collection.CountDocuments(new BsonDocument()) / (decimal)request.PageSize))
            };
        }

        protected async Task<PagedListResponse<T>> GetPagedAsync<T>(
            PagedRequest request,
            string orderBy = null,
            bool descending = false,
            CancellationToken cancellationToken = default) where T : class
        {
            var collection = Collection.Find(new BsonDocument()).Skip(request.Skip).Limit(request.PageSize);
            collection = Order(collection, orderBy, descending);

            var documents = await collection.ToListAsync(cancellationToken);
            var count = await Collection.CountDocumentsAsync(
                new BsonDocument(),
                cancellationToken: cancellationToken);

            return new PagedListResponse<T>
            {
                CurrentPage = request.Page,
                Items = documents.Select(p => BsonSerializer.Deserialize<T>(p)).ToList(),
                TotalPages = (int)Math.Ceiling(count / (decimal)request.PageSize)
            };
        }

        protected IFindFluent<BsonDocument, BsonDocument> Order(IFindFluent<BsonDocument, BsonDocument> collection, string orderBy = null, bool descending = false)
        {
            if (!string.IsNullOrEmpty(orderBy))
            {
                return descending ? collection.SortByDescending(bson => bson[orderBy]) : collection.SortBy(bson => bson[orderBy]);
            }

            return collection;
        }
    }
}
