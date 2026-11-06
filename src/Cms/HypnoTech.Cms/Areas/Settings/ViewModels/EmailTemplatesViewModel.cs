using System.Collections.Generic;
using HypnoTech.Common.Models;

namespace HypnoTech.Cms.Areas.Settings.ViewModels
{
    public class EmailTemplatesViewModel
    {
        public List<EmailTemplate> Templates { get;}

        public EmailTemplatesViewModel(List<EmailTemplate> templates)
        {
            Templates = templates;
        }
    }
}
