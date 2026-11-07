using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BraintreeHttp;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Ecommerce.Frontend.Extensions;
using HypnoTech.Ecommerce.Frontend.Models;
using HypnoTech.Ecommerce.Frontend.Services.Interfaces;
using HypnoTech.Ecommerce.Models;
using HypnoTech.Ecommerce.Models.Basket;
using Newtonsoft.Json;
using PayPal.Core;
using PayPal.v1.Payments;

namespace HypnoTech.Ecommerce.Frontend.Services
{
    public class PayPalService : IPayPalService
    {
        private readonly IConfigAdminRepository _configRepository;

        public PayPalService(IConfigAdminRepository config)
        {
            _configRepository = config;
        }

        public async Task<PaypalCreateResponse> CreatePaymentAsync(
            CheckoutDetails details,
            PostageAddress address,
            CancellationToken cancellationToken = default)
        {
            var config = (await _configRepository.GetConfigAsync(cancellationToken)).Paypal;
            var client = BuildClient(config);
            PaymentCreateRequest request = new PaymentCreateRequest();
            request.RequestBody(BuildPayment(details, address, config));

            try
            {
                // PayPal v1 exposes no cancellation-token overload. Once creation has
                // started, await it to completion so a disconnected request cannot
                // leave a remotely-created payment without the local order record.
                HttpResponse response = await client.Execute(request);
                Payment result = response.Result<Payment>();
                return ToCreateResponse(result, details);
            }
            catch (HttpException httpException)
            {
                return HandleCreateHttpException(httpException);
            }
        }

        public async Task<PaypalExecuteResponse> ExecutePaymentAsync(
            string paymentId,
            string payerId,
            CancellationToken cancellationToken = default)
        {
            var config = (await _configRepository.GetConfigAsync(cancellationToken)).Paypal;
            var client = BuildClient(config);
            var request = new PaymentExecuteRequest(paymentId)
            {
                Body = new PaymentExecution
                {
                    PayerId = payerId
                }
            };

            // See CreatePaymentAsync: do not abandon a side-effecting provider call
            // that the obsolete SDK cannot actually cancel.
            HttpResponse response = await client.Execute(request);
            Payment result = response.Result<Payment>();
            return ToExecuteResponse(result, paymentId, payerId, config);
        }

        protected virtual PayPalHttpClient BuildClient(PaypalConfig config)
        {
            PayPalEnvironment environment = config.IsProduction
                ? new LiveEnvironment(config.ApiClientId, config.ApiClientSecret)
                : new SandboxEnvironment(config.ApiClientId, config.ApiClientSecret);
            return new PayPalHttpClient(environment);
        }

        private static PaypalCreateResponse ToCreateResponse(
            Payment result,
            CheckoutDetails details)
        {
            if (result.State.ToLowerInvariant() == "created")
            {
                var redirectUrl = result.Links.FirstOrDefault(p => p.Method == "REDIRECT")?.Href;
                return PaypalCreateResponse.Success(
                    result.ToOrder(details.DownloadCodeIds),
                    redirectUrl);
            }

            throw new Exception("Paypal Failed:" + result.FailureReason);
        }

        private static PaypalCreateResponse HandleCreateHttpException(HttpException exception)
        {
            try
            {
                dynamic error = JsonConvert.DeserializeObject(exception.Message);
                if (error?.name == "VALIDATION_ERROR")
                {
                    return PaypalCreateResponse.Invalid(error.details[0].issue.ToString());
                }
            }
            catch (JsonException)
            {
                // The provider did not return a validation payload; preserve the
                // existing debug-id exception below.
            }

            var debugId = exception.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();
            throw new Exception("Paypal Failed - DebugId: " + debugId, exception);
        }

        private static PaypalExecuteResponse ToExecuteResponse(
            Payment result,
            string paymentId,
            string payerId,
            PaypalConfig config)
        {

            var executed = new PaypalExecuteResponse
            {
                EmailAddress = result.Payer?.PayerInfo?.Email,
                PaymentId = paymentId,
                PayerId = payerId,
                SaleId = result.Transactions?.FirstOrDefault()?.RelatedResources?.FirstOrDefault()?.Sale?.Id,
                Status = "SUCCESS"
            };
            executed.SaleUrl = config.SalesUrl + executed.SaleId;

            return executed;
        }

        private static Payment BuildPayment(
            CheckoutDetails details,
            PostageAddress address,
            PaypalConfig config)
        {
            var payment = new Payment
            {
                Intent = "sale",
                Payer = new Payer
                {
                    PaymentMethod = "paypal"
                },
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = new Amount
                        {
                            Total = details.Total.ToString("F2"),
                            Currency = "GBP",
                            Details = new AmountDetails
                            {
                                Subtotal = details.SubTotal.ToString("F2"),
                                Tax = "0.00",
                                Shipping = (details.Postage + details.SpecialDeliveryCost).ToString("F2"),
                                HandlingFee = "0.00",
                                ShippingDiscount = "0.00",
                                Insurance = "0.00",
                                GiftWrap = "0.00"
                            }
                        },
                        Custom = details.Tracked ? "tracked" : "standard",
                        PaymentOptions = new PaymentOptions()
                        {
                            AllowedPaymentMethod = "INSTANT_FUNDING_SOURCE"
                        },
                        ItemList = new ItemList
                        {
                            ShippingAddress = new ShippingAddress
                            {
                                RecipientName = address.Name,
                                Line1 = address.Address1,
                                Line2 = address.Address2,
                                City = address.Town,
                                CountryCode = address.CountryCode,
                                PostalCode = address.PostalCode,
                                State = address.Region
                            }
                        }
                    }
                },
                NoteToPayer = null,
                RedirectUrls = new RedirectUrls
                {
                    CancelUrl = config.PaymentCancelledUrl,
                    ReturnUrl = config.PaymentCompleteUrl
                }
            };

            payment.Transactions.First().ItemList.Items = details.Items.Select(item => new Item
            {
                Name = item.DisplayName,
                Description = item.DisplayName,
                Quantity = item.Quantity.ToString(),
                Price = item.UnitPrice.ToString("F2"),
                Tax = "0.00",
                Sku = "1",
                Currency = "GBP"
            }).ToList();

            return payment;
        }
    }
}
