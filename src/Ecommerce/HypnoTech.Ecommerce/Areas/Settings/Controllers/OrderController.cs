using System.Linq;
using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Emails;
using HypnoTech.Common.Models;
using HypnoTech.Common.Security;
using HypnoTech.Ecommerce.Areas.Settings.Models;
using HypnoTech.Ecommerce.Dal.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Areas.Settings.Controllers
{

        [Area("Settings")]
        [RoleAuthorize("orders")]
        public class OrderController : Controller
        {
            private readonly IOrderRepository _repository;
            
            private readonly IEmailService _emailService;

            public OrderController(IOrderRepository repository, IEmailService emailService)
            {
                _repository = repository;
                _emailService = emailService;
            }

            public async Task<IActionResult> Index(int page = 1)
            {
                var response = await _repository.GetOrdersAsync(
                    new PagedRequest
                    {
                        PageSize = 15,
                        Page = page
                    },
                    HttpContext.RequestAborted);

                var model = new OrdersViewModel
                {
                    Orders = response.Items.ToList(),
                    Pager = new PagerViewModel(response.CurrentPage, response.TotalPages, HttpContext)
                };

                return View(model);
            }

            public async Task<IActionResult> Details(string id)
            {
                return View(await _repository.GetOrderAsync(
                    id,
                    HttpContext.RequestAborted));
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> MarkAsShipped(string id)
            {
                var order = await _repository.MarkAsShippedAsync(
                    id,
                    HttpContext.RequestAborted);

                if (!string.IsNullOrEmpty(order.EmailAddress))
                {
                    await _emailService.SendEmailAsync(
                        order.EmailAddress,
                        "OrderShipped",
                        "Order",
                        new OrderShippedEmailViewModel(order),
                        HttpContext.RequestAborted);
                }
                return RedirectToAction("Index");
            }
        }
}
