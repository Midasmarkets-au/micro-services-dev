namespace Bacera.Gateway.Services;

public class WithdrawalRejectedForClientViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.WithdrawalRejectedForClient;
    public long AccountNumber { get; set; }
    public string NativeName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string FormattedAmount => $"{Amount:#,0.00}";
    public string Reason { get; set; } = string.Empty;
}