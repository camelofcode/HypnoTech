using System;
using System.Collections.Generic;
using System.Text;

namespace HypnoTech.Cms.Services.Interfaces
{
    public interface ILoginService
    {
        bool VerifyUser(string username, string password);
        List<string> GetRoles(string username);
    }
}
