using System.Collections.Generic;
using HypnoTech.Common.Emails;

namespace HypnoTech.Ecommerce.Frontend.Models.Emails
{
    public class DownloadCodesDepletedEmailViewModel : BaseEmailViewModel
    {
        public List<string> Codes { get; }

        public DownloadCodesDepletedEmailViewModel(List<string> codes)
        {
            Codes = codes;
        }

        public override void AfterApply()
        {
            //Nothing to do
        }
    }
}
