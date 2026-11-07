using System.Collections.Generic;

namespace HypnoTech.Ecommerce.Models
{
    public class MerchCategory
    {
        public string Name { get; set; }

        public List<MerchSubCategory> SubCategories { get; set; }
    }
}
