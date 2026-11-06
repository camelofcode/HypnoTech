using System;
using System.Runtime.CompilerServices;
using HypnoTech.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HypnoTech.Common.Security
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RoleAuthorizeAttribute : Attribute, IActionFilter
    {
        private readonly string _role;

        public RoleAuthorizeAttribute(string role)
        {
            _role = role;
            RoleStore.RegisterSettingsRole(role);
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.UserHasRole(_role))
            {
                context.Result = new UnauthorizedResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
