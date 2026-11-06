using System.Collections.Generic;
using HypnoTech.Common.Models;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IEmailTemplateAdminRepository
    {
        List<EmailTemplate> GetTemplates();

        EmailTemplate GetTemplateForEdit(string id);

        void UpdateTemplate(EmailTemplate profile);
    }
}
