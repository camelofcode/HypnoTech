using System.Collections.Generic;
using HypnoTech.Common.Models;
using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Ecommerce.Areas.Settings.Models
{
    public class OrdersViewModel
    {
        public List<Order> Orders { get; set; }

        public PagerViewModel Pager { get; set; }
    }
}
