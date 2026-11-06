using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Services.Interfaces
{
    public interface ILoginService
    {
        bool VerifyUser(string username, string password);
        Task<bool> VerifyUserAsync(string username, string password, CancellationToken cancellationToken = default);
        List<string> GetRoles(string username);
        Task<List<string>> GetRolesAsync(string username, CancellationToken cancellationToken = default);
    }
}
