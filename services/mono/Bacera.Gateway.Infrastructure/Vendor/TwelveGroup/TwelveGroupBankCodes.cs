namespace Bacera.Gateway.Vendor.TwelveGroup;

/// <summary>
/// Currency-keyed 12Group bank-code seed table (Thai banks).
///
/// <para>
/// <b>Role:</b> seed / fallback layer, NOT the runtime source of truth. Runtime authority lives in
/// <see cref="TwelveGroupOptions.Banks"/> on each <c>_PaymentMethod.Configuration</c> row (per
/// merchant, per tenant, mutable without a deploy). Same contract as <c>RivoBankCodes</c>.
/// </para>
///
/// <para>
/// The vendor's authoritative list ships only as an image-based PDF (<c>Bank Code_20230914.pdf</c>),
/// so we seed the common Thai banks here and let ops extend per-row. Codes are the standard
/// 3-digit BOT bank codes used across Thai QR / PromptPay rails.
/// </para>
/// </summary>
public static class TwelveGroupBankCodes
{
    /// <summary>Seed snapshot of common Thai bank codes (3-digit BOT codes), keyed by currency.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ByCurrency { get; } =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["THB"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["002"] = "Bangkok Bank (BBL)",
                ["004"] = "Kasikornbank (KBANK)",
                ["006"] = "Krung Thai Bank (KTB)",
                ["011"] = "TMBThanachart Bank (TTB)",
                ["014"] = "Siam Commercial Bank (SCB)",
                ["017"] = "Citibank Thailand",
                ["018"] = "Sumitomo Mitsui Banking Corporation (SMBC)",
                ["020"] = "Standard Chartered Bank (Thai)",
                ["022"] = "CIMB Thai Bank (CIMBT)",
                ["024"] = "United Overseas Bank (Thai) (UOB)",
                ["025"] = "Bank of Ayudhya (Krungsri / BAY)",
                ["030"] = "Government Savings Bank (GSB)",
                ["031"] = "Hongkong and Shanghai Banking Corporation (HSBC)",
                ["033"] = "Government Housing Bank (GHB)",
                ["034"] = "Bank for Agriculture and Agricultural Cooperatives (BAAC)",
                ["039"] = "Mizuho Bank",
                ["045"] = "Bank of Ayudhya (legacy)",
                ["066"] = "Islamic Bank of Thailand (IBANK)",
                ["067"] = "Tisco Bank (TISCO)",
                ["069"] = "Kiatnakin Phatra Bank (KKP)",
                ["070"] = "ICBC (Thai)",
                ["071"] = "Thai Credit Retail Bank (TCD)",
                ["073"] = "Land and Houses Bank (LHFG)",
                ["098"] = "Small and Medium Enterprise Development Bank (SME)",
            },
        };

    /// <summary>
    /// Bootstrap helper for seeding a new <c>_PaymentMethod</c> row's <see cref="TwelveGroupOptions.Banks"/>.
    /// Returns the seed list as a <see cref="TwelveGroupBankEntry"/> array. Unknown currencies return empty.
    /// </summary>
    public static IReadOnlyList<TwelveGroupBankEntry> GetSeedBanks(string currency)
    {
        if (!ByCurrency.TryGetValue(currency ?? string.Empty, out var table))
            return Array.Empty<TwelveGroupBankEntry>();

        return table.Select(kv => new TwelveGroupBankEntry { Code = kv.Key, Name = kv.Value }).ToArray();
    }

    /// <summary>True iff <paramref name="code"/> is a known seed bank code for <paramref name="currency"/>.</summary>
    public static bool IsKnown(string currency, string code) =>
        ByCurrency.TryGetValue(currency ?? string.Empty, out var table)
        && !string.IsNullOrWhiteSpace(code)
        && table.ContainsKey(code);

    /// <summary>Returns the seed table for the currency, or an empty dictionary if none.</summary>
    public static IReadOnlyDictionary<string, string> ForCurrency(string currency) =>
        ByCurrency.TryGetValue(currency ?? string.Empty, out var table)
            ? table
            : new Dictionary<string, string>();
}
