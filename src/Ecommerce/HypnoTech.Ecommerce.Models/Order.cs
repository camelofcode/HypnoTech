using System;
using System.Collections.Generic;
using System.Globalization;

namespace HypnoTech.Ecommerce.Models
{
    public class Order
    {
        public DateTime DateTime { get; set; }

        public string PaymentId { get; set; }

        public string SaleId { get; set; }

        public string SaleUrl { get; set; }

        public string PayerId { get; set; }

        public string Status { get; set; }

        public decimal SpecialDeliveryCost => Tracked ? 5.0m : 0.0m;

        public bool Tracked { get; set; }

        public decimal Postage { get; set; }

        public string PostageDisplay => Postage.ToString("C", new CultureInfo("en-GB"));

        public decimal Total { get; set; }

        public string TotalDisplay => Total.ToString("C", new CultureInfo("en-GB"));

        public decimal SubTotal { get; set; }

        public string SubTotalDisplay => SubTotal.ToString("C", new CultureInfo("en-GB"));

        public List<OrderItem> Items { get; set; }

        public PostageAddress Address { get; set; }

        public string EmailAddress { get; set; }

        public List<string> DownloadCodeIds { get; set; }
    }
}

