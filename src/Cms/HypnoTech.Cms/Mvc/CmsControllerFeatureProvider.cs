using System.Collections.Generic;
using System.Reflection;
using HypnoTech.Cms.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace HypnoTech.Cms.Mvc
{
    public class CmsControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var mapping in FormClassMapping.Maps)
            {
                feature.Controllers.Add(typeof(CmsController<>).MakeGenericType(mapping.Key).GetTypeInfo());
                AdminCmsNavbarCache.Add(mapping.Value.DisplayName, mapping.Value.ControllerName, mapping.Value.Icon, mapping.Value.ControllerName, mapping.Value.RequiredRole);
            }
        }
    }
}
