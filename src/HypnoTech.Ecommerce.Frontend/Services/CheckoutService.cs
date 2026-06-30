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
        private readonly EmailConfig _emailConfig;

        public CheckoutService(IMerchRepository merchRepository, ICountryRepository countryRepository, IPostageProfileRepository postageProfileRepository, IPayPalService payPalService, IOrderRepository orderRepository, IEmailService emailService, IDownloadCodeRepository downloadCodeRepository, IConfigAdminRepository config)
        {
            _merchRepository = merchRepository;
            _countryRepository = countryRepository;
            _postageProfileRepository = postageProfileRepository;
            _payPalService = payPalService;
            _orderRepository = orderRepository;
            _emailService = emailService;
            _downloadCodeRepository = downloadCodeRepository;
            _emailConfig = config.GetConfig().Email;
        }

        public CheckoutViewModel GetCheckout()
        {
            return new CheckoutViewModel(_countryRepository.GetCountries());
        }

        public CheckoutDetails GetCheckoutDetails(CheckoutSummary summary)
        {
            var region = _countryRepository.GetCountryByCode(summary.CountryCode).Region;
            var merchItems = _merchRepository.GetMerchFlattened();

            CheckoutDetails details = new CheckoutDetails(summary.SpecialDelivery);

            foreach (var summaryItem in summary.Items)
            {
                var merchItem = merchItems.FirstOrDefault(p => p.Id == summaryItem.MerchId);
                if (merchItem != null)
                {
                    var postageProfile = _postageProfileRepository.GetProfile(merchItem.PostageProfile);
                    details.AddItem(summaryItem, merchItem, postageProfile, region);
                }
            }

            details.Items = details.Items.OrderBy(p => p.DisplayName).ToList();

            return details;
        }

        public PaypalCreateResponse Pay(CheckoutSummary summary, PostageAddress address)
        {
            var payPalResponse = _payPalService.CreatePayment(GetCheckoutDetails(summary), address);

            if (payPalResponse.Status == "SUCCESS")
            {
                _orderRepository.SaveOrder(payPalResponse.Order);
            }

            return payPalResponse;
        }

        public PaypalExecuteResponse Complete(string paymentId, string payerId)
        {
            var payPalResponse = _payPalService.ExecutePayment(paymentId, payerId);

            if (payPalResponse.Status == "SUCCESS")
            {
                payPalResponse.Order = _orderRepository.CompleteOrder(payPalResponse.ToPaymentCompletion());

                if (!string.IsNullOrEmpty(payPalResponse.Order.EmailAddress))
                {
                    var response = _downloadCodeRepository.AssignDownloadCodes(payPalResponse.Order);
                    _emailService.SendEmail(payPalResponse.Order.EmailAddress, "CustomerOrder", "Order", new CustomerOrderEmailViewModel(payPalResponse.Order, response.AssignedDownloadCodes));

                    if (response.ExpiredCollections.Any())
                    {
                        _emailService.SendEmail(_emailConfig.DefaultRecipient, "DownloadCodesDepleted", "DownloadCodesDepleted", new DownloadCodesDepletedEmailViewModel(response.ExpiredCollections));
                    }
                }

                _emailService.SendEmail(_emailConfig.DefaultRecipient, "AdminOrder", "Order", new OrderEmailViewModel(payPalResponse.Order));
            }


            return payPalResponse;
        }

        public void Cancel(string paymentId)
        {
            _orderRepository.CancelOrder(paymentId);
        }
    }
}
