using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HypnoTech.Cms.Mvc;
using HypnoTech.Cms.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Xunit;

namespace HypnoTech.Cms.UnitTests;

public sealed class CmsConventionTests : IDisposable
{
    public CmsConventionTests()
    {
        FormClassMapping.Maps.Clear();
        AdminCmsNavbarCache.Links.Clear();
    }

    public void Dispose()
    {
        FormClassMapping.Maps.Clear();
        AdminCmsNavbarCache.Links.Clear();
    }

    [Fact]
    public void Feature_provider_adds_closed_generic_controller_and_navigation_link()
    {
        FormClassMapping.RegisterClassMapping<SyntheticPage>(map =>
        {
            map.SetControllerName("SyntheticPages");
            map.SetDisplayName("Synthetic Page");
            map.SetIcon("file");
            map.RequiresRole("synthetic-page-editor");
        });
        var feature = new ControllerFeature();

        new CmsControllerFeatureProvider().PopulateFeature(Array.Empty<Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPart>(), feature);

        Assert.Contains(typeof(CmsController<SyntheticPage>).GetTypeInfo(), feature.Controllers);
        var link = Assert.Single(AdminCmsNavbarCache.Links);
        Assert.Equal("Synthetic Page", link.Text);
        Assert.Equal("/Cms/SyntheticPages", link.Url);
        Assert.Equal("SyntheticPages", link.Controller);
        Assert.Equal("file", link.Icon);
        Assert.Equal("synthetic-page-editor", link.Role);
    }

    [Fact]
    public void Route_convention_adds_mapping_controller_route_to_generic_controller()
    {
        FormClassMapping.RegisterClassMapping<SyntheticPage>(map =>
        {
            map.SetControllerName("SyntheticPages");
            map.SetDisplayName("Synthetic Page");
        });
        var model = new ControllerModel(
            typeof(CmsController<SyntheticPage>).GetTypeInfo(),
            new List<object>());

        new CmsControllerRouteConvention().Apply(model);

        var route = Assert.Single(model.Selectors).AttributeRouteModel;
        Assert.NotNull(route);
        Assert.Equal("Cms/SyntheticPages", route.Template);
    }

    public sealed class SyntheticPage
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }
}
