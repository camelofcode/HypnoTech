using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Frontend.Attributes;
using HypnoTech.Frontend.Caching;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Frontend.Controllers
{
    [ServiceFilter(typeof(ApiAuthorizeAttribute))]
    public class ApiController : Controller
    {
        private readonly INavBarRepository _navBarRepository;

        public ApiController(INavBarRepository navBarRepository)
        {
            _navBarRepository = navBarRepository;
        }

        public IActionResult ClearNavbarCache()
        {
            NavBarCache.Init(_navBarRepository);
            return Ok();
        }
    }
}
