namespace HypnoTech.Common
{
    public static class PasswordHelper
    {
        private const int Cost = 12;

        public static string HashPassword(string plain) => BCrypt.Net.BCrypt.HashPassword(plain, workFactor: Cost);

        public static bool VerifyPassword(string plain, string hash) => BCrypt.Net.BCrypt.Verify(plain, hash);
    }
}
