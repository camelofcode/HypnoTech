using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IConfigAdminRepository
    {
        Config GetConfig();

        void SetConfig(Config config);
    }
}
