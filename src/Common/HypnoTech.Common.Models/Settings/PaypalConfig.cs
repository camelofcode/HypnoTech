namespace HypnoTech.Common.Models.Settings
{
    public class PaypalConfig
    {
        public string ApiClientId { get; set; }

        public string ApiClientSecret { get; set; }

        public string ApiUrl { get; set; }

        public string SalesUrl { get; set; }

        public string PaymentCompleteUrl { get; set; }

        public string PaymentCancelledUrl { get; set; }

        public bool IsProduction { get; set; }
    }
}
