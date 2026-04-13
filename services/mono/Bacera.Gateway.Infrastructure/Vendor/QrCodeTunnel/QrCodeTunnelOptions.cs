using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.QrCodeTunnel;

public class QrCodeTunnelOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;

    public static QrCodeTunnelOptions FromJson(string json) =>
        JsonConvert.DeserializeObject<QrCodeTunnelOptions>(json) ?? new QrCodeTunnelOptions();

    public bool IsValid() =>
        !string.IsNullOrEmpty(BaseUrl)
        && !string.IsNullOrEmpty(ApiKey)
        && !string.IsNullOrEmpty(ApiSecret);
}
