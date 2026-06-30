using System.Linq;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Frontend.Services.Interfaces;
using HypnoTech.Ecommerce.Models.Basket;

namespace HypnoTech.Ecommerce.Frontend.Services
{
    public class BasketService : IBasketService
    {
        private readonly IMerchRepository _merchRepository;

        public BasketService(IMerchRepository merchRepository)
        {
            _merchRepository = merchRepository;
        }

        public BasketDetails GetDetails(BasketSummary summary)
        {
            var merchItems = _merchRepository.GetMerchFlattened();

            BasketDetails details = new BasketDetails();

            foreach (var summaryItem in summary.Items)
            {
                var merchItem = merchItems.FirstOrDefault(p => p.Id == summaryItem.MerchId);
                if (merchItem != null)
                {
                    details.AddItem(summaryItem, merchItem);
                }
            }

            return details;
        }
    }
}
