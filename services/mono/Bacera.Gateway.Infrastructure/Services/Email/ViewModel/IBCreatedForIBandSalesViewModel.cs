namespace Bacera.Gateway.Services.Email.ViewModel;

public class IBCreatedForIBandSalesViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.IBCreatedForIBandSales;
    public long AccountUid { get; set; }
    public string IbEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string IbCode { get; set; } = string.Empty;
}