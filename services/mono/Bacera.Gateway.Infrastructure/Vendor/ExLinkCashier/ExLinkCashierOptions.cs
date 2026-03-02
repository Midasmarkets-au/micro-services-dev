using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.ExLinkCashier;

public class ExLinkCashierOptions
{
    public string Uid { get; set; } = string.Empty;
    public string RequestUrl { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty; // VND, THB, IDR, INR, MXN, KRW, JPY, BRL, PHP
    public string SecretKey { get; set; } = string.Empty;
    public string CallbackSecretKey { get; set; } = string.Empty;
    
    // Withdrawal/Payout specific fields
    public string WithdrawalUrl { get; set; } = string.Empty; // Default: https://api.exlinked.global/coin/pay/withdraw/order/create
    public string PaymentType { get; set; } = "BankDirect"; // Default payment type for withdrawals

    // 以下参数收银台支付不需要， 创建代收订单才会需要
    //public string ChannelCode { get; set; } = string.Empty;
    //public string BankCode { get; set; } = string.Empty;
    //public int PaymentMethod { get; set; } // 1: JSON, 3: 平台收银

    public static ExLinkCashierOptions FromJson(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<ExLinkCashierOptions>(json) ?? new ExLinkCashierOptions();
        }
        catch
        {
            return new ExLinkCashierOptions();
        }
    }
}
