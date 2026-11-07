using System.ComponentModel;

namespace HypnoTech.Common.Models
{
    public class EmailTemplate
    {
        public string Id { get; set; }

        public EmailTypes EmailType { get; set; }

        public string Subject { get; set; }

        public string Header { get; set; }

        public string IntroLine { get; set; }

        public string Body { get; set; }

        public string FooterText { get; set; }
    }

    public enum EmailTypes
    {
        [Description("Sent to customer when they place an order")]
        CustomerOrder,
        [Description("Sent to store owner when customer places an order")]
        AdminOrder,
        [Description("Sent to customer when their order is marked as shipped")]
        OrderShipped,
        [Description("Sent when a password reset for admin site is requested")]
        PasswordReset,
        [Description("Sent to store owner when a set of download codes have been depleted")]
        DownloadCodesDepleted
    }
}
