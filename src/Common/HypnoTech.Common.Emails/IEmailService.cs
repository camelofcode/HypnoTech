using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Emails
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string templateName, string fileName, BaseEmailViewModel model, CancellationToken cancellationToken = default);
    }
}
