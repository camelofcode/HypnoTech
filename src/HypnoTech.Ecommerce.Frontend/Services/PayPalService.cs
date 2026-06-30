using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly PayPalHttpClient _client;
        private readonly PaypalConfig _config;

        public PayPalService(IConfigAdminRepository config)
        {
            _config = config.GetConfig().Paypal;

            PayPalEnvironment environment;
            if (_config.IsProduction)
            {
                environment = new LiveEnvironment(_config.ApiClientId, _config.ApiClientSecret);
            }
            else
            {
                environment = new SandboxEnvironment(_config.ApiClientId, _config.ApiClientSecret);
            }

            _client = new PayPalHttpClient(environment);
        }

        public PaypalCreateResponse CreatePayment(CheckoutDetails details, PostageAddress address)
        {
            PaymentCreateRequest request = new PaymentCreateRequest();
            request.RequestBody(BuildPayment(details, address));

            try
            {
                HttpResponse response = _client.Execute(request).Result;
                Payment result = response.Result<Payment>();

                if (result.State.ToLowerInvariant() == "created")
                {
                    var redirectUrl = result.Links.FirstOrDefault(p => p.Method == "REDIRECT")?.Href;
                    return PaypalCreateResponse.Success(result.ToOrder(details.DownloadCodeIds), redirectUrl);
                }

                throw new Exception("Paypal Failed:" + result.FailureReason);
            }
            catch (HttpException httpException)
            {
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();
                throw new Exception("Paypal Failed - DebugId: " + debugId, httpException);
            }
            catch (AggregateException aggregateException)
            {
                if (aggregateException.InnerException != null)
                {
                    dynamic error = JsonConvert.DeserializeObject(aggregateException.InnerException.Message);
                    if (error.name == "VALIDATION_ERROR")
                    {
                        var validationMessage = error.details[0].issue.ToString();
                        return PaypalCreateResponse.Invalid(validationMessage);
                    }
                }
                throw new Exception("Paypal Failed - Reason Unknown");
            }
        }

        public PaypalExecuteResponse ExecutePayment(string paymentId, string payerId)
        {
            var execution = new PaymentExecution
            {
                PayerId = payerId
            };

            var request = new PaymentExecuteRequest(paymentId)
            {
                Body = execution
            };

            HttpResponse response = _client.Execute(request).Result;
            Payment result = response.Result<Payment>();

            var executed = new PaypalExecuteResponse
            {
                EmailAddress = result.Payer?.PayerInfo?.Email,
                PaymentId = paymentId,
                PayerId = payerId,
                SaleId = result.Transactions?.FirstOrDefault()?.RelatedResources?.FirstOrDefault()?.Sale?.Id,
                Status = "SUCCESS"
            };
            executed.SaleUrl = _config.SalesUrl + executed.SaleId;

            return executed;
        }

        private Payment BuildPayment(CheckoutDetails details, PostageAddress address)
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
                    CancelUrl = _config.PaymentCancelledUrl,
                    ReturnUrl = _config.PaymentCompleteUrl
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
