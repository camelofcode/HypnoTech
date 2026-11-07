using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.EntityFrameworkCore
{
    /// <summary>
    /// Defines an application or module seed routine that runs after EF Core migrations.
    /// A routine may use lower-level repositories such as the CMS ISeedRepository.
    /// </summary>
    public interface IHypnoTechStartupSeeder
    {
        int Order { get; }

        Task SeedAsync(CancellationToken cancellationToken = default);
    }
}
