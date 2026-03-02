namespace Bacera.Gateway.ViewModels.Tenant;

public class WalletStatisticViewModel
{
    public long Balance { get; set; }
    public CurrencyTypes CurrencyId { get; set; }
    public FundTypes FundType { get; set; }
}