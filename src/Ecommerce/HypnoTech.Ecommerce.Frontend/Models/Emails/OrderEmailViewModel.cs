using System;
using HypnoTech.Common.Emails;
using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Ecommerce.Frontend.Models.Emails
{
    public class OrderEmailViewModel : BaseEmailViewModel
    {
        public Order Order { get; }


        public OrderEmailViewModel(Order order)
        {
            Order = order;
        }

        public override void AfterApply()
        {
            //Nothing to do
        }
    }
}
