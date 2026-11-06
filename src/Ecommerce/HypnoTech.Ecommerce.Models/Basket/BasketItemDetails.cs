using System.Linq;

namespace HypnoTech.Ecommerce.Models.Basket
{
    public class BasketItemDetails
    {
        private readonly string _name;

        private readonly string _option;

        private readonly decimal _unitPrice;

        public string DisplayName => string.IsNullOrEmpty(_option) ? _name : $"{_name} ({_option})";

        public int Quantity { get; set; }

        public decimal RowPrice => _unitPrice * Quantity;

        public decimal UnitPrice => _unitPrice;

        public BasketItemSummary Summary { get; set; }

        public string DownloadCodeId { get; set; }

        public BasketItemDetails()
        {
            
        }

        public BasketItemDetails(MerchItem item, BasketItemSummary summary)
        {
            Summary = summary;
            _name = item.Name;
            Quantity = 1;
            _unitPrice = decimal.Parse(item.Price);
            DownloadCodeId = item.DownloadCodeId;

            if (item.Options != null && item.Options.Any() && !string.IsNullOrEmpty(summary.OptionId))
            {
                _option = item.Options.First(p => p.Id == summary.OptionId).Name;
            }
        }
    }
}