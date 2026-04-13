namespace Bacera.Gateway.Services.Email.ViewModel;

public class DepositReceiptUploadedForIBandSalesViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.DepositReceiptUploadedForIBandSales;
    public long AccountNumber { get; set; }
    public string Group { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string FormattedAmount { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
