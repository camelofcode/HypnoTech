using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Frontend.Controllers
{
    [Route("Error")]
    public class ErrorController : SpaController
    {
        [Route("500")]
        public IActionResult Error500()
        {
            Response.StatusCode = 500;
            return AjaxPartialView("Error");
        }

        [Route("{code}")]
        public IActionResult StatusCode(HttpStatusCode code)
        {
            if (code == HttpStatusCode.NotFound)
            {
                Response.StatusCode = 404;
                return AjaxPartialView("NotFound");
            }

            //I think this is fair enough as standard behaviour for front end sites. To the average user, other than 404 and status code is some random error.
            //Someone building a site on top of the framework can of course build custom controllers with specific code routes e.g. /Error/401 if they wished.
            Response.StatusCode = 500;
            return AjaxPartialView("Error");
        }
    }
}
