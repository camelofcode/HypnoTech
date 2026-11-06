using System.Collections.Generic;
using HypnoTech.Common.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IEmailTemplateAdminRepository
    {
        List<EmailTemplate> GetTemplates();
        Task<List<EmailTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default);

        EmailTemplate GetTemplateForEdit(string id);
        Task<EmailTemplate> GetTemplateForEditAsync(string id, CancellationToken cancellationToken = default);

        void UpdateTemplate(EmailTemplate profile);
        Task UpdateTemplateAsync(EmailTemplate profile, CancellationToken cancellationToken = default);
    }
}
