using System;
using System.Linq;
using System.Net;
using HypnoTech.Frontend.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace HypnoTech.Frontend.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EnabledFeatureAttribute : ActionFilterAttribute
    {
        private readonly string _feature;

        public EnabledFeatureAttribute(string feature)
        {
            _feature = feature;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!NavBarCache.CachedLinks.Any(p => string.Equals(p.Key, _feature, StringComparison.CurrentCultureIgnoreCase)))
            {
                Log.Logger.Warning($"Attempted access to disabled feature {_feature} detected");
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Result = new LocalRedirectResult("~/NotFound404");
            }
        }
    }
}
