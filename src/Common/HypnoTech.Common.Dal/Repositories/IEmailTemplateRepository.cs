using HypnoTech.Common.Models;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IEmailTemplateRepository
    {
        Task<EmailTemplate> GetEmailTemplateAsync(string type, CancellationToken cancellationToken = default);
    }
}
