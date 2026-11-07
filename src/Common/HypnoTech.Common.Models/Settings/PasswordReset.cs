using System;
using System.Security.Cryptography;
using System.Text;

namespace HypnoTech.Common.Models.Settings
{
    public class PasswordReset
    {
        public string Email { get; set; }

        public string Username { get; set; }

        public string Code { get; set; }

        public DateTime Created { get; set; }

        public DateTime Expiry => Created.AddMinutes(30);

        public bool Valid => DateTime.UtcNow < Expiry;

        public PasswordReset(AdminUser user)
        {
            Email = user.Email;
            Username = user.Username;
            GenerateCode();
            Created = DateTime.UtcNow;
        }

        private void GenerateCode()
        {
            var length = 40;
            const string valid = "jFVMApBr1agYUvGqxcCoZ8X5Wk9HPeud6TRIQbwSsiD4JmO70NLlEntyz3Kh2f";
            StringBuilder res = new StringBuilder();
            while (length-- > 0)
            {
                res.Append(valid[RandomNumberGenerator.GetInt32(valid.Length)]);
            }

            Code = res.ToString();
        }
    }
}
