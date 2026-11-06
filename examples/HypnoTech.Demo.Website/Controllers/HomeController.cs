using System.Linq;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Demo.Models;
using HypnoTech.Demo.Website.ViewModels;
using HypnoTech.Ecommerce.Dal.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class HomeController : Controller
{
    private readonly ICmsRepository<Article> _articles;
    private readonly ICmsRepository<Event> _events;
    private readonly ICmsRepository<Artist> _artists;
    private readonly IMerchRepository _merch;

    public HomeController(
        ICmsRepository<Article> articles,
        ICmsRepository<Event> events,
        ICmsRepository<Artist> artists,
        IMerchRepository merch)
    {
        _articles = articles;
        _events = events;
        _artists = artists;
        _merch = merch;
    }

    public IActionResult Index()
    {
        return View(new HomeViewModel
        {
            Articles = _articles.GetAll<Article>().OrderByDescending(item => item.PublishedAt).Take(2).ToList(),
            Events = _events.GetAll<Event>().OrderBy(item => item.StartsAt).Take(3).ToList(),
            Artists = _artists.GetAll<Artist>().OrderBy(item => item.Order).Take(3).ToList(),
            ShopItems = _merch.GetMerchFlattened().Take(2).ToList()
        });
    }
}
