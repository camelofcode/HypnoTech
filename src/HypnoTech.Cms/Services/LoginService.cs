using System.Collections.Generic;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common;
using HypnoTech.Common.Dal.Repositories;

namespace HypnoTech.Cms.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _repository;

        public LoginService(IUserRepository repository)
        {
            _repository = repository;
        }

        public bool VerifyUser(string username, string password)
        {
            var user = _repository.GetUserForLogin(username);
            return user != null && PasswordHelper.VerifyPassword(password, user.Password);
        }

        public List<string> GetRoles(string username)
        {
            return _repository.GetUserRoles(username);
        }
    }
}
