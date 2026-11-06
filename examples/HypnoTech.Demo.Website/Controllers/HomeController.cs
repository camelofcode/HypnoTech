using System.Linq;
using System.Threading.Tasks;
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

    public async Task<IActionResult> Index()
    {
        var cancellationToken = HttpContext.RequestAborted;
        var articlesTask = _articles.GetAllAsync(cancellationToken);
        var eventsTask = _events.GetAllAsync(cancellationToken);
        var artistsTask = _artists.GetAllAsync(cancellationToken);
        var shopItemsTask = _merch.GetMerchFlattenedAsync(cancellationToken);

        await Task.WhenAll(articlesTask, eventsTask, artistsTask, shopItemsTask);

        var articles = await articlesTask;
        var events = await eventsTask;
        var artists = await artistsTask;
        var shopItems = await shopItemsTask;

        return AjaxPartialView(new HomeViewModel
        {
            Articles = articles.OrderByDescending(item => item.PublishedAt).Take(2).ToList(),
            Events = events.OrderBy(item => item.StartsAt).Take(3).ToList(),
            Artists = artists.OrderBy(item => item.Order).Take(3).ToList(),
            ShopItems = shopItems.Take(2).ToList()
        });
    }
}
