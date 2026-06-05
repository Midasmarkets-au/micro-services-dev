namespace Bacera.Gateway.Vendor.Rivo;

/// <summary>
/// Currency-keyed Rivo bank-code seed table (Appendix C of the vendor doc).
///
/// <para>
/// <b>Role:</b> this is the <b>seed / fallback layer</b>, NOT the runtime source of truth.
/// The runtime authority lives in <see cref="RivoOptions.Banks"/> on each
/// <c>_PaymentMethod.Configuration</c> row (per merchant, per tenant, mutable without a deploy).
/// This class exists for three cold-path uses:
/// </para>
///
/// <list type="number">
/// <item>
///   <description><b>Bootstrap a new tenant row</b> — <see cref="GetSeedBanks"/> returns the
///   Appendix C snapshot so a freshly seeded row is FE-ready out of the gate. Ops then prunes
///   to whatever the merchant has actually enabled.</description>
/// </item>
/// <item>
///   <description><b>Soft-check fallback</b> — when a row's <c>Banks[currency]</c> is empty,
///   <see cref="RivoOptions.IsBankSupported"/> still uses <see cref="IsKnown"/> to warn-log
///   unknown codes, so we don't go fully blind during config drift.</description>
/// </item>
/// <item>
///   <description><b>Developer reference</b> — `grep VCB` should produce a recognisable
///   bank name without requiring a DB query.</description>
/// </item>
/// </list>
///
/// <para>
/// Only VND is seeded today (63 entries from Appendix C v1.1). Other currencies start empty;
/// <see cref="IsKnown"/> returns <c>false</c> for those — and the vendor doc explicitly says
/// the list is subject to change and "contact your account manager for updates" — so we
/// warn-log rather than block.
/// </para>
/// </summary>
public static class RivoBankCodes
{
    /// <summary>
    /// Seed snapshot of vendor-published bank codes, keyed by currency. Treat as immutable.
    /// Each entry is keyed by Rivo's bank code with the official display name as the value.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ByCurrency { get; } =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["VND"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["VCB"]      = "Vietcombank (Joint Stock Commercial Bank for Foreign Trade of Vietnam)",
                ["BIDV"]     = "BIDV (Bank for Investment and Development of Vietnam)",
                ["MB"]       = "MB Bank (Military Commercial Joint Stock Bank)",
                ["TCB"]      = "Techcombank (Vietnam Technological and Commercial Joint Stock Bank)",
                ["TPB"]      = "TPBank (Tien Phong Commercial Joint Stock Bank)",
                ["VPB"]      = "VPBank (Vietnam Prosperity Joint Stock Commercial Bank)",
                ["ACB"]      = "ACB (Asia Commercial Joint Stock Bank)",
                ["STB"]      = "Sacombank (Saigon Thuong Tin Commercial Joint Stock Bank)",
                ["HDB"]      = "HDBank (Ho Chi Minh City Development Joint Stock Commercial Bank)",
                ["MSB"]      = "MSB (Vietnam Maritime Commercial Joint Stock Bank)",
                ["VIB"]      = "VIB (Vietnam International Commercial Joint Stock Bank)",
                ["SHB"]      = "SHB (Saigon-Hanoi Commercial Joint Stock Bank)",
                ["LPB"]      = "LPBank (LienVietPostBank)",
                ["OCB"]      = "OCB (Orient Commercial Joint Stock Bank)",
                ["EIB"]      = "Eximbank (Vietnam Export Import Commercial Joint Stock Bank)",
                ["SCB"]      = "SCB (Sai Gon Joint Stock Commercial Bank)",
                ["NAB"]      = "Nam A Bank",
                ["ABB"]      = "ABBank (An Binh Commercial Joint Stock Bank)",
                ["PGB"]      = "PG Bank (Petrolimex Group Commercial Joint Stock Bank)",
                ["VAB"]      = "VietABank (Vietnam Asia Commercial Joint Stock Bank)",
                ["BAB"]      = "BacABank (Bac A Commercial Joint Stock Bank)",
                ["KLB"]      = "KienLongBank (Kien Long Commercial Joint Stock Bank)",
                ["NCB"]      = "NCB (National Citizen Commercial Joint Stock Bank)",
                ["SGB"]      = "Saigonbank (Saigon Commercial Joint Stock Bank for Industry and Trade)",
                ["BVB"]      = "BaoVietBank (Bao Viet Joint Stock Commercial Bank)",
                ["VietBank"] = "VietBank (Vietnam Thuong Tin Commercial Joint Stock Bank)",
                ["DongA"]    = "DongA Bank (Dong A Commercial Joint Stock Bank)",
                ["GPB"]      = "GPBank (Global Petro Sole Member Limited Commercial Bank)",
                ["OceanBank"]= "Ocean Bank",
                ["CBBank"]   = "CBBank (Construction Bank)",
                ["VRB"]      = "VRB (Vietnam-Russia Joint Venture Bank)",
                ["VCCB"]     = "Viet Capital Bank (Ban Viet Commercial Joint Stock Bank)",
                ["SeABank"]  = "SeABank (Southeast Asia Commercial Joint Stock Bank)",
                ["PVcomBank"]= "PVcomBank (Vietnam Public Joint Stock Commercial Bank)",

