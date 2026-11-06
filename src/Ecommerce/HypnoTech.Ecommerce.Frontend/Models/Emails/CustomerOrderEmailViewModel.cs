using System.Collections.Generic;
using System.Linq;
using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Ecommerce.Frontend.Models.Emails
{
    public class CustomerOrderEmailViewModel : OrderEmailViewModel
    {
        public List<AssignedDownloadCode> DownloadCodes { get; }

        public bool HasDownloadCodes => DownloadCodes != null && DownloadCodes.Any();

        public CustomerOrderEmailViewModel(Order order, List<AssignedDownloadCode> downloadCodes) : base(order)
        {
            DownloadCodes = downloadCodes;
        }

        public override void AfterApply()
        {
            IntroLine = IntroLine.Replace("#{Name}", Order.Address.Name);
        }
    }
}
