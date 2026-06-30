using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace HypnoTech.Frontend.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ApiAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly IConfiguration _config;

        public ApiAuthorizeAttribute(IConfiguration config)
        {
            _config = config;
        }

        private void HandleUnauthorizedRequest(ActionExecutingContext context)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Result = new JsonResult(new { HttpStatusCode.Unauthorized });
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Headers["X-Auth-Token"].Any())
            {
                HandleUnauthorizedRequest(context);
                return;
            }

            var authHeader = context.HttpContext.Request.Headers["X-Auth-Token"].First();
            var apiToken = _config["ApiKey"];
            if (authHeader == null || authHeader != apiToken)
            {
                HandleUnauthorizedRequest(context);
            }
        }
    }
}
