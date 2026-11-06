using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Dtos.Responses;
using HypnoTech.Ecommerce.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Dal.Repositories
{
    public interface IOrderRepository
    {
        void SaveOrder(Order order);
        Task SaveOrderAsync(Order order, CancellationToken cancellationToken = default);

        Order CompleteOrder(PaymentCompletion completion);
        Task<Order> CompleteOrderAsync(PaymentCompletion completion, CancellationToken cancellationToken = default);

        void CancelOrder(string paymentId);
        Task CancelOrderAsync(string paymentId, CancellationToken cancellationToken = default);

        PagedListResponse<Order> GetOrders(PagedRequest request);
        Task<PagedListResponse<Order>> GetOrdersAsync(PagedRequest request, CancellationToken cancellationToken = default);

        Order GetOrder(string paymentId);
        Task<Order> GetOrderAsync(string paymentId, CancellationToken cancellationToken = default);

        Order MarkAsShipped(string paymentId);
        Task<Order> MarkAsShippedAsync(string paymentId, CancellationToken cancellationToken = default);
    }
}
