namespace HypnoTech.Cms.Areas.Settings.ViewModels
{
    public class ChangePasswordViewModel
    {
        public string Id { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string Validate()
        {
            if (Password != ConfirmPassword)
            {
                return "Passwords do not match";
            }

            if (string.IsNullOrEmpty(Password) || Password.Length < 8)
            {
                return "Passwords does not meet length requirement";
            }

            return "";
        }
    }
}