                ["Agribank"] = "Agribank (Vietnam Bank for Agriculture and Rural Development)",
                ["VTB"]      = "VietinBank (Vietnam Joint Stock Commercial Bank for Industry and Trade)",
                ["Vikki"]    = "Vikki Bank",
                ["MAFC"]     = "MAFC (Mirae Asset Finance Company)",
                ["HSBC"]     = "HSBC Vietnam",
                ["SC"]       = "Standard Chartered Vietnam",
                ["CITI"]     = "Citibank Vietnam",
                ["ANZ"]      = "ANZ Vietnam",
                ["WB"]       = "Woori Bank Vietnam",
                ["SHBVN"]    = "Shinhan Bank Vietnam",
                ["UOB"]      = "UOB Vietnam",
                ["PBVN"]     = "Public Bank Vietnam",
                ["IBKHCM"]   = "Industrial Bank of Korea - HCM",
                ["IBKHN"]    = "Industrial Bank of Korea - Hanoi",
                ["BOC"]      = "Bank of China Vietnam",
                ["BIDC"]     = "Banque pour l'Industrie et le Commerce du Cambodge",
                ["DBSHCM"]   = "DBS Bank - HCM Branch",
                ["KBHCM"]    = "Kookmin Bank - HCM",
                ["KBHN"]     = "Kookmin Bank - Hanoi",
                ["KEBHANAHCM"]= "KEB Hana Bank - HCM",
                ["KEBHANAHN"]= "KEB Hana Bank - Hanoi",
                ["MIZUHO"]   = "Mizuho Bank Vietnam",
                ["MUFG"]     = "MUFG Bank",
                ["SMBC"]     = "Sumitomo Mitsui Banking Corporation Vietnam",
                ["NHBHCM"]   = "Nonghyup Bank - HCM Branch",
                ["BNP"]      = "BNP Paribas Vietnam",
                ["DBI"]      = "Deutsche Bank Vietnam",
                ["CIMB"]     = "CIMB Bank Vietnam",
                ["VBSP"]     = "VBSP (Vietnam Bank for Social Policies)",
                ["Coopbank"] = "Coopbank (Co-operative Bank of Vietnam)",
            },
        };

    /// <summary>
    /// Bootstrap helper for seeding a new <c>_PaymentMethod</c> row's <see cref="RivoOptions.Banks"/>.
    /// Returns the vendor-published list as a <see cref="RivoBankEntry"/> array ready to be
    /// serialised into <c>Configuration.Banks[currency]</c>. Currencies with no published table
    /// return an empty array.
    /// </summary>
    public static IReadOnlyList<RivoBankEntry> GetSeedBanks(string currency)
    {
        if (!ByCurrency.TryGetValue(currency ?? string.Empty, out var table))
            return Array.Empty<RivoBankEntry>();

        return table.Select(kv => new RivoBankEntry { Code = kv.Key, Name = kv.Value }).ToArray();
    }

    /// <summary>True iff <paramref name="code"/> is a known seed bank code for <paramref name="currency"/>.</summary>
    public static bool IsKnown(string currency, string code) =>
        ByCurrency.TryGetValue(currency ?? string.Empty, out var table)
        && !string.IsNullOrWhiteSpace(code)
        && table.ContainsKey(code);

    /// <summary>Returns the seed table for the currency, or an empty dictionary if the currency has no published codes yet.</summary>
    public static IReadOnlyDictionary<string, string> ForCurrency(string currency) =>
        ByCurrency.TryGetValue(currency ?? string.Empty, out var table)
            ? table
            : new Dictionary<string, string>();
}
