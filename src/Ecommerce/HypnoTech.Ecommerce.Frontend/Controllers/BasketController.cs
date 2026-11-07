using HypnoTech.Ecommerce.Frontend.Services.Interfaces;
using HypnoTech.Ecommerce.Models.Basket;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Frontend.Controllers
{
    public class BasketController : Controller
    {
        private readonly IBasketService _basketService;

        public BasketController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetDetails(BasketSummary summary)
        {
            return Ok(await _basketService.GetDetailsAsync(
                summary,
                HttpContext.RequestAborted));
        }


    }
}
