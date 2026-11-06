using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using HandlebarsDotNet;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Common.Emails
{
    public class EmailService : IEmailService
    {
        private readonly IConfigAdminRepository _configRepository;
        private readonly IEmailTemplateRepository _emailTemplateRepository;

        public EmailService(IConfigAdminRepository repository, IEmailTemplateRepository emailTemplateRepository)
        {
            _configRepository = repository;
            _emailTemplateRepository = emailTemplateRepository;
        }

        public void SendEmail(string to, string templateName, string filename, BaseEmailViewModel model)
        {
            var config = _configRepository.GetConfig();
            var template = _emailTemplateRepository.GetEmailTemplate(templateName);
            RegisterPartials();
            model.ApplyTemplate(config.MainSite.Url, template);
            SendEmail(config.Email, to, template.Subject, BuildEmail(model, filename));
        }

        public async Task SendEmailAsync(
            string to,
            string templateName,
            string filename,
            BaseEmailViewModel model,
            CancellationToken cancellationToken = default)
        {
            var config = await _configRepository.GetConfigAsync(cancellationToken);
            var template = await _emailTemplateRepository.GetEmailTemplateAsync(
                templateName,
                cancellationToken);
            await RegisterPartialsAsync(cancellationToken);
            model.ApplyTemplate(config.MainSite.Url, template);
            await SendEmailAsync(
                config.Email,
                to,
                template.Subject,
                await BuildEmailAsync(model, filename, cancellationToken),
                cancellationToken);
        }

        private void SendEmail(EmailConfig config, string to, string subject, string body)
        {
            if (!config.EnableEmails)
            {
                WriteToFolder(subject, body);
                return;
            }

            using SmtpClient client = new SmtpClient(config.SmtpServer)
            {
                Credentials = new NetworkCredential(config.MailboxUser, config.MailboxPassword)
            };

            using MailMessage message = new MailMessage(config.Sender, to)
            {
                Subject = subject,
                IsBodyHtml = true,
                Body = body
            };

            client.Send(message);
        }

        private async Task SendEmailAsync(
            EmailConfig config,
            string to,
            string subject,
            string body,
            CancellationToken cancellationToken)
        {
            if (!config.EnableEmails)
            {
                await WriteToFolderAsync(subject, body, cancellationToken);
                return;
            }

            using SmtpClient client = new SmtpClient(config.SmtpServer)
            {
                Credentials = new NetworkCredential(config.MailboxUser, config.MailboxPassword)
            };
            using MailMessage message = new MailMessage(config.Sender, to)
            {
                Subject = subject,
                IsBodyHtml = true,
                Body = body
            };

            await client.SendMailAsync(message, cancellationToken);
        }

        private void WriteToFolder(string subject, string body)
        {
            Directory.CreateDirectory("TestEmails");
            File.WriteAllText($@"TestEmails/{DateTime.Now:dd MM yyyy - hh-mm}-{subject}.html", body);
        }

        private async Task WriteToFolderAsync(string subject, string body, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory("TestEmails");
            await File.WriteAllTextAsync(
                $@"TestEmails/{DateTime.Now:dd MM yyyy - hh-mm}-{subject}.html",
                body,
                cancellationToken);
        }

        private string BuildEmail(BaseEmailViewModel model, string name)
        {
            var render = Handlebars.Compile(File.ReadAllText($@"EmailTemplates/{name}.hbs"));
            return render(model);
        }

        private async Task<string> BuildEmailAsync(
            BaseEmailViewModel model,
            string name,
            CancellationToken cancellationToken)
        {
            var source = await File.ReadAllTextAsync(
                $@"EmailTemplates/{name}.hbs",
                cancellationToken);
            var render = Handlebars.Compile(source);
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

        private async Task RegisterPartialsAsync(CancellationToken cancellationToken)
        {
            foreach (var name in new[]
                     {
                         "Footer", "Head", "Header", "OrderDetails", "PasswordResetLink",
                         "Preamble", "DownloadCodes"
                     })
            {
                var path = $@"EmailTemplates/Partials/{name}.hbs";
                if (File.Exists(path))
                {
                    Handlebars.RegisterTemplate(
                        name,
                        await File.ReadAllTextAsync(path, cancellationToken));
                }
            }
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
