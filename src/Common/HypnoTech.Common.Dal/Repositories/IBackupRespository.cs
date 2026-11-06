using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IBackupRespository
    {
        dynamic GetAllData(List<string> includedTypes);
        Task<dynamic> GetAllDataAsync(List<string> includedTypes, CancellationToken cancellationToken = default);
    }
}
