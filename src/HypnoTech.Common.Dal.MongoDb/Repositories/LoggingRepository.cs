using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Dtos.Responses;
using HypnoTech.Common.Models.Settings;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class LoggingRepository : BaseMongoRepository, ILoggingRepository
    {
        public LoggingRepository(IOptions<MongoOptions> config) : base(config, "serilog")
        {
        }

        public PagedListResponse<SerilogEntry> GetLogs(PagedRequest request)
        {
            return GetPaged<SerilogEntry>(request, "Timestamp", true);
        }

        public SerilogEntry GetLog(string id)
        {
            return GetByObjectId<SerilogEntry>(id);
        }
    }
}
