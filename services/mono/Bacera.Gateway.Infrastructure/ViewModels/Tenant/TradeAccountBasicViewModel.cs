namespace Bacera.Gateway.ViewModels.Tenant;

public class TradeAccountBasicViewModel
{
    public long AccountNumber { get; set; }
    public CurrencyTypes CurrencyId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public double Balance { get; set; }
    public double Equity { get; set; }
    public double Credit { get; set; }
    public long BalanceInCents => (long)Math.Round(Balance * 100, 0);
    public long EquityInCents => (long)Math.Round(Equity * 100, 0);

    public long CreditInCents => (long)Math.Round(Credit * 100, 0);
    public int Leverage { get; set; }

    // public bool IsEmpty => AccountNumber == 0;
}