namespace HypnoTech.Common.Models.Settings
{
    public class EmailConfig
    {
        public bool EnableEmails { get; set; }

        public string SmtpServer { get; set; }

        public string MailboxUser { get; set; }

        public string MailboxPassword { get; set; }

        public string Sender { get; set; }

        public string DefaultRecipient { get; set; }
    }
}
