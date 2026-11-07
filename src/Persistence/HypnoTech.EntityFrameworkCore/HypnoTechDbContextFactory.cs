using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.EntityFrameworkCore
{
    internal sealed class HypnoTechDbContextFactory<TContext> : IHypnoTechDbContextFactory
        where TContext : DbContext
    {
        private readonly IDbContextFactory<TContext> _factory;

        public HypnoTechDbContextFactory(IDbContextFactory<TContext> factory)
        {
            _factory = factory;
        }

        public async ValueTask<DbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default)
        {
            return await _factory.CreateDbContextAsync(cancellationToken);
        }
    }
}
