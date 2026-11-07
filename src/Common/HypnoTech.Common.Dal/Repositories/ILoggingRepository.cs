using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Dtos.Responses;
using HypnoTech.Common.Models.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface ILoggingRepository
    {
        Task<PagedListResponse<SerilogEntry>> GetLogsAsync(PagedRequest request, CancellationToken cancellationToken = default);
        Task<SerilogEntry> GetLogAsync(string id, CancellationToken cancellationToken = default);
    }
}
