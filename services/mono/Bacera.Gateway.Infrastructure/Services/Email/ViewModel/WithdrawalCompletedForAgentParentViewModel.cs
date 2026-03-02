namespace Bacera.Gateway.Services.Email.ViewModel;

public class WithdrawalCompletedForAgentParentViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.AgentWithdrawalCompletedForParent;
    public string AgentEmail { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}