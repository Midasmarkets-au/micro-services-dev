using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Long77Pay;

public class Long77PayCallbackViewModel
{
    private Long77PayOptions _options = new();

    public void ApplyOptions(Long77PayOptions options)
    {
        _options = options;
    }

    [JsonProperty("partner_id")] public string PartnerId { get; set; } = string.Empty;

    [JsonProperty("system_order_code")] public string SystemOrderCode { get; set; } = string.Empty;

    [JsonProperty("partner_order_code")] public string PartnerOrderCode { get; set; } = string.Empty;

    [JsonProperty("channel_code")] public string ChannelCode { get; set; } = string.Empty;

    [JsonProperty("amount")] public string Amount { get; set; } = string.Empty;

    [JsonProperty("request_time")] public string RequestTime { get; set; } = string.Empty;

    [JsonProperty("extra_data")] public string ExtraData { get; set; } = string.Empty;

    [JsonProperty("payment")] public PaymentViewModel Payment { get; set; } = new();

    [JsonProperty("sign")] public string Sign { get; set; } = string.Empty;

    public class PaymentViewModel
    {
        [JsonProperty("payment_id")] public string PaymentId { get; set; } = string.Empty;

        [JsonProperty("paid_amount")] public string PaidAmount { get; set; } = string.Empty;

        [JsonProperty("fees")] public int? Fees { get; set; }

        [JsonProperty("payment_time")] public int? PaymentTime { get; set; }

        [JsonProperty("bank_code")] public string BankCode { get; set; } = string.Empty;

        [JsonProperty("bank_account_no")] public string BankAccountNo { get; set; } = string.Empty;

        [JsonProperty("bank_account_name")] public string BankAccountName { get; set; } = string.Empty;

        [JsonProperty("callback_time")] public int? CallbackTime { get; set; }

        [JsonProperty("status")] public int? Status { get; set; }
    }

    public bool IsValid()
    {
        var url =
            $"{PartnerId}:{SystemOrderCode}:{PartnerOrderCode}:{ChannelCode}:{Amount}:{RequestTime}:{ExtraData}" +
            $":{Payment.PaymentId}:{Payment.PaidAmount}:{Payment.Fees}:{Payment.PaymentTime}:{Payment.BankCode}" +
            $":{Payment.BankAccountNo}:{Payment.BankAccountName}:{Payment.CallbackTime}:{Payment.Status}" +
            $":{_options.MerchantSecret}";
        return Sign == Utils.Md5Hash(url).ToLower();
    }
}