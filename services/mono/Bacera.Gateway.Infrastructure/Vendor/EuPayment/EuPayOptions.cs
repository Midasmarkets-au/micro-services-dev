namespace Bacera.Gateway.Vendor.EuPayment;

public class EuPayOptions
{
    /// <summary>Secure1Gateway processTx API (https://ts.secure1gateway.com/api/v2/processTx)</summary>
    public const string DefaultEndpoint = "https://ts.secure1gateway.com/api/v2/processTx";

    public string AccountId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PassPhrase { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ActionType { get; set; } = "payment"; // payment, preauthorization, eft
    public string AccountGateway { get; set; } = "1";
    public string CurrencyCode { get; set; } = "USD";
    public string CallbackUrl { get; set; } = "https://example.com/callback";

    public static EuPayOptions FromJson(string json)
        => Utils.JsonDeserializeObjectWithDefault<EuPayOptions>(json);
}