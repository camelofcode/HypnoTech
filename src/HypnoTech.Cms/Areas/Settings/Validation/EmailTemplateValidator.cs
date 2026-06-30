using HypnoTech.Common.Models;

namespace HypnoTech.Cms.Areas.Settings.Validation
{
    public static class EmailTemplateValidator
    {
        public static string Validate(this EmailTemplate template)
        {
            if (string.IsNullOrWhiteSpace(template.Subject))
            {
                return "Subject is required";
            }

            if (string.IsNullOrWhiteSpace(template.Header))
            {
                return "Header is required";
            }

            if (string.IsNullOrWhiteSpace(template.IntroLine))
            {
                return "Intro Line is required";
            }

            if (string.IsNullOrWhiteSpace(template.Body))
            {
                return "Body is required";
            }

            if (string.IsNullOrWhiteSpace(template.FooterText))
            {
                return "Footer is required";
            }

            return string.Empty;
        }
    }
}
