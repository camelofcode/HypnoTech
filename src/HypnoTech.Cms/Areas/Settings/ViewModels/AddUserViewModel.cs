using System.Collections.Generic;
using HypnoTech.Common;
using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Cms.Areas.Settings.ViewModels
{
    public class AddUserViewModel
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public List<string> Roles { get; set; }

        public List<string> AvailableRoles { get; set; }

        public AddUserViewModel()
        {
            Roles = new List<string>();
        }

        public string Validate()
        {
            if (string.IsNullOrEmpty(Username))
            {
                return "Username is required";
            }

            if (string.IsNullOrEmpty(Email))
            {
                return "Email is required";
            }

            if (Password != ConfirmPassword)
            {
                return "Passwords do not match";
            }

            if (string.IsNullOrEmpty(Password) || Password.Length < 8)
            {
                return "Passwords does not meet length requirement";
            }

            if (Roles.Count < 1)
            {
                return "User must have at least one role";
            }

            return "";
        }

        public AdminUser ToAdminUser()
        {
            return new AdminUser
            {
                Email = Email,
                Username = Username,
                Password = PasswordHelper.HashPassword(Password),
                Roles = Roles
            };
        }
    }
}
