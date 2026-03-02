namespace Bacera.Gateway.Services.Email.ViewModel;

public class EventShopOrderPlacedForSalesViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.EventShopOrderPlacedForSales;
    public string NativeName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemPoints { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string AccountList { get; set; } = null!;
}