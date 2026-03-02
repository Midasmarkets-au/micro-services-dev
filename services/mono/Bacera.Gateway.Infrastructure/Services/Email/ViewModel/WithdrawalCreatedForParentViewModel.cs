namespace Bacera.Gateway.Services;

public class WithdrawalCreatedForParentViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.WithdrawalCreatedForParent;
    public string PaymentMethod { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string PaymentNumber { get; set; } = string.Empty;
    public long AccountNumber { get; set; }
    public string Currency { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string Date { get; set; } = string.Empty;
}