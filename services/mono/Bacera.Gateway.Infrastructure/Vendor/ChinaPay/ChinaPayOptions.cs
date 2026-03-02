using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.ChinaPay;

public class ChinaPayOptions
{
    public string ApiDomain { get; set; } = string.Empty;
    public string AppId { get; set; } = string.Empty;
    public string ServerPublicKey { get; set; } = string.Empty;
    public string AppPrivateKey { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int Symbol { get; set; }
    public string Charset { get; set; } = "utf-8";
    public int PayType { get; set; }
    public string InterfaceName { get; set; } = string.Empty;

    public string CallbackDomain { get; set; } = string.Empty;

    /// <summary>
    /// Template for callback path with {tenantId}
    /// </summary>
    public string CallbackPathTemplate { get; set; } = "/api/v1/payment/callback/{tenantId}/chinapay";

    /// <summary>
    /// Tenant identifier (for multi-tenant routing)
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// Full callback URL: {CallbackDomain}/{CallbackPathTemplate}
    /// </summary>
    public string CallbackUri =>
        $"{CallbackDomain.TrimEnd('/')}{CallbackPathTemplate.Replace("{tenantId}", TenantId.ToString())}";


    public static ChinaPayOptions FromJson(string json) =>
        JsonConvert.DeserializeObject<ChinaPayOptions>(json) ?? new ChinaPayOptions();

    /// <summary>
    /// Validate required fields
    /// </summary>
    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (string.IsNullOrEmpty(ApiDomain))
            return (false, "ApiDomain is required.");

        if (string.IsNullOrEmpty(AppId))
            return (false, "AppId is required.");

        if (string.IsNullOrEmpty(ServerPublicKey))
            return (false, "ServerPublicKey is required.");

        if (string.IsNullOrEmpty(AppPrivateKey))
            return (false, "AppPrivateKey is required.");

        if (string.IsNullOrEmpty(CallbackDomain))
            return (false, "CallbackDomain is required.");

        if (!Uri.TryCreate(CallbackDomain, UriKind.Absolute, out _))
            return (false, "CallbackDomain must be a valid absolute URL.");

        if (TenantId <= 0)
            return (false, "TenantId must be greater than 0.");

        return (true, string.Empty);
    }
}