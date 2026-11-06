using HypnoTech.Ecommerce.Frontend.Models;
using HypnoTech.Ecommerce.Models;
using HypnoTech.Ecommerce.Models.Basket;

namespace HypnoTech.Ecommerce.Frontend.Services.Interfaces
{
    public interface IPayPalService
    {
        PaypalCreateResponse CreatePayment(CheckoutDetails details, PostageAddress address);
        PaypalExecuteResponse ExecutePayment(string paymentId, string payerId);
    }
}
