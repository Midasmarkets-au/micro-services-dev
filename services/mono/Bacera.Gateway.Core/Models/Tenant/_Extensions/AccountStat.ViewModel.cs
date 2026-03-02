using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = AccountStat;

public partial class AccountStat
{
    public sealed class ParentModel
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public long NewAccountCount { get; set; }
        public long NewAgentCount { get; set; }
        public long DepositAmount { get; set; }
        public long DepositCount { get; set; }
        public long WithdrawAmount { get; set; }
        public long WithdrawCount { get; set; }
        public long TradeVolume { get; set; }
        public List<SymbolStat> TradeBySymbol { get; set; } = new();
        public long RebateAmount { get; set; }
        public long RebateCount { get; set; }
        public long TradeProfit { get; set; }
        public long TradeCount { get; set; }
    }

    public sealed class SymbolStat
    {
        public string Symbol { get; set; } = "";
        public long TotalTrade { get; set; }
        public long TotalProfit { get; set; }
        public long TotalVolume { get; set; }
    }

    public sealed class SymbolStatModel
    {
        [JsonPropertyName("total_trades")]
        [JsonProperty("total_trades")]
        public long TotalTrades { get; set; }

        [JsonPropertyName("total_profit")]
        [JsonProperty("total_profit")]
        public long TotalProfit { get; set; }

        [JsonPropertyName("total_volume")]
        [JsonProperty("total_volume")]
        public long TotalVolume { get; set; }

        public static SymbolStatModel Build() => new();

        public static Dictionary<string, SymbolStatModel> FromJson(string json) =>
            Utils.JsonDeserializeObjectWithDefault<Dictionary<string, SymbolStatModel>>(json);
    }
}

public static class AccountStatViewModelExt
{
    public static IQueryable<M.ParentModel> ToGroupedParentModel(this IQueryable<M> query)
    {
        return query.GroupBy(x => x.AccountId)
            .Select(x => new M.ParentModel
            {
                NewAccountCount = x.Sum(y => y.NewAccountCount),
                NewAgentCount = x.Sum(y => y.NewAgentCount),
                DepositAmount = x.Sum(y => y.DepositAmount),
                DepositCount = x.Sum(y => y.DepositCount),
                WithdrawAmount = x.Sum(y => y.WithdrawAmount),
                WithdrawCount = x.Sum(y => y.WithdrawCount),
                TradeVolume = x.Sum(y => y.TradeVolume),
                RebateAmount = x.Sum(y => y.RebateAmount),
                RebateCount = x.Sum(y => y.RebateCount),
                TradeProfit = x.Sum(y => y.TradeProfit),
                TradeCount = x.Sum(y => y.TradeCount),
            });
    }
}