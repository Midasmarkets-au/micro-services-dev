namespace Bacera.Gateway.Services;

public class DepositCompletedForClientViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.DepositCompletedForClient;
    public string Date { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public long AccountNumber { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public string PaymentNumber { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string FormattedAmount => $"{Amount:#,0.00}";
    public string NativeName { get; set; } = string.Empty;
}