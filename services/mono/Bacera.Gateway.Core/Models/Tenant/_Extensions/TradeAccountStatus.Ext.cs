namespace Bacera.Gateway;

partial class TradeAccountStatus : IEntity
{
    public long BalanceInCents => Balance.ToAmountInCents();
    public long EquityInCents => Equity.ToAmountInCents();
}