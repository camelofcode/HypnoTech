using System.Collections.Generic;
using HypnoTech.Ecommerce.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Dal.Repositories
{
    public interface IPostageProfileRepository
    {
        Task<PostageProfile> GetProfileAsync(string name, CancellationToken cancellationToken = default);
        Task<List<PostageProfile>> GetProfilesAsync(CancellationToken cancellationToken = default);
        Task AddProfileAsync(PostageProfile profile, CancellationToken cancellationToken = default);
        Task<PostageProfile> GetProfileForEditAsync(string id, CancellationToken cancellationToken = default);
        Task UpdateProfileAsync(PostageProfile profile, CancellationToken cancellationToken = default);
        Task<string> RemoveProfileAsync(string id, CancellationToken cancellationToken = default);
    }
}
