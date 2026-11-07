using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Services.Interfaces
{
    public interface IBackupService
    {
        Task<dynamic> GetAllDataAsync(List<string> includedTypes, CancellationToken cancellationToken = default);

        Task<byte[]> GetAllFilesAsync(List<string> includedTypes, CancellationToken cancellationToken = default);
    }
}
