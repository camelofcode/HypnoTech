using System.Linq;
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
    public IActionResult Index() =>
        AjaxPartialView(_articles.GetAll().OrderByDescending(item => item.PublishedAt).ToList());

    [HttpGet("articles/{slug}")]
    public IActionResult Details(string slug)
    {
        var article = _articles.GetBySlug(slug);
        return article == null ? NotFound() : AjaxPartialView(article);
    }
}
