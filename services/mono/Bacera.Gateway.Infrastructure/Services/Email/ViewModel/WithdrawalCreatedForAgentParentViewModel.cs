namespace Bacera.Gateway.Services.Email.ViewModel;

public class WithdrawalCreatedForAgentParentViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.AgentWithdrawalCreatedForParent;
    public string AgentEmail { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}