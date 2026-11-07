using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.EntityFrameworkCore
{
    public interface IHypnoTechDbContextFactory
    {
        ValueTask<DbContext> CreateDbContextAsync(CancellationToken cancellationToken = default);
    }
}
