using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public class GetAccountBalanceInfoResult : BaseResponse
{
    public decimal Balance { get; set; }
    public decimal Credit { get; set; }
    public decimal Equity { get; set; }

    public decimal Margin { get; set; }
    public decimal MarginFree { get; set; }
    public decimal MarginLevel { get; set; }
    public decimal MarginLeverage { get; set; }
    public decimal Profit { get; set; }
}