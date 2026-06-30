namespace HypnoTech.Cms.Services.Interfaces
{
    public interface IForgottenPasswordService
    {
        bool CreatePasswordResetIfUserExists(string nameOrEmail);
        bool CheckCode(string code);
        void ResetPassword(string code, string password);
    }
}
