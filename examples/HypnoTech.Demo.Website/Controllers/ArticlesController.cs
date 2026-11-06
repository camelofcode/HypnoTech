using System.Linq;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Demo.Models;
using HypnoTech.Frontend.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class ArticlesController : Controller
{
    private readonly ICmsRepository<Article> _articles;

    public ArticlesController(ICmsRepository<Article> articles)
    {
        _articles = articles;
    }

    [NavigationLink("articles")]
    public IActionResult Index() =>
        View(_articles.GetAll<Article>().OrderByDescending(item => item.PublishedAt).ToList());

    [HttpGet("articles/{slug}")]
    public IActionResult Details(string slug)
    {
        var article = _articles.GetAll<Article>().FirstOrDefault(item => item.Slug == slug);
        return article == null ? NotFound() : View(article);
    }
}
