namespace Bacera.Gateway.Services.Email.ViewModel;

public class TradeAccountCreatedForIBandSalesViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.TradeAccountCreatedForIBandSales;
    public string UserName { get; set; } = string.Empty;
    public long AccountNumber { get; set; }
    public string TradeServiceName { get; set; } = string.Empty;
    public string ReadOnlyPassword { get; set; } = string.Empty;
}