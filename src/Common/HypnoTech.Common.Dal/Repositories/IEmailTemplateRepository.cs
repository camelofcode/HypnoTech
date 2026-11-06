using HypnoTech.Common.Models;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IEmailTemplateRepository
    {
        EmailTemplate GetEmailTemplate(string type);
    }
}
