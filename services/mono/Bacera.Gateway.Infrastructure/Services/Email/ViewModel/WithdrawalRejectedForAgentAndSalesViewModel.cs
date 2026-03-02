namespace Bacera.Gateway.Services;

public class WithdrawalRejectedForAgentAndSalesViewModel : WithdrawalRejectedForClientViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.WithdrawalRejectedForAgentAndSales;
}