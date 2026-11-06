using System.Collections.Generic;
using HypnoTech.Ecommerce.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Dal.Repositories
{
    public interface IMerchRepository
    {
        List<MerchCategory> GetMerch();
        Task<List<MerchCategory>> GetMerchAsync(CancellationToken cancellationToken = default);
        List<MerchItem> GetMerchFlattened();
        Task<List<MerchItem>> GetMerchFlattenedAsync(CancellationToken cancellationToken = default);
        MerchItem GetMerchItem(string slug);
        Task<MerchItem> GetMerchItemAsync(string slug, CancellationToken cancellationToken = default);
    }
}
