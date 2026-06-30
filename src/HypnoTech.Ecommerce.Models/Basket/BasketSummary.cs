using System.Collections.Generic;

namespace HypnoTech.Ecommerce.Models.Basket
{
    public class BasketSummary
    {
        public List<BasketItemSummary> Items { get; set; }

        public BasketSummary()
        {
            Items = new List<BasketItemSummary>();
        }
    }
}
