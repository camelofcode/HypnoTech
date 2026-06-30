using HypnoTech.Ecommerce.Frontend.Services.Interfaces;
using HypnoTech.Ecommerce.Models.Basket;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult GetDetails(BasketSummary summary)
        {
            return Ok(_basketService.GetDetails(summary));
        }


    }
}
