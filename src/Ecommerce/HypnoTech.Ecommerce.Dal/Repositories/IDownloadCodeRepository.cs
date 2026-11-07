using System.Collections.Generic;
using HypnoTech.Ecommerce.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Dal.Repositories
{
    public interface IDownloadCodeRepository
    {
        Task<AssignDownloadCodesResponse> AssignDownloadCodesAsync(Order order, CancellationToken cancellationToken = default);
        Task<List<DownloadCodeCollection>> GetAllCodeCollectionsAsync(CancellationToken cancellationToken = default);
        Task AddCodeCollectionAsync(DownloadCodeCollection codes, CancellationToken cancellationToken = default);
        Task<DownloadCodeCollection> GetDownloadCodeCollectionAsync(string id, CancellationToken cancellationToken = default);
        Task<string> AssignDownloadCodeAsync(string collectionId, string emailAddress, CancellationToken cancellationToken = default);
    }
}
