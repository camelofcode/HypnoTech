using System.Linq;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Demo.Models;
using HypnoTech.Frontend.Attributes;
using HypnoTech.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class EventsController : SpaController
{
    private readonly ICmsRepository<Event> _events;

    public EventsController(ICmsRepository<Event> events)
    {
        _events = events;
    }

    [NavigationLink("events")]
    public IActionResult Index() =>
        AjaxPartialView(_events.GetAll<Event>().OrderBy(item => item.StartsAt).ToList());

    [HttpGet("events/{slug}")]
    public IActionResult Details(string slug)
    {
        var item = _events.GetAll<Event>().FirstOrDefault(item => item.Slug == slug);
        return item == null ? NotFound() : AjaxPartialView(item);
    }
}
