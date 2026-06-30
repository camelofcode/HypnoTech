using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using HandlebarsDotNet;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Common.Emails
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfig _config;
        private readonly MainSiteConfig _site;
        private readonly IEmailTemplateRepository _emailTemplateRepository;

        public EmailService(IConfigAdminRepository repository, IEmailTemplateRepository emailTemplateRepository)
        {
            _emailTemplateRepository = emailTemplateRepository;
            var config = repository.GetConfig();
            _config = config.Email;
            _site = config.MainSite;

            RegisterPartials();
        }

        public void SendEmail(string to, string templateName, string filename, BaseEmailViewModel model)
        {
            var template = _emailTemplateRepository.GetEmailTemplate(templateName);
            model.ApplyTemplate(_site.Url, template);
            SendEmail(to, template.Subject, BuildEmail(model, filename));
        }

        private void SendEmail(string to, string subject, string body)
        {
            if (!_config.EnableEmails)
            {
                WriteToFolder(subject, body);
                return;
            }

            SmtpClient client = new SmtpClient(_config.SmtpServer)
            {
                Credentials = new NetworkCredential(_config.MailboxUser, _config.MailboxPassword)
            };

            MailMessage message = new MailMessage(_config.Sender, to)
            {
                Subject = subject,
                IsBodyHtml = true,
                Body = body
            };

            client.Send(message);
        }

        private void WriteToFolder(string subject, string body)
        {
            Directory.CreateDirectory("TestEmails");
            File.WriteAllText($@"TestEmails/{DateTime.Now:dd MM yyyy - hh-mm}-{subject}.html", body);
        }

        private string BuildEmail(BaseEmailViewModel model, string name)
        {
            var render = Handlebars.Compile(File.ReadAllText($@"EmailTemplates/{name}.hbs"));
            return render(model);
        }

        private void RegisterPartials()
        {
            RegisterPartial("Footer");
            RegisterPartial("Head");
            RegisterPartial("Header");
            RegisterPartial("OrderDetails");
            RegisterPartial("PasswordResetLink");
            RegisterPartial("Preamble");
            RegisterPartial("DownloadCodes");
        }

        private void RegisterPartial(string name)
        {
            var path = $@"EmailTemplates/Partials/{name}.hbs";
            if (File.Exists(path))
            {
                Handlebars.RegisterTemplate(name, File.ReadAllText($@"EmailTemplates/Partials/{name}.hbs"));
            }
        }
    }
}
