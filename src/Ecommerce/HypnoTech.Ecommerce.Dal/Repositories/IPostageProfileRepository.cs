using System.Collections.Generic;
using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Ecommerce.Dal.Repositories
{
    public interface IPostageProfileRepository
    {
        PostageProfile GetProfile(string name);

        List<PostageProfile> GetProfiles();

        void AddProfile(PostageProfile profile);

        PostageProfile GetProfileForEdit(string id);

        void UpdateProfile(PostageProfile profile);

        string RemoveProfile(string id);
    }
}
