using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Dtos.Responses;
using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface ILoggingRepository
    {
        PagedListResponse<SerilogEntry> GetLogs(PagedRequest request);

        SerilogEntry GetLog(string id);
    }
}
