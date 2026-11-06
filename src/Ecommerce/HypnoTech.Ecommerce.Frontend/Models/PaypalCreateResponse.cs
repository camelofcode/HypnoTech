using HypnoTech.Ecommerce.Models;
using Newtonsoft.Json;

namespace HypnoTech.Ecommerce.Frontend.Models
{
    public class PaypalCreateResponse
    {
        public string Status { get; set; }

        public string RedirectUrl { get; set; }

        public string ValidationMessage { get; set; }

        [JsonIgnore]
        public Order Order { get; set; }

        public static PaypalCreateResponse Success(Order order, string redirectUrl)
        {
            return new PaypalCreateResponse
            {
                Status = "SUCCESS",
                Order = order,
                RedirectUrl = redirectUrl,             
            };
        }

        public static PaypalCreateResponse Invalid(string validationMessage)
        {
            return new PaypalCreateResponse
            {
                Status = "INVALID",
                ValidationMessage = validationMessage
            };
        }
    }
}
