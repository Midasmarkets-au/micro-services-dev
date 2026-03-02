using Newtonsoft.Json;

namespace Bacera.Gateway.DTO;

public class TradeStatDTO
{
    public sealed class SymbolCurrencyDTO
    {
        public string Symbol { get; set; } = string.Empty;
        public CurrencyTypes CurrencyId { get; set; }
        public long Volume { get; set; }
        public long Profit { get; set; }
    }

    public sealed class TradeBySymbolAndCmd
    {
        // public long AccountNumber { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public int Cmd { get; set; }
        public decimal Volume { get; set; }
        public int Digits { get; set; }
        public decimal AveragePrice => Math.Round(AvePriceSum / Volume, Digits);
        [JsonIgnore] public decimal AvePriceSum { get; set; }

        // public double AveragePriceCurrent { get; set; }
        public double Profit { get; set; }
    }

    public sealed class MtPrice
    {
        public string Symbol { get; set; } = string.Empty;
        public double Bid { get; set; }
        public double Ask { get; set; }
    }


    public sealed class TenantAccountStat
    {
        public List<TradeBySymbolAndCmd> OpenTradeStats { get; set; } = [];
        public List<TradeBySymbolAndCmd> ClosedTradeStats { get; set; } = [];
        public List<TradeBySymbolAndCmd> OpenTrades { get; set; } = [];
        public List<MtPrice> OpenTradeCurrentPrices { get; set; } = [];
        public double Equity { get; set; }
        public double Balance { get; set; }
    }
}