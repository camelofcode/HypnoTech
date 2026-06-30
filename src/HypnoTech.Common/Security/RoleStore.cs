using System.Collections.Generic;
using System.Linq;

namespace HypnoTech.Common.Security
{
    public static class RoleStore
    {
        private static readonly List<string> SettingsRoles = new List<string>();
        private static readonly List<string> CmsRoles = new List<string>();

        public static List<string> GetAllRoles()
        {
            return SettingsRoles.Concat(CmsRoles).ToList();
        }

        public static List<string> GetAllSettingsRoles()
        {
            return SettingsRoles.ToList();
        }

        public static List<string> GetAllCmsRoles()
        {
            return CmsRoles;
        }

        public static void RegisterCmsRole(string role)
        {
            if (!CmsRoles.Contains(role))
            {
                CmsRoles.Add(role);
            }
        }

        public static void RegisterSettingsRole(string role)
        {
            if (!GetAllSettingsRoles().Contains(role))
            {
                SettingsRoles.Add(role);
            }
        }
    }
}
