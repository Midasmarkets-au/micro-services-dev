using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Long77Pay;

public class Long77PayUsdtCallbackViewModel
{
    private Long77PayUsdtOptions _options = new();

    public Long77PayUsdtCallbackViewModel ApplyOptions(Long77PayUsdtOptions options)
    {
        _options = options;
        return this;
    }

    [JsonProperty("partner_id")] public string PartnerId { get; set; } = string.Empty;

    [JsonProperty("system_order_code")] public string SystemOrderCode { get; set; } = string.Empty;

    [JsonProperty("partner_order_code")] public string PartnerOrderCode { get; set; } = string.Empty;
    public string PartnerOrderCodeFormatted => PartnerOrderCode.Replace("_", "-");

    [JsonProperty("guest_id")] public string GuestId { get; set; } = string.Empty;

    [JsonProperty("amount")] public string Amount { get; set; } = string.Empty;
    [JsonProperty("amount_cny")] public string AmountCny { get; set; } = string.Empty;
    [JsonProperty("otc_usdt_cny")] public string OtcUsdtCny { get; set; } = string.Empty;

    [JsonProperty("request_time")] public string RequestTime { get; set; } = string.Empty;

    [JsonProperty("extra_data")] public string ExtraData { get; set; } = string.Empty;

    [JsonProperty("payment")] public PaymentViewModel Payment { get; set; } = new();

    [JsonProperty("sign")] public string Sign { get; set; } = string.Empty;

    public class PaymentViewModel
    {
        [JsonProperty("txid")] public string TxId { get; set; } = string.Empty;

        [JsonProperty("paid_amount")] public string PaidAmount { get; set; } = string.Empty;

        [JsonProperty("fees")] public int? Fees { get; set; }

        [JsonProperty("payment_time")] public int? PaymentTime { get; set; }

        [JsonProperty("callback_time")] public int? CallbackTime { get; set; }
        [JsonProperty("erc20address")] public int? Erc20Address { get; set; }
        [JsonProperty("trc20address")] public int? Trc20Address { get; set; }

        [JsonProperty("status")] public int? Status { get; set; }
    }

    public bool IsValid()
    {
        var url =
            $"{PartnerId}:{SystemOrderCode}:{PartnerOrderCode}:{GuestId}:{Amount}:{AmountCny}:{OtcUsdtCny}:{RequestTime}:{ExtraData}" +
            $":{Payment.TxId}:{Payment.PaidAmount}:{Payment.Fees}:{Payment.PaymentTime}:{Payment.CallbackTime}:{Payment.Erc20Address}:{Payment.Trc20Address}:{Payment.Status}" +
            $":{_options.MerchantSecret}";
        return Sign == Utils.Md5Hash(url).ToLower();
    }
}