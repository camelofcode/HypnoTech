using System.Collections.Generic;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common;
using HypnoTech.Common.Dal.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _repository;

        public LoginService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> VerifyUserAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var user = await _repository.GetUserForLoginAsync(username, cancellationToken);
            return user != null && PasswordHelper.VerifyPassword(password, user.Password);
        }

        public Task<List<string>> GetRolesAsync(string username, CancellationToken cancellationToken = default)
        {
            return _repository.GetUserRolesAsync(username, cancellationToken);
        }
    }
}
