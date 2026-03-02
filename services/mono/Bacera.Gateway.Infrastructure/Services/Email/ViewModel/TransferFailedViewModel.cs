namespace Bacera.Gateway.Services.Email.ViewModel;

public class TransferFailedViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.TransferFailed;
    public string Currency { get; set; } = string.Empty;
    public string FormattedAmount { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public long SourceAccountNumber { get; set; }
    public long TargetAccountNumber { get; set; }
}