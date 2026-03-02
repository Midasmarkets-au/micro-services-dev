namespace Bacera.Gateway.Services;

public class WithdrawalConfirmationForClientViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.WithdrawalConfirmationForClient;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public long AccountNumber { get; set; }
    public string Currency { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string Date { get; set; } = string.Empty;
}