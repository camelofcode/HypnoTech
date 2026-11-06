using System.Linq;
using System.Threading.Tasks;
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
    public async Task<IActionResult> Index()
    {
        var events = await _events.GetAllAsync(HttpContext.RequestAborted);
        return AjaxPartialView(events.OrderBy(item => item.StartsAt).ToList());
    }

    [HttpGet("events/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var item = await _events.GetBySlugAsync(slug, HttpContext.RequestAborted);
        return item == null ? NotFound() : AjaxPartialView(item);
    }
}
