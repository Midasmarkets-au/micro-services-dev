namespace Bacera.Gateway.Services;

public class WithdrawalCancelledForAgentAndSalesViewModel : WithdrawalCancelledForClientViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.WithdrawalCancelledForAgentAndSales;
}