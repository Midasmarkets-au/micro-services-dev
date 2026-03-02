namespace Bacera.Gateway.Services;

public class WithdrawalConfirmationForParentViewModel : WithdrawalConfirmationForClientViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.WithdrawalConfirmationForAgentAndSales;
    public string NativeName { get; set; } = string.Empty;
}