namespace Bacera.Gateway.Services.Email.ViewModel;

public class WithdrawalCreatedForClientViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.WithdrawalApplication;
    public long AccountNumber { get; set; }
    public string NativeName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string FormattedAmount { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}