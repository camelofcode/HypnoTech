using System.Linq;
using HypnoTech.Demo.Models;
using HypnoTech.Demo.Website.Dal;
using HypnoTech.Frontend.Attributes;
using HypnoTech.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class ArtistsController : SpaController
{
    private readonly IArtistRepository _artists;

    public ArtistsController(IArtistRepository artists)
    {
        _artists = artists;
    }

    [NavigationLink("artists")]
    public IActionResult Index() =>
        AjaxPartialView(_artists.GetAll().OrderBy(item => item.Order).ToList());
}
