namespace Bacera.Gateway.Services.Email.ViewModel;

public class TransferCompletedViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.TransferCompleted;
    public long SourceAccountNumber { get; set; }
    public long TargetAccountNumber { get; set; }
    public string FormattedAmount { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Currency { get; set; } = string.Empty;
}