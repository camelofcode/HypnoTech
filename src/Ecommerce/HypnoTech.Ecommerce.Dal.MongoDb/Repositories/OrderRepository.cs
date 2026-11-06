using System;
using System.Linq;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Dtos.Responses;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Dal.MongoDb.Repositories
{
    public class OrderRepository : BaseMongoRepository, IOrderRepository
    {
        private readonly ICountryRepository _countryRepository;

        public OrderRepository(IOptions<MongoOptions> config, ICountryRepository countryRepository) : base(config, "orders")
        {
            _countryRepository = countryRepository;
        }

        public void SaveOrder(Order order)
        {
            order.Address.Country = _countryRepository.GetCountryByCode(order.Address.CountryCode).DisplayText;

            Collection.InsertOne(order.ToBsonDocument());
        }

        public async Task SaveOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            order.Address.Country = (await _countryRepository.GetCountryByCodeAsync(
                order.Address.CountryCode,
                cancellationToken)).DisplayText;
            await Collection.InsertOneAsync(
                order.ToBsonDocument(),
                cancellationToken: cancellationToken);
        }

        public Order CompleteOrder(PaymentCompletion completion)
        {
            var order = GetOne<Order>(new BsonDocument("paymentId", completion.PaymentId));

            order.PayerId = completion.PayerId;
            order.SaleId = completion.SaleId;
            order.SaleUrl = completion.SaleUrl;
            order.EmailAddress = completion.EmailAddress;
            order.Status = "COMPLETE";

            Collection.ReplaceOne(new BsonDocument("paymentId", completion.PaymentId), order.ToBsonDocument());

            return order;
        }

        public async Task<Order> CompleteOrderAsync(PaymentCompletion completion, CancellationToken cancellationToken = default)
        {
            var order = await GetOneAsync<Order>(
                new BsonDocument("paymentId", completion.PaymentId),
                cancellationToken);

            order.PayerId = completion.PayerId;
            order.SaleId = completion.SaleId;
            order.SaleUrl = completion.SaleUrl;
            order.EmailAddress = completion.EmailAddress;
            order.Status = "COMPLETE";

            await Collection.ReplaceOneAsync(
                new BsonDocument("paymentId", completion.PaymentId),
                order.ToBsonDocument(),
                cancellationToken: cancellationToken);
            return order;
        }

        public void CancelOrder(string paymentId)
        {
            var order = GetOne<Order>(new BsonDocument("paymentId", paymentId));

            order.Status = "CANCELLED";

            Collection.ReplaceOne(new BsonDocument("paymentId", paymentId), order.ToBsonDocument());
        }

        public async Task CancelOrderAsync(string paymentId, CancellationToken cancellationToken = default)
        {
            var order = await GetOneAsync<Order>(
                new BsonDocument("paymentId", paymentId),
                cancellationToken);
            order.Status = "CANCELLED";
            await Collection.ReplaceOneAsync(
                new BsonDocument("paymentId", paymentId),
                order.ToBsonDocument(),
                cancellationToken: cancellationToken);
        }

        public PagedListResponse<Order> GetOrders(PagedRequest request)
        {
            var search = new BsonDocument("status", "COMPLETE");
            var collection = Order(Collection.Find(search), "createdDate", true)
                .Skip(request.Skip)
                .Limit(request.PageSize);

            var items = collection.ToList().Select(p => BsonSerializer.Deserialize<Order>(p)).ToList();

            return new PagedListResponse<Order>
            {
                CurrentPage = request.Page,
                Items = items,
                TotalPages = (int)Math.Ceiling((Collection.CountDocuments(search) / (decimal)request.PageSize))
            };
        }

        public async Task<PagedListResponse<Order>> GetOrdersAsync(
            PagedRequest request,
            CancellationToken cancellationToken = default)
        {
            var search = new BsonDocument("status", "COMPLETE");
            var collection = Order(Collection.Find(search), "createdDate", true)
                .Skip(request.Skip)
                .Limit(request.PageSize);
            var documents = await collection.ToListAsync(cancellationToken);
            var count = await Collection.CountDocumentsAsync(
                search,
                cancellationToken: cancellationToken);

            return new PagedListResponse<Order>
            {
                CurrentPage = request.Page,
                Items = documents.Select(p => BsonSerializer.Deserialize<Order>(p)).ToList(),
                TotalPages = (int)Math.Ceiling(count / (decimal)request.PageSize)
            };
        }

        public Order GetOrder(string paymentId)
        {
            return GetOne<Order>(new BsonDocument("paymentId", paymentId));
        }

        public Task<Order> GetOrderAsync(string paymentId, CancellationToken cancellationToken = default) =>
            GetOneAsync<Order>(new BsonDocument("paymentId", paymentId), cancellationToken);

        public Order MarkAsShipped(string paymentId)
        {
            var order = GetOne<Order>(new BsonDocument("paymentId", paymentId));

            order.Status = "SHIPPED";

            Collection.ReplaceOne(new BsonDocument("paymentId", paymentId), order.ToBsonDocument());

            return order;
        }

        public async Task<Order> MarkAsShippedAsync(string paymentId, CancellationToken cancellationToken = default)
        {
            var order = await GetOneAsync<Order>(
                new BsonDocument("paymentId", paymentId),
                cancellationToken);
            order.Status = "SHIPPED";
            await Collection.ReplaceOneAsync(
                new BsonDocument("paymentId", paymentId),
                order.ToBsonDocument(),
                cancellationToken: cancellationToken);
            return order;
        }
    }
}
