using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Services.Interfaces
{
    public interface ILoginService
    {
        Task<bool> VerifyUserAsync(string username, string password, CancellationToken cancellationToken = default);
        Task<List<string>> GetRolesAsync(string username, CancellationToken cancellationToken = default);
    }
}
