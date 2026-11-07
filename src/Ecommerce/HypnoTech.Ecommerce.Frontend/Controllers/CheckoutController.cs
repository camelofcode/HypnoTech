using System;
using HypnoTech.Ecommerce.Frontend.Services.Interfaces;
using HypnoTech.Ecommerce.Models;
using HypnoTech.Ecommerce.Models.Basket;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index()
        {
            return View(await _checkoutService.GetCheckoutAsync(HttpContext.RequestAborted));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetDetails(CheckoutSummary summary)
        {
            return Ok(await _checkoutService.GetCheckoutDetailsAsync(
                summary,
                HttpContext.RequestAborted));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(CheckoutSummary summary, PostageAddress address)
        {
            var response = await _checkoutService.PayAsync(
                summary,
                address,
                HttpContext.RequestAborted);
            if (response.Status == "SUCCESS")
            {
                TempData["PaymentId"] = response.Order.PaymentId;
            }

            return Ok(response);
        }

        public async Task<IActionResult> Complete(string payerId, string paymentId)
        {
            paymentId = GetPaymentId(paymentId);

            if (string.IsNullOrEmpty(paymentId))
            {
                throw new Exception("Payment could not be completed as Payment Id could not be found from any source - tried query string, cookie and tempdata to no avail.");
            }

            var result = await _checkoutService.CompleteAsync(
                paymentId,
                payerId,
                HttpContext.RequestAborted);

            return View(result);
        }

        public async Task<IActionResult> Cancelled(string paymentId)
        {
            paymentId = GetPaymentId(paymentId);

            if (!string.IsNullOrEmpty(paymentId))
            {
                await _checkoutService.CancelAsync(paymentId, HttpContext.RequestAborted);
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
