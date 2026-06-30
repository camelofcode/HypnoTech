using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Dtos.Responses;
using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Ecommerce.Dal.Repositories
{
    public interface IOrderRepository
    {
        void SaveOrder(Order order);

        Order CompleteOrder(PaymentCompletion completion);

        void CancelOrder(string paymentId);

        PagedListResponse<Order> GetOrders(PagedRequest request);

        Order GetOrder(string paymentId);

        Order MarkAsShipped(string paymentId);
    }
}
