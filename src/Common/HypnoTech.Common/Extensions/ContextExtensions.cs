using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using HypnoTech.Common.Security;
using Microsoft.AspNetCore.Http;

namespace HypnoTech.Common.Extensions
{
    public static class ContextExtensions
    {
        public static bool UserHasRole(this HttpContext context, string role)
        {
            return context.User.UserHasRole(role);
        }

        public static bool UserHasRole(this ClaimsPrincipal user, string role)
        {
            return user.Claims.Any(p => p.Type == ClaimTypes.Role && p.Value.Equals(role));
        }

        public static bool UserHasSettingsRoles(this HttpContext context)
        {
            return context.User.UserHasSettingsRoles();
        }

        public static bool UserHasSettingsRoles(this ClaimsPrincipal user)
        {
            return user.Claims.Any(p => p.Type == ClaimTypes.Role && RoleStore.GetAllSettingsRoles().Contains(p.Value));
        }

        public static bool UserHasCmsRoles(this HttpContext context)
        {
            return context.User.UserHasCmsRoles();
        }

        public static bool UserHasCmsRoles(this ClaimsPrincipal user)
        {
            return user.Claims.Any(p => p.Type == ClaimTypes.Role && RoleStore.GetAllCmsRoles().Contains(p.Value));
        }
    }
}
