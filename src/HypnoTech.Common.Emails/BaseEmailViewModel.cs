using HypnoTech.Common.Models;

namespace HypnoTech.Common.Emails
{
    public abstract class BaseEmailViewModel
    {
        private string _baseSitePath;

        public string HeaderBackgroundImage => $"{_baseSitePath}img/emails/header-bg.jpg";

        public string LogoImage => $"{_baseSitePath}img/emails/logo.png";

        public string FooterBackgroundImage => $"{_baseSitePath}img/emails/footer-bg.jpg";

        public string Title { get; set; }

        public string Header { get; set; }

        public string IntroLine { get; set; }

        public string Body { get; set; }

        public string FooterText { get; set; }

        public abstract void AfterApply();

        public void ApplyTemplate(string baseSitePath, EmailTemplate template)
        {
            _baseSitePath = baseSitePath;
            Header = template.Header;
            IntroLine = template.IntroLine;
            Body = template.Body;
            FooterText = template.FooterText;
            Title = template.Subject;

            AfterApply();
        }
    }
}
