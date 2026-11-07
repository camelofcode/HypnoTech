using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Dtos.Responses;
using HypnoTech.Common.Models.Settings;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class LoggingRepository : BaseMongoRepository, ILoggingRepository
    {
        public LoggingRepository(IOptions<MongoOptions> config) : base(config, "serilog")
        {
        }

        public Task<PagedListResponse<SerilogEntry>> GetLogsAsync(PagedRequest request, CancellationToken cancellationToken = default)
        {
            return GetPagedAsync<SerilogEntry>(request, "Timestamp", true, cancellationToken);
        }

        public Task<SerilogEntry> GetLogAsync(string id, CancellationToken cancellationToken = default)
        {
            return GetByObjectIdAsync<SerilogEntry>(id, cancellationToken);
        }
    }
}
