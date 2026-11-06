using HypnoTech.Ecommerce.Frontend.Models;
using HypnoTech.Ecommerce.Models;
using HypnoTech.Ecommerce.Models.Basket;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Frontend.Services.Interfaces
{
    public interface ICheckoutService
    {
        CheckoutViewModel GetCheckout();
        Task<CheckoutViewModel> GetCheckoutAsync(CancellationToken cancellationToken = default);
        CheckoutDetails GetCheckoutDetails(CheckoutSummary summary);
        Task<CheckoutDetails> GetCheckoutDetailsAsync(CheckoutSummary summary, CancellationToken cancellationToken = default);
        PaypalCreateResponse Pay(CheckoutSummary summary, PostageAddress address);
        Task<PaypalCreateResponse> PayAsync(CheckoutSummary summary, PostageAddress address, CancellationToken cancellationToken = default);
        PaypalExecuteResponse Complete(string paymentId, string payerId);
        Task<PaypalExecuteResponse> CompleteAsync(string paymentId, string payerId, CancellationToken cancellationToken = default);
        void Cancel(string paymentId);
        Task CancelAsync(string paymentId, CancellationToken cancellationToken = default);
    }
}
