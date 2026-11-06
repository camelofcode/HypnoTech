using System.Collections.Generic;
using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Ecommerce.Dal.Repositories
{
    public interface IDownloadCodeRepository
    {
        AssignDownloadCodesResponse AssignDownloadCodes(Order order);

        List<DownloadCodeCollection> GetAllCodeCollections();

        void AddCodeCollection(DownloadCodeCollection codes);

        DownloadCodeCollection GetDownloadCodeCollection(string id);

        string AssignDownloadCode(string collectionId, string emailAddress);
    }
}
