namespace Bacera.Gateway.Services.Email.ViewModel;

public class RegisteredWelcomeViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.RegisteredWelcome;
    public string Password { get; set; } = string.Empty;
}