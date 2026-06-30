using System.Collections.Generic;

namespace HypnoTech.Ecommerce.Models.Basket
{
    public class CheckoutDetails : BasketDetails
    {
        private readonly bool _specialDelivery;

        public decimal SpecialDeliveryCost => _specialDelivery ? 4.0m : 0.0m;

        public bool Tracked => _specialDelivery;

        public decimal Postage { get; set; }

        public decimal Total => SubTotal + SpecialDeliveryCost + Postage;

        private readonly List<string> _discountedTypes = new List<string>();

        public CheckoutDetails(bool specialDelivery)
        {
            _specialDelivery = specialDelivery;
        }

        public void AddItem(BasketItemSummary summary, MerchItem item, PostageProfile postage, string region)
        {
            base.AddItem(summary, item);

            var isDiscounted = _discountedTypes.Contains(item.SubCategory);

            switch (region)
            {
                case "UK":
                {
                    Postage += isDiscounted
                        ? postage.UnitedKingdom.WithOthers
                        : postage.UnitedKingdom.Alone;
                    break;
                }
                case "EU":
                {
                    Postage += isDiscounted
                        ? postage.EuropeanUnion.WithOthers
                        : postage.EuropeanUnion.Alone;
                    break;
                }
                case "ROW1":
                {
                    Postage += isDiscounted
                        ? postage.RestOfWorld1.WithOthers
                        : postage.RestOfWorld1.Alone;
                    break;
                }
                case "ROW2":
                {
                    Postage += isDiscounted
                        ? postage.RestOfWorld2.WithOthers
                        : postage.RestOfWorld2.Alone;
                    break;
                }
                case "ROW3":
                {
                    Postage += isDiscounted
                        ? postage.RestOfWorld3.WithOthers
                        : postage.RestOfWorld3.Alone;
                    break;
                }
            }

            _discountedTypes.Add(item.SubCategory);
        }
    }
}
