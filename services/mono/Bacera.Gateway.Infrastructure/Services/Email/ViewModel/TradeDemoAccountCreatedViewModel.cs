namespace Bacera.Gateway.Services;

public class TradeDemoAccountCreatedViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.TradeDemoAccountCreated;
    public long AccountNumber { get; set; }
    public string Password { get; set; } = null!;
    public string NativeName { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
}