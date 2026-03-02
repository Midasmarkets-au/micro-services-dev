namespace Bacera.Gateway.Services;

public class ResetPasswordViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.ResetPassword;
    public string UserName { get; }
    public string CallbackUrl { get; }

    public ResetPasswordViewModel(string email, string userName, string callbackUrl)
    {
        UserName = userName;
        Email = email;
        CallbackUrl = callbackUrl;
    }
}