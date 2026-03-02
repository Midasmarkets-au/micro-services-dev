using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.BipiPay;

public class BipiPayCallbackViewModel
{
    private BipiPayOptions _options = new();

    public BipiPayCallbackViewModel ApplyOptions(BipiPayOptions options)
    {
        _options = options;
        return this;
    }

    public bool IsValid()
    {
        var form = BuildForm();
        var url = form
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}={x.Value}").Aggregate((x, y) => $"{x}&{y}");

        url += _options.MerchantSecret;
        var signature = Utils.Sha512Hash(url);
        return signature == Signed;
    }

    public bool IsSuccess() => Result == "Approved";

    private Dictionary<string, string> BuildForm() =>
        new()
        {
            { "txcode", TxCode },
            { "merchant", Merchant },
            { "customer", Customer },
            { "currency", Currency },
            { "amount", $"{Amount:0.00}" },
            { "paytxtime", PayTxTime },
            { "paytxno", PayTxNo },
            { "result", Result }
        };

    public string GePaymentNumber() => TxCode;
    public string GetReferenceNumber() => PayTxNo;

    [JsonPropertyName("txcode"), JsonProperty("txcode")]
    public string TxCode { get; set; } = string.Empty;

    [JsonPropertyName("merchant"), JsonProperty("merchant")]
    public string Merchant { get; set; } = string.Empty;

    [JsonPropertyName("currency"), JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("amount"), JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("paytxtime"), JsonProperty("paytxtime")]
    public string PayTxTime { get; set; } = string.Empty;

    [JsonPropertyName("paytxno"), JsonProperty("paytxno")]
    public string PayTxNo { get; set; } = string.Empty;

    [JsonPropertyName("result"), JsonProperty("result")]
    public string Result { get; set; } = string.Empty;

    [JsonPropertyName("signed"), JsonProperty("signed")]
    public string Signed { get; set; } = string.Empty;

    [JsonPropertyName("customer"), JsonProperty("customer")]
    public string Customer { get; set; } = string.Empty;

    [JsonPropertyName("error"), JsonProperty("error")]
    public string? Error { get; set; }
}