using System.Linq;
using HypnoTech.Demo.Models;
using HypnoTech.Demo.Website.Dal;
using HypnoTech.Demo.Website.ViewModels;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class HomeController : SpaController
{
    private readonly IArticleRepository _articles;
    private readonly IEventRepository _events;
    private readonly IArtistRepository _artists;
    private readonly IMerchRepository _merch;

    public HomeController(
        IArticleRepository articles,
        IEventRepository events,
        IArtistRepository artists,
        IMerchRepository merch)
    {
        _articles = articles;
        _events = events;
        _artists = artists;
        _merch = merch;
    }

    public IActionResult Index()
    {
        return AjaxPartialView(new HomeViewModel
        {
            Articles = _articles.GetAll().OrderByDescending(item => item.PublishedAt).Take(2).ToList(),
            Events = _events.GetAll().OrderBy(item => item.StartsAt).Take(3).ToList(),
            Artists = _artists.GetAll().OrderBy(item => item.Order).Take(3).ToList(),
            ShopItems = _merch.GetMerchFlattened().Take(2).ToList()
        });
    }
}
