using System;
using System.Collections.Generic;
using System.Text;
using HypnoTech.Common.Models.Settings;

namespace HypnoTech.Cms.Mvc.ViewModels
{
    public class ChangeAccountDetailsViewModel
    {
        public string Id { get; set; }

        public string Password { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

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

            return "";
        }

        public AdminUser ToAdminUser()
        {
            return new AdminUser
            {
                Id = Id,
                Email = Email,
                Username = Username
            };
        }
    }
}
