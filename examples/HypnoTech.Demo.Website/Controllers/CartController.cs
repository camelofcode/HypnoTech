using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class CartController : Controller
{
    public IActionResult Index() => View();
}
