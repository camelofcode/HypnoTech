using System.Threading.Tasks;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Frontend.Attributes;
using HypnoTech.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class ShopController : SpaController
{
    private readonly IMerchRepository _merch;

    public ShopController(IMerchRepository merch)
    {
        _merch = merch;
    }

    [NavigationLink("shop")]
    public async Task<IActionResult> Index() =>
        AjaxPartialView(await _merch.GetMerchAsync(HttpContext.RequestAborted));

    [HttpGet("shop/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var item = await _merch.GetMerchItemAsync(slug, HttpContext.RequestAborted);
        return item == null ? NotFound() : AjaxPartialView(item);
    }
}
