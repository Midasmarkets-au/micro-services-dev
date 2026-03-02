namespace Bacera.Gateway.Services;

public class DepositCompletedForParentViewModel : DepositCompletedForClientViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.DepositCompletedForAgentAndSales;
}