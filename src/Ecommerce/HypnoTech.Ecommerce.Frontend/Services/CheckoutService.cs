using System.Linq;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Emails;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Frontend.Models;
using HypnoTech.Ecommerce.Frontend.Models.Emails;
using HypnoTech.Ecommerce.Frontend.Services.Interfaces;
using HypnoTech.Ecommerce.Models;
using HypnoTech.Ecommerce.Models.Basket;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Frontend.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IMerchRepository _merchRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IPostageProfileRepository _postageProfileRepository;
        private readonly IPayPalService _payPalService;
        private readonly IOrderRepository _orderRepository;
        private readonly IEmailService _emailService;
        private readonly IDownloadCodeRepository _downloadCodeRepository;
        private readonly IConfigAdminRepository _config;

        public CheckoutService(IMerchRepository merchRepository, ICountryRepository countryRepository, IPostageProfileRepository postageProfileRepository, IPayPalService payPalService, IOrderRepository orderRepository, IEmailService emailService, IDownloadCodeRepository downloadCodeRepository, IConfigAdminRepository config)
        {
            _merchRepository = merchRepository;
            _countryRepository = countryRepository;
            _postageProfileRepository = postageProfileRepository;
            _payPalService = payPalService;
            _orderRepository = orderRepository;
            _emailService = emailService;
            _downloadCodeRepository = downloadCodeRepository;
            _config = config;
        }

        public async Task<CheckoutViewModel> GetCheckoutAsync(CancellationToken cancellationToken = default)
        {
            return new CheckoutViewModel(await _countryRepository.GetCountriesAsync(cancellationToken));
        }

        public async Task<CheckoutDetails> GetCheckoutDetailsAsync(
            CheckoutSummary summary,
            CancellationToken cancellationToken = default)
        {
            var region = (await _countryRepository.GetCountryByCodeAsync(
                summary.CountryCode,
                cancellationToken)).Region;
            var merchItems = await _merchRepository.GetMerchFlattenedAsync(cancellationToken);
            CheckoutDetails details = new CheckoutDetails(summary.SpecialDelivery);

            foreach (var summaryItem in summary.Items)
            {
                var merchItem = merchItems.FirstOrDefault(p => p.Id == summaryItem.MerchId);
                if (merchItem != null)
                {
                    var postageProfile = await _postageProfileRepository.GetProfileAsync(
                        merchItem.PostageProfile,
                        cancellationToken);
                    details.AddItem(summaryItem, merchItem, postageProfile, region);
                }
            }

            details.Items = details.Items.OrderBy(p => p.DisplayName).ToList();
            return details;
        }

        public async Task<PaypalCreateResponse> PayAsync(
            CheckoutSummary summary,
            PostageAddress address,
            CancellationToken cancellationToken = default)
        {
            var details = await GetCheckoutDetailsAsync(summary, cancellationToken);
            var payPalResponse = await _payPalService.CreatePaymentAsync(
                details,
                address,
                cancellationToken);

            if (payPalResponse.Status == "SUCCESS")
            {
                await _orderRepository.SaveOrderAsync(payPalResponse.Order, cancellationToken);
            }

            return payPalResponse;
        }

        public async Task<PaypalExecuteResponse> CompleteAsync(
            string paymentId,
            string payerId,
            CancellationToken cancellationToken = default)
        {
            var emailConfig = (await _config.GetConfigAsync(cancellationToken)).Email;
            var payPalResponse = await _payPalService.ExecutePaymentAsync(
                paymentId,
                payerId,
                cancellationToken);

            if (payPalResponse.Status == "SUCCESS")
            {
                payPalResponse.Order = await _orderRepository.CompleteOrderAsync(
                    payPalResponse.ToPaymentCompletion(),
                    cancellationToken);

                if (!string.IsNullOrEmpty(payPalResponse.Order.EmailAddress))
                {
                    var response = await _downloadCodeRepository.AssignDownloadCodesAsync(
                        payPalResponse.Order,
                        cancellationToken);
                    await _emailService.SendEmailAsync(
                        payPalResponse.Order.EmailAddress,
                        "CustomerOrder",
                        "Order",
                        new CustomerOrderEmailViewModel(
                            payPalResponse.Order,
                            response.AssignedDownloadCodes),
                        cancellationToken);

                    if (response.ExpiredCollections.Any())
                    {
                        await _emailService.SendEmailAsync(
                            emailConfig.DefaultRecipient,
                            "DownloadCodesDepleted",
                            "DownloadCodesDepleted",
                            new DownloadCodesDepletedEmailViewModel(response.ExpiredCollections),
                            cancellationToken);
                    }
                }

                await _emailService.SendEmailAsync(
                    emailConfig.DefaultRecipient,
                    "AdminOrder",
                    "Order",
                    new OrderEmailViewModel(payPalResponse.Order),
                    cancellationToken);
            }

            return payPalResponse;
        }

        public Task CancelAsync(string paymentId, CancellationToken cancellationToken = default)
        {
            return _orderRepository.CancelOrderAsync(paymentId, cancellationToken);
        }
    }
}
