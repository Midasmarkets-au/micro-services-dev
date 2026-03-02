namespace Bacera.Gateway;

public class Tenancy : ITenantGetter, ITenantSetter
{
    public long GetTenantId() => TenantId;
    public bool IsTenantSet() => TenantId != 0;

    private long TenantId { get; set; }

    public void SetTenantId(long tenantId)
    {
        TenantId = tenantId;
    }

    public static string GetTenancyInReferCode(long tenantId) => tenantId switch
    {
        1 => "A",
        10000 => "B",
        10002 => "C",
        10004 => "D",
        _ => "Z",
    };

    public static long GetTenantIdByCountryCode(string countryCode, long? defaultTenantId = null) =>
        countryCode.ToUpper() switch
        {
            "CN" => 10000,
            "TW" => 10000,
            "VN" => 10004,
            "JP" => 10000,
            "MN" => 10000,
            "MY" => 10004,
            "AU" => 1,
            _ => defaultTenantId ?? 0,
        };
}