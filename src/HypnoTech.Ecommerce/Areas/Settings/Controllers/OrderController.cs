using System.Linq;
using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Emails;
using HypnoTech.Common.Models;
using HypnoTech.Common.Security;
using HypnoTech.Ecommerce.Areas.Settings.Models;
using HypnoTech.Ecommerce.Dal.Repositories;
using Microsoft.AspNetCore.Mvc;

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

            public IActionResult Index(int page = 1)
            {
                var response = _repository.GetOrders(new PagedRequest
                {
                    PageSize = 15,
                    Page = page
                });

                var model = new OrdersViewModel
                {
                    Orders = response.Items.ToList(),
                    Pager = new PagerViewModel(response.CurrentPage, response.TotalPages, HttpContext)
                };

                return View(model);
            }

            public IActionResult Details(string id)
            {
                return View(_repository.GetOrder(id));
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult MarkAsShipped(string id)
            {
                var order = _repository.MarkAsShipped(id);

                if (!string.IsNullOrEmpty(order.EmailAddress))
                {
                    _emailService.SendEmail(order.EmailAddress, "OrderShipped", "Order", new OrderShippedEmailViewModel(order));
                }
                return RedirectToAction("Index");
            }
        }
}
