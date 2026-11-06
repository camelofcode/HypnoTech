using System.Linq;
using HypnoTech.Demo.Models;
using HypnoTech.Demo.Website.Dal;
using HypnoTech.Frontend.Attributes;
using HypnoTech.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class EventsController : SpaController
{
    private readonly IEventRepository _events;

    public EventsController(IEventRepository events)
    {
        _events = events;
    }

    [NavigationLink("events")]
    public IActionResult Index() =>
        AjaxPartialView(_events.GetAll().OrderBy(item => item.StartsAt).ToList());

    [HttpGet("events/{slug}")]
    public IActionResult Details(string slug)
    {
        var item = _events.GetBySlug(slug);
        return item == null ? NotFound() : AjaxPartialView(item);
    }
}
