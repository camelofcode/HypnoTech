using System.Linq;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Demo.Models;
using HypnoTech.Frontend.Attributes;
using HypnoTech.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class ArtistsController : SpaController
{
    private readonly ICmsRepository<Artist> _artists;

    public ArtistsController(ICmsRepository<Artist> artists)
    {
        _artists = artists;
    }

    [NavigationLink("artists")]
    public IActionResult Index() =>
        AjaxPartialView(_artists.GetAll<Artist>().OrderBy(item => item.Order).ToList());
}
