using HypnoTech.Ecommerce.Frontend.Models;
using HypnoTech.Ecommerce.Models;
using HypnoTech.Ecommerce.Models.Basket;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Frontend.Services.Interfaces
{
    public interface ICheckoutService
    {
        Task<CheckoutViewModel> GetCheckoutAsync(CancellationToken cancellationToken = default);
        Task<CheckoutDetails> GetCheckoutDetailsAsync(CheckoutSummary summary, CancellationToken cancellationToken = default);
        Task<PaypalCreateResponse> PayAsync(CheckoutSummary summary, PostageAddress address, CancellationToken cancellationToken = default);
        Task<PaypalExecuteResponse> CompleteAsync(string paymentId, string payerId, CancellationToken cancellationToken = default);
        Task CancelAsync(string paymentId, CancellationToken cancellationToken = default);
    }
}
