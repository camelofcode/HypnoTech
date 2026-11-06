using System;
using HypnoTech.Ecommerce.Frontend.Services.Interfaces;
using HypnoTech.Ecommerce.Models;
using HypnoTech.Ecommerce.Models.Basket;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Ecommerce.Frontend.Controllers
{
    // Checkout is an intentional full-document boundary and must not inherit SpaController.
    public class CheckoutController : Controller
    {
        private readonly ICheckoutService _checkoutService;

        public CheckoutController(ICheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        public IActionResult Index()
        {
            return View(_checkoutService.GetCheckout());
        }

        [HttpPost]
        public IActionResult GetDetails(CheckoutSummary summary)
        {
            return Ok(_checkoutService.GetCheckoutDetails(summary));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Pay(CheckoutSummary summary, PostageAddress address)
        {
            var response = _checkoutService.Pay(summary, address);
            if (response.Status == "SUCCESS")
            {
                TempData["PaymentId"] = response.Order.PaymentId;
            }

            return Ok(response);
        }

        public IActionResult Complete(string payerId, string paymentId)
        {
            paymentId = GetPaymentId(paymentId);

            if (string.IsNullOrEmpty(paymentId))
            {
                throw new Exception("Payment could not be completed as Payment Id could not be found from any source - tried query string, cookie and tempdata to no avail.");
            }

            var result = _checkoutService.Complete(paymentId, payerId);

            return View(result);
        }

        public IActionResult Cancelled(string paymentId)
        {
            paymentId = GetPaymentId(paymentId);

            if (!string.IsNullOrEmpty(paymentId))
            {
                _checkoutService.Cancel(paymentId);
            }

            return View();
        }

        private string GetPaymentId(string paymentId)
        {
            if (!string.IsNullOrEmpty(paymentId))
            {
                return paymentId;
            }

            paymentId = HttpContext.Request.Cookies["paypal_payment_id"];

            if (!string.IsNullOrEmpty(paymentId))
            {
                HttpContext.Response.Cookies.Delete("paypal_payment_id");
                return paymentId;
            }

            if (TempData.ContainsKey("PaymentId"))
            {
                paymentId = TempData["PaymentId"].ToString();
                if (!string.IsNullOrEmpty(paymentId))
                {
                    return paymentId;
                }
            }

            return null;
        }
    }
}
