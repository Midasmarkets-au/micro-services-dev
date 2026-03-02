namespace Bacera.Gateway.DTO;

public class AccountApplicationConfig
{
    // tradingPlatformAvailable
    public List<TradeService.ClientTradingPlatformAvailableModel> TradingPlatformAvailable { get; set; } = null!;

    public List<AccountTypes> AccountTypeAvailable { get; set; } = null!;

    public List<CurrencyTypes> CurrencyAvailable { get; set; } = null!;

    public List<int> LeverageAvailable { get; set; } = null!;

    public string? ReferCode { get; set; }
}