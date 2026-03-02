namespace Bacera.Gateway.Services.Email.ViewModel;

public class TransferCreatedForIBandSalesViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.TransferCreatedForIBandSales;
    public string NativeName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string FormattedAmount { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public long SourceAccountNumber { get; set; }
    public long TargetAccountNumber { get; set; }
}