using HypnoTech.Ecommerce.Frontend.Models;
using HypnoTech.Ecommerce.Models;
using HypnoTech.Ecommerce.Models.Basket;

namespace HypnoTech.Ecommerce.Frontend.Services.Interfaces
{
    public interface ICheckoutService
    {
        CheckoutViewModel GetCheckout();
        CheckoutDetails GetCheckoutDetails(CheckoutSummary summary);
        PaypalCreateResponse Pay(CheckoutSummary summary, PostageAddress address);
        PaypalExecuteResponse Complete(string paymentId, string payerId);
        void Cancel(string paymentId);
    }
}
