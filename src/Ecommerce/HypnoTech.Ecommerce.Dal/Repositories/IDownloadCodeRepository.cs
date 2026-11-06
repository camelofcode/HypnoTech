using System.Collections.Generic;
using HypnoTech.Ecommerce.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Dal.Repositories
{
    public interface IDownloadCodeRepository
    {
        AssignDownloadCodesResponse AssignDownloadCodes(Order order);
        Task<AssignDownloadCodesResponse> AssignDownloadCodesAsync(Order order, CancellationToken cancellationToken = default);

        List<DownloadCodeCollection> GetAllCodeCollections();
        Task<List<DownloadCodeCollection>> GetAllCodeCollectionsAsync(CancellationToken cancellationToken = default);

        void AddCodeCollection(DownloadCodeCollection codes);
        Task AddCodeCollectionAsync(DownloadCodeCollection codes, CancellationToken cancellationToken = default);

        DownloadCodeCollection GetDownloadCodeCollection(string id);
        Task<DownloadCodeCollection> GetDownloadCodeCollectionAsync(string id, CancellationToken cancellationToken = default);

        string AssignDownloadCode(string collectionId, string emailAddress);
        Task<string> AssignDownloadCodeAsync(string collectionId, string emailAddress, CancellationToken cancellationToken = default);
    }
}
