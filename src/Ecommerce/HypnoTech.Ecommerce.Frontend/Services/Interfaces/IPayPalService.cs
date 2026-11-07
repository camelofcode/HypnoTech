using HypnoTech.Ecommerce.Frontend.Models;
using HypnoTech.Ecommerce.Models;
using HypnoTech.Ecommerce.Models.Basket;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Frontend.Services.Interfaces
{
    public interface IPayPalService
    {
        Task<PaypalCreateResponse> CreatePaymentAsync(CheckoutDetails details, PostageAddress address, CancellationToken cancellationToken = default);
        Task<PaypalExecuteResponse> ExecutePaymentAsync(string paymentId, string payerId, CancellationToken cancellationToken = default);
    }
}
