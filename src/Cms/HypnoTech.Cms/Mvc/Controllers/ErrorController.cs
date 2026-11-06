using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Cms.Mvc.Controllers
{
    [Route("Error")]
    public class ErrorController : Controller
    {
        [Route("500")]
        public IActionResult Error500()
        {
            Response.StatusCode = 500;
            return View("Error");
        }

        [Route("{code}")]
        public IActionResult StatusCodeError(int code)
        {
            switch (code)
            {
                case (int)HttpStatusCode.NotFound:
                    return View("NotFound");
                case (int)HttpStatusCode.Unauthorized:
                case (int)HttpStatusCode.Forbidden:
                    return View("Security");
                case 418:
                    return View("Teapot"); //Important to implement meme status code.
                default:
                    return View("Error");
            }
        }
    }
}
