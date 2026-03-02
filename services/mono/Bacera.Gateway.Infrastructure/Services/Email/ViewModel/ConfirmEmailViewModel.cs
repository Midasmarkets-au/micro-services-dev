namespace Bacera.Gateway.Services;

public class ConfirmEmailViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.ConfirmEmail;
    public string UserName { get; }
    public string CallbackUrl { get; }

    public ConfirmEmailViewModel(string email, string userName, string callbackUrl)
    {
        Email = email;
        UserName = userName;
        CallbackUrl = callbackUrl;
    }
}