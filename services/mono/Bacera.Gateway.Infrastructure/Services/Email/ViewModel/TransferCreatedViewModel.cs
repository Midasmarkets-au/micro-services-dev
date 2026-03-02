namespace Bacera.Gateway.Services.Email.ViewModel;

public class TransferCreatedViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.TransferCreated;
    public long SourceAccountNumber { get; set; }
    public long TargetAccountNumber { get; set; }
    public DateTime Date { get; set; }
    public string FormattedAmount { get; set; } = string.Empty;
}