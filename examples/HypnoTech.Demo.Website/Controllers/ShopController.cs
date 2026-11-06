using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Frontend.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class ShopController : Controller
{
    private readonly IMerchRepository _merch;

    public ShopController(IMerchRepository merch)
    {
        _merch = merch;
    }

    [NavigationLink("shop")]
    public IActionResult Index() => View(_merch.GetMerch());

    [HttpGet("shop/{slug}")]
    public IActionResult Details(string slug)
    {
        var item = _merch.GetMerchItem(slug);
        return item == null ? NotFound() : View(item);
    }
}
