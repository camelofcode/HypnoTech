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

        private async Task WriteToFolderAsync(string subject, string body, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory("TestEmails");
            await File.WriteAllTextAsync(
                $@"TestEmails/{DateTime.Now:dd MM yyyy - hh-mm}-{subject}.html",
                body,
                cancellationToken);
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

    }
}
