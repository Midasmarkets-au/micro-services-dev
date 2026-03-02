namespace Bacera.Gateway.Services.Email.ViewModel;

public class WithdrawalFailedForAgentSelfViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.AgentWithdrawalFailed;
    public string Currency { get; set; } = string.Empty;
    public string FormattedAmount { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}