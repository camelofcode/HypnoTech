using HypnoTech.Ecommerce.Frontend.Models;
using HypnoTech.Ecommerce.Models;
using HypnoTech.Ecommerce.Models.Basket;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Frontend.Services.Interfaces
{
    public interface IPayPalService
    {
        PaypalCreateResponse CreatePayment(CheckoutDetails details, PostageAddress address);
        Task<PaypalCreateResponse> CreatePaymentAsync(CheckoutDetails details, PostageAddress address, CancellationToken cancellationToken = default);
        PaypalExecuteResponse ExecutePayment(string paymentId, string payerId);
        Task<PaypalExecuteResponse> ExecutePaymentAsync(string paymentId, string payerId, CancellationToken cancellationToken = default);
    }
}
