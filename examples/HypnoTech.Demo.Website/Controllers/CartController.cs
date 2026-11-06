using HypnoTech.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class CartController : SpaController
{
    public IActionResult Index() => AjaxPartialView();
}
