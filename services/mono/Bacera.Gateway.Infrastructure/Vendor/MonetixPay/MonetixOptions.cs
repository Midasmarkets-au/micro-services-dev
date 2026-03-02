using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MonetixPay;

public class MonetixOptions
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string SecretKeyForSignature { get; set; } = null!;
    public string ProjectId { get; set; } = null!;
    public string SecretKeyForAesEncrypt { get; set; } = null!;
    public string EndPoint { get; set; } = null!;
    public string Server { get; set; } = null!;

    public static MonetixOptions FromJson(string json) =>
        JsonConvert.DeserializeObject<MonetixOptions>(json) ?? new MonetixOptions();
}