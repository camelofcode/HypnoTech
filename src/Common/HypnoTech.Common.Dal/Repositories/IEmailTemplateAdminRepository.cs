using System.Collections.Generic;
using HypnoTech.Common.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IEmailTemplateAdminRepository
    {
        Task<List<EmailTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default);
        Task<EmailTemplate> GetTemplateForEditAsync(string id, CancellationToken cancellationToken = default);
        Task UpdateTemplateAsync(EmailTemplate profile, CancellationToken cancellationToken = default);
    }
}
