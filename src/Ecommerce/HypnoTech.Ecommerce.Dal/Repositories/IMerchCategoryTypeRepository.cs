using System.Collections.Generic;
using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Ecommerce.Dal.Repositories
{
    public interface IMerchCategoryTypeRepository
    {
        List<MerchCategoryType> GetTypes();
    }
}
