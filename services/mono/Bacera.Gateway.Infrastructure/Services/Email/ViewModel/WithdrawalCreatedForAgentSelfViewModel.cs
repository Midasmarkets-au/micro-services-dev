namespace Bacera.Gateway.Services.Email.ViewModel;

public class WithdrawalCreatedForAgentSelfViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.AgentWithdrawalCreated;
    public string Currency { get; set; } = string.Empty;
    public string FormattedAmount { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}