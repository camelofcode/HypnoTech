using System.Collections.Generic;
using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Ecommerce.Dal.Repositories
{
    public interface IMerchRepository
    {
        List<MerchCategory> GetMerch();
        List<MerchItem> GetMerchFlattened();
        MerchItem GetMerchItem(string slug);
    }
}