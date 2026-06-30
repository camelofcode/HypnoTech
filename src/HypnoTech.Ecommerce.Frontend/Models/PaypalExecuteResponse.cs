using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Ecommerce.Frontend.Models
{
    public class PaypalExecuteResponse
    {
        public string Status { get; set; }

        public string PaymentId { get; set; }

        public string PayerId { get; set; }

        public string SaleId { get; set; }

        public string SaleUrl { get; set; }

        public string EmailAddress { get; set; }

        public Order Order { get; set; }

        public PaymentCompletion ToPaymentCompletion()
        {
            return new PaymentCompletion
            {
                PayerId = PayerId,
                PaymentId = PaymentId,
                SaleId = SaleId,
                SaleUrl = SaleUrl,
                EmailAddress = EmailAddress
            };
        }
    }
}
