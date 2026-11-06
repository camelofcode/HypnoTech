using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Services.Interfaces
{
    public interface IBackupService
    {
        dynamic GetAllData(List<string> includedTypes);
        Task<dynamic> GetAllDataAsync(List<string> includedTypes, CancellationToken cancellationToken = default);

        byte[] GetAllFiles(List<string> includedTypes);
        Task<byte[]> GetAllFilesAsync(List<string> includedTypes, CancellationToken cancellationToken = default);
    }
}
