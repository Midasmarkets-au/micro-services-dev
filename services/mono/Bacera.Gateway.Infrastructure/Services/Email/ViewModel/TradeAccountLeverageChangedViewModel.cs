namespace Bacera.Gateway.Services;
public class TradeAccountLeverageChangedViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.TradeAccountLeverageChanged;
    public string NativeName { get; set; } = string.Empty;
    public long AccountNumber { get; set; }
    public int Leverage { get; set; }
    public int OriginalLeverage { get; set; }
}