using System.Collections.Generic;
using HypnoTech.Ecommerce.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Dal.Repositories
{
    public interface IMerchCategoryTypeRepository
    {
        List<MerchCategoryType> GetTypes();
        Task<List<MerchCategoryType>> GetTypesAsync(CancellationToken cancellationToken = default);
    }
}
