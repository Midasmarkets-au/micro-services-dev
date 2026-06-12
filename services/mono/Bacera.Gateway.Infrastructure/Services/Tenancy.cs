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

    /// <summary>
    /// TODO: 当前只有一个10000租户，后续如果有更多租户需要根据国家码进行区分，可以在这里添加映射关系
    /// </summary>
    public static long GetTenantIdByCountryCode(string countryCode, long? defaultTenantId = null) =>
        countryCode.ToUpper() switch
        {
            "CN" => 10000,
            "TW" => 10000,
            "VN" => 10000,
            "JP" => 10000,
            "MN" => 10000,
            "MY" => 10000,
            "AU" => 10000,
            _ => defaultTenantId ?? 0,
        };
}