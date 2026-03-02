namespace Bacera.Gateway.Services;

public class TradeAccountCreatedViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.TradeAccountCreated;
    public DateTime OpenedDate { get; set; } = DateTime.UtcNow;
    public string TradeServiceName { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public long AccountNumber { get; set; }
    public string Password { get; set; } = string.Empty;
    public string InvestorPassword { get; set; } = string.Empty;
    public string PhonePassword { get; set; } = string.Empty;
}