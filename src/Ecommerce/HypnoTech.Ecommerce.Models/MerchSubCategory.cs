using System.Collections.Generic;

namespace HypnoTech.Ecommerce.Models
{
    public class MerchSubCategory
    {
        public string Name { get; set; }

        public List<MerchItem> Items { get; set; }
    }
}