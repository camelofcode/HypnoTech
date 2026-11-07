using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Dtos.Responses;
using HypnoTech.Ecommerce.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Dal.Repositories
{
    public interface IOrderRepository
    {
        Task SaveOrderAsync(Order order, CancellationToken cancellationToken = default);
        Task<Order> CompleteOrderAsync(PaymentCompletion completion, CancellationToken cancellationToken = default);
        Task CancelOrderAsync(string paymentId, CancellationToken cancellationToken = default);
        Task<PagedListResponse<Order>> GetOrdersAsync(PagedRequest request, CancellationToken cancellationToken = default);
        Task<Order> GetOrderAsync(string paymentId, CancellationToken cancellationToken = default);
        Task<Order> MarkAsShippedAsync(string paymentId, CancellationToken cancellationToken = default);
    }
}
