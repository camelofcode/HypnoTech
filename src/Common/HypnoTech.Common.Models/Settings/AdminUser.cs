using System.Collections.Generic;

namespace HypnoTech.Common.Models.Settings
{
    public class AdminUser
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public List<string> Roles { get; set; }
    }
}
