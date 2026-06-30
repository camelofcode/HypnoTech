using HypnoTech.Ecommerce.Models.Basket;

namespace HypnoTech.Ecommerce.Frontend.Services.Interfaces
{
    public interface IBasketService
    {
        BasketDetails GetDetails(BasketSummary summary);
    }
}
