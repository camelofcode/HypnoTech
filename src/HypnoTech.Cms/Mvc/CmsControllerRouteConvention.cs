using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace HypnoTech.Cms.Mvc
{
    public class CmsControllerRouteConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.IsGenericType)
            {
                var type = controller.ControllerType.GenericTypeArguments[0];
                var mapping = FormClassMapping.GetMapping(type);

                controller.Selectors.Add(new SelectorModel
                {
                    AttributeRouteModel = new AttributeRouteModel(new RouteAttribute($"Cms/{mapping.ControllerName}"))
                });
            }
        }
    }
}
