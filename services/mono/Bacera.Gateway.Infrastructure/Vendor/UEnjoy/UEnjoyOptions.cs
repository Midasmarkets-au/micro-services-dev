namespace Bacera.Gateway.Vendor;

public sealed class UEnjoyOptions
{
    public string EndPoint { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string SignVersion { get; set; } = null!;

    public string Currency { get; set; } = null!;
    public string UserAreaCode { get; set; } = null!;
    public string LegalCurrency { get; set; } = null!;
    public string KycLevel { get; set; } = null!;
    public string CallbackDomain { get; set; } = null!;


    public bool IsValid() => !string.IsNullOrEmpty(EndPoint) && !string.IsNullOrEmpty(SecretKey) &&
                             !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(CallbackDomain);

    public static UEnjoyOptions FromJson(string json) => Utils.JsonDeserializeObjectWithDefault<UEnjoyOptions>(json);
}