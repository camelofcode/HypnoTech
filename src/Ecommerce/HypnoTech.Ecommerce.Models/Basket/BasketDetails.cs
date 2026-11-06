using System.Collections.Generic;
using System.Linq;

namespace HypnoTech.Ecommerce.Models.Basket
{
    public class BasketDetails
    {
        public List<BasketItemDetails> Items { get; set; }

        public decimal SubTotal => Items.Sum(p => p.RowPrice);

        public List<string> DownloadCodeIds => Items.Where(p => !string.IsNullOrEmpty(p.DownloadCodeId)).Select(p => p.DownloadCodeId).ToList();

        public BasketDetails()
        {
            Items = new List<BasketItemDetails>();
        }

        public void AddItem(BasketItemSummary summary, MerchItem merchItem)
        {
            BasketItemDetails item = Items.FirstOrDefault(p => p.Summary.MerchId == summary.MerchId && p.Summary?.OptionId == summary?.OptionId);

            if (item == null)
            {
                Items.Add(new BasketItemDetails(merchItem, summary));
            }
            else
            {
                item.Quantity += 1;
            }
        }
    }
}
