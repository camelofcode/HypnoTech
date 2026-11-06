namespace HypnoTech.Common.Emails
{
    public interface IEmailService
    {
        void SendEmail(string to, string templateName, string fileName, BaseEmailViewModel model);
    }
}
