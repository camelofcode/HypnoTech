namespace HypnoTech.Ecommerce.Models.Basket
{
    public class CheckoutSummary : BasketSummary
    {
        public bool SpecialDelivery { get; set; }

        public string CountryCode { get; set; }
    }
}
