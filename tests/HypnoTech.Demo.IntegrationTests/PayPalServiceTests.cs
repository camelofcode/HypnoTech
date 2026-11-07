using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Ecommerce.Frontend.Services;
using HypnoTech.Ecommerce.Models;
using HypnoTech.Ecommerce.Models.Basket;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PayPal.Core;
using Xunit;

namespace HypnoTech.Demo.IntegrationTests;

public sealed class PayPalServiceTests
{
    [Fact]
    public async Task CreatePaymentAsync_maps_a_successful_rest_response_without_contacting_paypal()
    {
        await using var api = await PayPalApiStub.StartAsync();
        var service = CreateService(api.BaseUrl);

        var response = await service.CreatePaymentAsync(
            CreateCheckoutDetails(),
            CreateAddress(),
            TestContext.Current.CancellationToken);

        Assert.Equal("SUCCESS", response.Status);
        Assert.Equal("PAY-SYNTHETIC", response.Order.PaymentId);
        Assert.Equal(17.50m, response.Order.Total);
        Assert.Equal("https://example.test/approve/PAY-SYNTHETIC", response.RedirectUrl);
        Assert.Contains("/v1/oauth2/token", api.RequestPaths);
        Assert.Contains("/v1/payments/payment", api.RequestPaths);
        Assert.Contains("\"total\":\"17.50\"", api.CreateRequestBody);
    }

    [Fact]
    public async Task CreatePaymentAsync_preserves_paypal_validation_failures()
    {
        await using var api = await PayPalApiStub.StartAsync(rejectCreate: true);
        var service = CreateService(api.BaseUrl);

        var response = await service.CreatePaymentAsync(
            CreateCheckoutDetails(),
            CreateAddress(),
            TestContext.Current.CancellationToken);

        Assert.Equal("INVALID", response.Status);
        Assert.Equal("Shipping address is invalid", response.ValidationMessage);
        Assert.Contains("/v1/payments/payment", api.RequestPaths);
    }

    [Fact]
    public async Task ExecutePaymentAsync_maps_the_payer_and_sale_without_contacting_paypal()
    {
        await using var api = await PayPalApiStub.StartAsync();
        var service = CreateService(api.BaseUrl);

        var response = await service.ExecutePaymentAsync(
            "PAY-SYNTHETIC",
            "PAYER-SYNTHETIC",
            TestContext.Current.CancellationToken);

        Assert.Equal("SUCCESS", response.Status);
        Assert.Equal("buyer@example.test", response.EmailAddress);
        Assert.Equal("SALE-SYNTHETIC", response.SaleId);
        Assert.Equal("https://example.test/sales/SALE-SYNTHETIC", response.SaleUrl);
        Assert.Contains("/v1/payments/payment/PAY-SYNTHETIC/execute", api.RequestPaths);
    }

    private static PayPalService CreateService(string baseUrl)
    {
        var config = new Config
        {
            Paypal = new PaypalConfig
            {
                ApiClientId = "synthetic-client",
                ApiClientSecret = "synthetic-secret",
                SalesUrl = "https://example.test/sales/",
                PaymentCompleteUrl = "https://example.test/payment/complete",
                PaymentCancelledUrl = "https://example.test/payment/cancelled"
            }
        };
        return new TestPayPalService(new StubConfigRepository(config), baseUrl);
    }

    private static CheckoutDetails CreateCheckoutDetails()
    {
        var details = new CheckoutDetails(specialDelivery: false);
        details.AddItem(
            new BasketItemSummary { MerchId = "synthetic-item" },
            new MerchItem
            {
                Id = "synthetic-item",
                Name = "Synthetic print",
                Price = "14.00",
                SubCategory = "Prints"
            },
            new PostageProfile
            {
                UnitedKingdom = new PostageRegion { Alone = 3.50m, WithOthers = 1m }
            },
            "UK");
        return details;
    }

    private static PostageAddress CreateAddress()
    {
        return new PostageAddress
        {
            Name = "Synthetic Buyer",
            Address1 = "1 Example Street",
            Town = "Exampleton",
            CountryCode = "GB",
            PostalCode = "EX1 1EX",
            Region = "Example Region"
        };
    }

    private sealed class TestPayPalService : PayPalService
    {
        private readonly string _baseUrl;

