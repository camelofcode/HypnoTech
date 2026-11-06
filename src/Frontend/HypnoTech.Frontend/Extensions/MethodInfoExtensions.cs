using System.Linq;
using System.Reflection;
using HypnoTech.Frontend.Attributes;

namespace HypnoTech.Frontend.Extensions
{
    public static class MethodInfoExtensions
    {
        public static string GetUrl(this MethodInfo method)
        {
            var url = $"/{method.DeclaringType.Name.Replace("Controller", string.Empty)}";

            if (method.Name != "Index")
            {
                url += $"/{method.Name.ToLower()}";
            }

            return url;
        }

        public static string GetNavigationLinkId(this MethodInfo method)
        {
            return (method.GetCustomAttributes(typeof(NavigationLinkAttribute), false).First() as NavigationLinkAttribute)?.Id;
        }
    }
}
