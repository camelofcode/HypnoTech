using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Cms.Areas.Settings.ViewModels
{
    public class EditUserViewModel : AddUserViewModel
    {
        public string Id { get; set; }

        public EditUserViewModel(AdminUser user)
        {
            Id = user.Id;
            Email = user.Email;
            Username = user.Username;
            Roles = user.Roles;
        }

        public EditUserViewModel()
        {
            
        }

        public new string Validate()
        {
            if (string.IsNullOrEmpty(Username))
            {
                return "Username is required";
            }

            if (string.IsNullOrEmpty(Email))
            {
                return "Email is required";
            }

            if (Roles.Count < 1)
            {
                return "User must have at least one role";
            }

            return "";
        }

        public new AdminUser ToAdminUser()
        {
            return new AdminUser
            {
                Id = Id,
                Email = Email,
                Username = Username,
                Roles = Roles
            };
        }
    }
}
