using System.Collections.Generic;
using HypnoTech.Ecommerce.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Dal.Repositories
{
    public interface IMerchRepository
    {
        Task<List<MerchCategory>> GetMerchAsync(CancellationToken cancellationToken = default);
        Task<List<MerchItem>> GetMerchFlattenedAsync(CancellationToken cancellationToken = default);
        Task<MerchItem> GetMerchItemAsync(string slug, CancellationToken cancellationToken = default);
    }
}
