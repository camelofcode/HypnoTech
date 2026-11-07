using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HypnoTech.EntityFrameworkCore
{
    internal sealed class HypnoTechDatabaseHostedService<TContext> : IHostedService
        where TContext : DbContext
    {
        private readonly IDbContextFactory<TContext> _contextFactory;
        private readonly IReadOnlyCollection<IHypnoTechStartupSeeder> _seeders;
        private readonly HypnoTechEntityFrameworkCoreOptions _options;

        public HypnoTechDatabaseHostedService(
            IDbContextFactory<TContext> contextFactory,
            IEnumerable<IHypnoTechStartupSeeder> seeders,
            IOptions<HypnoTechEntityFrameworkCoreOptions> options)
        {
            _contextFactory = contextFactory;
            _seeders = seeders.ToList();
            _options = options.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_options.ApplyMigrationsAtStartup)
            {
                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
                await context.Database.MigrateAsync(cancellationToken);
            }

            if (_options.SeedAtStartup)
            {
                foreach (var seeder in _seeders.OrderBy(seeder => seeder.Order))
                {
                    await seeder.SeedAsync(cancellationToken);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
