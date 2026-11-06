using System.Linq;
using System.Threading.Tasks;
using HypnoTech.Demo.Models;
using HypnoTech.Demo.Website.Dal;
using HypnoTech.Frontend.Attributes;
using HypnoTech.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class ArticlesController : SpaController
{
    private readonly IArticleRepository _articles;

    public ArticlesController(IArticleRepository articles)
    {
        _articles = articles;
    }

    [NavigationLink("articles")]
    public async Task<IActionResult> Index()
    {
        var articles = await _articles.GetAllAsync(HttpContext.RequestAborted);
        return AjaxPartialView(articles.OrderByDescending(item => item.PublishedAt).ToList());
    }

    [HttpGet("articles/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var article = await _articles.GetBySlugAsync(slug, HttpContext.RequestAborted);
        return article == null ? NotFound() : AjaxPartialView(article);
    }
}
