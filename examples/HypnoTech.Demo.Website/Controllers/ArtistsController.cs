using System.Linq;
using System.Threading.Tasks;
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
    public async Task<IActionResult> Index()
    {
        var artists = await _artists.GetAllAsync(HttpContext.RequestAborted);
        return AjaxPartialView(artists.OrderBy(item => item.Order).ToList());
    }
}
