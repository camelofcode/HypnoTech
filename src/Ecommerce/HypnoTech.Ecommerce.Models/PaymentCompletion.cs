namespace HypnoTech.Ecommerce.Models
{
    public class PaymentCompletion
    {
        public string PaymentId { get; set; }

        public string PayerId { get; set; }

        public string SaleId { get; set; }

        public string SaleUrl { get; set; }

        public string EmailAddress { get; set; }
    }
}
