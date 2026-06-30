using System.Globalization;

namespace HypnoTech.Ecommerce.Models
{
    public class OrderItem
    {
        public string Item { get; set; }

        public int Quantity { get; set; }

        public decimal RowPrice { get; set; }

        public string RowPriceDisplay => RowPrice.ToString("C", new CultureInfo("en-GB"));
    }
}
