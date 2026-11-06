using System.Collections.Generic;
using HypnoTech.Ecommerce.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Dal.Repositories
{
    public interface IPostageProfileRepository
    {
        PostageProfile GetProfile(string name);
        Task<PostageProfile> GetProfileAsync(string name, CancellationToken cancellationToken = default);

        List<PostageProfile> GetProfiles();
        Task<List<PostageProfile>> GetProfilesAsync(CancellationToken cancellationToken = default);

        void AddProfile(PostageProfile profile);
        Task AddProfileAsync(PostageProfile profile, CancellationToken cancellationToken = default);

        PostageProfile GetProfileForEdit(string id);
        Task<PostageProfile> GetProfileForEditAsync(string id, CancellationToken cancellationToken = default);

        void UpdateProfile(PostageProfile profile);
        Task UpdateProfileAsync(PostageProfile profile, CancellationToken cancellationToken = default);

        string RemoveProfile(string id);
        Task<string> RemoveProfileAsync(string id, CancellationToken cancellationToken = default);
    }
}
