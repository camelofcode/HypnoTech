using HypnoTech.Common.Emails;
using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Ecommerce.Areas.Settings.Models
{
    class OrderShippedEmailViewModel : BaseEmailViewModel
    {
        public Order Order { get; }

        public OrderShippedEmailViewModel(Order order)
        {
            Order = order;
        }

        public override void AfterApply()
        {
            IntroLine = IntroLine.Replace("#{Name}", Order.Address.Name);
        }
    }
}
