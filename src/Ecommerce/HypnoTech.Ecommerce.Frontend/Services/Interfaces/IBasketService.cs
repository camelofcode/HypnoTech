using HypnoTech.Ecommerce.Models.Basket;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Frontend.Services.Interfaces
{
    public interface IBasketService
    {
        Task<BasketDetails> GetDetailsAsync(BasketSummary summary, CancellationToken cancellationToken = default);
    }
}
