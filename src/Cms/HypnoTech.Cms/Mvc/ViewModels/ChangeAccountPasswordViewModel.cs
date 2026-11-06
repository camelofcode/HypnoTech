using System;
using System.Collections.Generic;
using System.Text;

namespace HypnoTech.Cms.Mvc.ViewModels
{
    public class ChangeAccountPasswordViewModel
    {
        public string Id { get; set; }

        public string Password { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmNewPassword { get; set; }

        public string Validate()
        {
            if (NewPassword != ConfirmNewPassword)
            {
                return "Passwords do not match";
            }

            if (string.IsNullOrEmpty(NewPassword) || NewPassword.Length < 8)
            {
                return "Passwords does not meet length requirement";
            }

            return "";
        }
    }
}
