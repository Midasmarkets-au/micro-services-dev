namespace Bacera.Gateway.Services.Email.ViewModel;

public class PuTradeAccountCreatedViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.PuTradeAccountCreated;
    public string UserName { get; set; } = string.Empty;
    public long AccountNumber { get; set; }
    public string Password { get; set; } = string.Empty;
    public string InvestorPassword { get; set; } = string.Empty;
    public string PhonePassword { get; set; } = string.Empty;
    public string TradeServiceName { get; set; } = string.Empty;
}