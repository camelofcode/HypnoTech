using System;
using System.Diagnostics;
using System.Linq;
using HypnoTech.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HypnoTech.Cms.Mvc
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CmsRoleAuthorizeAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var path = context.HttpContext.Request.Path.Value;
            var controllerName = path.Split("/")[2];
            var role = FormClassMapping.Maps.FirstOrDefault(p => p.Value.ControllerName == controllerName).Value.RequiredRole;

            if (!context.HttpContext.UserHasRole(role))
            {
                context.Result = new UnauthorizedResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
