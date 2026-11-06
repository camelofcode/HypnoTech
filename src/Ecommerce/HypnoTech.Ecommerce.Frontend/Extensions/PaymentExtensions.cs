using System;
using System.Collections.Generic;
using System.Linq;
using HypnoTech.Ecommerce.Models;
using PayPal.v1.Payments;
using Order = HypnoTech.Ecommerce.Models.Order;

namespace HypnoTech.Ecommerce.Frontend.Extensions
{
    public static class PaymentExtensions
    {
        public static Order ToOrder(this Payment payment, List<string> downloadCodeIds)
        {
            var transaction = payment.Transactions.First();

            return new Order
            {
                Status = "CREATED",
                PaymentId = payment.Id,
                DateTime = DateTime.Parse(payment.CreateTime),
                Tracked = transaction.Custom == "tracked",
                Total = decimal.Parse(transaction.Amount.Total),
                SubTotal = decimal.Parse(transaction.Amount.Details.Subtotal),
                Postage = decimal.Parse(transaction.Amount.Details.Shipping),
                Address = transaction.ItemList.ShippingAddress.ToAddress(),
                Items = transaction.ItemList.Items.Select(p => p.ToItem()).ToList(),
                DownloadCodeIds = downloadCodeIds              
            };
        }

        public static PostageAddress ToAddress(this ShippingAddress address)
        {
            return new PostageAddress
            {
                Name = address.RecipientName,
                Address1 = address.Line1,
                Address2 = address.Line2,
                CountryCode = address.CountryCode,
                PostalCode = address.PostalCode,
                Region = address.State,
                Town = address.City
            };
        }

        public static OrderItem ToItem(this Item item)
        {
            return new OrderItem
            {
                Item = item.Name,
                RowPrice = decimal.Parse(item.Price),
                Quantity = int.Parse(item.Quantity)
            };
        }
    }
}
