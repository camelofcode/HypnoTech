using HypnoTech.Common.Emails;

namespace HypnoTech.Cms.Models
{
    public class PasswordResetEmailViewModel : BaseEmailViewModel
    {
        public string ResetUrl { get; }

        public string Username { get; }

        public PasswordResetEmailViewModel(string resetUrl, string username)
        {
            ResetUrl = resetUrl;
            Username = username;
        }

        public override void AfterApply()
        {
            IntroLine = IntroLine.Replace("#{Name}", Username);
        }
    }
}
