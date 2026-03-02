using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public class GetAccountInfoResult : BaseResponse
{
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("email")] public string Email { get; set; } = string.Empty;
    [JsonProperty("group")] public string Group { get; set; } = string.Empty;
    [JsonProperty("leverage")] public int Leverage { get; set; }
    // ReSharper disable once StringLiteralTypo
    [JsonProperty("regdate")] public string RegDate { get; set; } = string.Empty;
    [JsonProperty("country")] public string Country { get; set; } = string.Empty;
    [JsonProperty("state")] public string State { get; set; } = string.Empty;
    [JsonProperty("address")] public string Address { get; set; } = string.Empty;
    [JsonProperty("phone")] public string Phone { get; set; } = string.Empty;
    [JsonProperty("city")] public string City { get; set; } = string.Empty;
    [JsonProperty("zip")] public string Zip { get; set; } = string.Empty;
    [JsonProperty("enable")] public int Enable { get; set; }
    // ReSharper disable once StringLiteralTypo
    [JsonProperty("tradingblocked")] public int TradingBlocked { get; set; }
    [JsonProperty("balance")] public decimal Balance { get; set; }
    [JsonProperty("equity")] public decimal Equity { get; set; }
    [JsonProperty("credit")] public decimal Credit { get; set; }

    [JsonProperty("margin")] public decimal Margin { get; set; }
    [JsonProperty("margin_free")] public decimal MarginFree { get; set; }
    [JsonProperty("margin_level")] public decimal MarginLevel { get; set; }
    [JsonProperty("margin_leverage")] public decimal MarginLeverage { get; set; }
    [JsonProperty("profit")] public decimal Profit { get; set; }
    [JsonProperty("comment")] public string Comment { get; set; } = string.Empty;
    [JsonProperty("enableChangePassword")] public int EnableChangePassword { get; set; }
    [JsonProperty("free_margin")] public decimal FreeMargin { get; set; }
    [JsonProperty("opened_orders")] public string OpenedOrders { get; set; } = string.Empty;
    [JsonProperty("agent")] public long Agent { get; set; }
    // ReSharper disable once StringLiteralTypo
    [JsonProperty("lastdate")] public string LastDate { get; set; } = string.Empty;

    public void ApplyToTradeAccountStatus(TradeAccountStatus status)
    {
        status.Equity = (double)Equity;
        status.Margin = (double)Margin;
        status.MarginLevel = (double)MarginLevel;
        status.MarginFree = (double)MarginFree;
        status.Credit = (double)Credit;
        status.Balance = (double)Balance;
        status.Leverage = Leverage;
        status.Group = Group;
    }
}