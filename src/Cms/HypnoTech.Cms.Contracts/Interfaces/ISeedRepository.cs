using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Contracts.Interfaces
{
    public interface ISeedRepository
    {
        void Seed<T>(string collectionName, string fileName);

        Task SeedAsync<T>(string collectionName, string fileName, CancellationToken cancellationToken = default);
    }
}
