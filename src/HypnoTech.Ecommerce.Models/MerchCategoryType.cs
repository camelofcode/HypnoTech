using System.Collections.Generic;

namespace HypnoTech.Ecommerce.Models
{
    public class MerchCategoryType
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        public List<MerchSubcategoryType> SubcategoryTypes { get; set; }
    }
}