        public TestPayPalService(IConfigAdminRepository config, string baseUrl) : base(config)
        {
            _baseUrl = baseUrl;
        }

        protected override PayPalHttpClient BuildClient(PaypalConfig config)
        {
            return new PayPalHttpClient(new PayPalEnvironment(
                config.ApiClientId,
                config.ApiClientSecret,
                _baseUrl,
                _baseUrl));
        }
    }

    private sealed class StubConfigRepository : IConfigAdminRepository
    {
        private Config _config;

        public StubConfigRepository(Config config)
        {
            _config = config;
        }

        public Task<Config> GetConfigAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_config);
        }

        public Task SetConfigAsync(Config config, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _config = config;
            return Task.CompletedTask;
        }
    }

    private sealed class PayPalApiStub : IAsyncDisposable
    {
        private readonly WebApplication _application;

        private PayPalApiStub(
            WebApplication application,
            string baseUrl,
            ConcurrentQueue<string> requestPaths)
        {
            _application = application;
            BaseUrl = baseUrl;
            RequestPaths = requestPaths;
        }

        public string BaseUrl { get; }

        public ConcurrentQueue<string> RequestPaths { get; }

        public string CreateRequestBody { get; private set; }

        public static async Task<PayPalApiStub> StartAsync(bool rejectCreate = false)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Logging.ClearProviders();
            builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
            var application = builder.Build();
            var requestPaths = new ConcurrentQueue<string>();
            PayPalApiStub stub = null;

            application.MapPost("/v1/oauth2/token", async context =>
            {
                requestPaths.Enqueue(context.Request.Path);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    """
                    {"scope":"payments","access_token":"synthetic-token","token_type":"Bearer","expires_in":3600}
                    """,
                    context.RequestAborted);
            });

            application.MapPost("/v1/payments/payment", async context =>
            {
                requestPaths.Enqueue(context.Request.Path);
                using var reader = new System.IO.StreamReader(context.Request.Body);
                stub.CreateRequestBody = await reader.ReadToEndAsync(context.RequestAborted);
                context.Response.ContentType = "application/json";

                if (rejectCreate)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.Headers["PayPal-Debug-Id"] = "synthetic-debug-id";
                    await context.Response.WriteAsync(
                        """
                        {"name":"VALIDATION_ERROR","details":[{"issue":"Shipping address is invalid"}]}
                        """,
                        context.RequestAborted);
                    return;
                }

                await context.Response.WriteAsync(
                    """
                    {
                      "id":"PAY-SYNTHETIC",
                      "intent":"sale",
                      "state":"created",
                      "create_time":"2026-11-06T12:00:00Z",
                      "payer":{"payment_method":"paypal"},
                      "transactions":[{
                        "amount":{"total":"17.50","currency":"GBP","details":{"subtotal":"14.00","tax":"0.00","shipping":"3.50"}},
                        "custom":"standard",
                        "item_list":{
                          "items":[{"name":"Synthetic print","quantity":"1","price":"14.00","tax":"0.00","currency":"GBP"}],
                          "shipping_address":{"recipient_name":"Synthetic Buyer","line1":"1 Example Street","city":"Exampleton","country_code":"GB","postal_code":"EX1 1EX","state":"Example Region"}
                        }
                      }],
                      "links":[{"href":"https://example.test/approve/PAY-SYNTHETIC","rel":"approval_url","method":"REDIRECT"}]
                    }
                    """,
                    context.RequestAborted);
            });

            application.MapPost("/v1/payments/payment/{paymentId}/execute", async context =>
            {
                requestPaths.Enqueue(context.Request.Path);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    """
                    {
                      "id":"PAY-SYNTHETIC",
                      "state":"approved",
                      "payer":{"payer_info":{"email":"buyer@example.test"}},
                      "transactions":[{"related_resources":[{"sale":{"id":"SALE-SYNTHETIC"}}]}]
                    }
                    """,
                    context.RequestAborted);
            });

            await application.StartAsync(TestContext.Current.CancellationToken);
            var address = application.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()
                .Addresses.Single()
                .Replace("127.0.0.1", "localhost", StringComparison.Ordinal);
            stub = new PayPalApiStub(application, address, requestPaths);
            return stub;
        }

        public async ValueTask DisposeAsync()
        {
            await _application.StopAsync();
            await _application.DisposeAsync();
        }
    }
}
