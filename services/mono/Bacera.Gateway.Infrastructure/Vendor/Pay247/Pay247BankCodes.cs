namespace Bacera.Gateway.Vendor.Pay247;

/// <summary>
/// Currency-keyed Pay247 bank-code seed table (from the vendor doc Bank pages).
///
/// <para>
/// <b>Role:</b> this is the <b>seed / fallback layer</b>, NOT the runtime source of truth.
/// The runtime authority lives in <see cref="Pay247Options.Banks"/> on each
/// <c>_PaymentMethod.Configuration</c> row (per merchant, per tenant, mutable without a deploy).
/// </para>
///
/// <list type="number">
/// <item><description><b>Bootstrap a new tenant row</b> — <see cref="GetSeedBanks"/> returns the
/// vendor snapshot so a freshly seeded row is FE-ready out of the gate.</description></item>
/// <item><description><b>Soft-check fallback</b> — when a row's <c>Banks[currency]</c> is empty,
/// <see cref="Pay247Options.IsBankSupported"/> still uses <see cref="IsKnown"/> to warn-log
/// unknown codes.</description></item>
/// <item><description><b>Developer reference</b> — <c>grep BCA</c> should produce a recognisable
/// bank name without a DB query.</description></item>
/// </list>
///
/// <para>
/// Doc references:
///   ID: https://docs.pay247.io/api-reference/en/banks/bank_id.md (~180 entries)
///   MY: https://docs.pay247.io/api-reference/en/banks/bank_my.md (~55 entries)
///   VN: https://docs.pay247.io/api-reference/en/banks/bank_vn.md (~65 entries)
///   PH: https://docs.pay247.io/api-reference/en/banks/bank_ph.md (~95 entries)
///
/// We seed the most commonly used real banks per country (the long tail of provincial/syariah
/// units lives in the doc URL and can be added per-tenant via <c>Configuration.Banks</c>).
/// Non-BANK methods (GCASH, MAYA, QRIS, DANA, VIETQR, EWALLET) have <c>bank_code = pay_method</c>
/// on the wire and don't need a seed entry — <see cref="Pay247Options.IsBankSupported"/>
/// short-circuits to <c>true</c> for those.
/// </para>
/// </summary>
public static class Pay247BankCodes
{
    /// <summary>
    /// Seed snapshot of vendor-published bank codes, keyed by ISO currency. Treat as immutable.
    /// Each entry maps Pay247's <c>bank_code</c> → official display name.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ByCurrency { get; } =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            // -----------------------------------------------------------------------------------
            // Indonesia (IDR) — top ~40 by domestic volume + the 4 e-wallet method codes.
            // Full list (~180) is in the vendor doc; add the long tail per-tenant via Banks dict.
            // -----------------------------------------------------------------------------------
            ["IDR"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Top retail banks
                ["BCA"]            = "Bank Central Asia (BCA)",
                ["BNI"]            = "Bank Negara Indonesia (BNI)",
                ["BRI"]            = "Bank Rakyat Indonesia (BRI)",
                ["MANDIRI"]        = "Bank Mandiri",
                ["BSI"]            = "Bank Syariah Indonesia",
                ["CIMB"]           = "CIMB Niaga",
                ["PERMATA"]        = "Bank Permata",
                ["DANAMON"]        = "Bank Danamon",
                ["BTN"]            = "Bank Tabungan Negara (BTN)",
                ["MAYBANK"]        = "Maybank Indonesia",
                ["PANIN"]          = "Bank Panin",
                ["OCBC"]           = "Bank OCBC NISP",
                ["UOB"]            = "Bank UOB",
                ["MEGA"]           = "Bank Mega",
                ["SINARMAS"]       = "Bank Sinarmas",
                ["DBS"]            = "Bank DBS",
                ["HSBC"]           = "HSBC Indonesia",
                ["CITI"]           = "Citibank Indonesia",
                ["STANCHART"]      = "Standard Chartered Indonesia",
                ["BTPN"]           = "BTPN",
                ["BJB"]            = "Bank BJB",
                ["DKI"]            = "Bank DKI",
                ["JAGO"]           = "Bank Jago",
                ["SEABANK"]        = "PT Bank Seabank Indonesia",
                ["ALLO"]           = "Allo Bank",
                ["NEO"]            = "Bank Neo Commerce",
                ["BLU"]            = "Blu (BCA Digital)",
                ["LINE"]           = "LINE Bank",
                ["MNC"]            = "MNC Bank",
                ["BANTEN"]         = "BPD Banten",
                ["JATIM"]          = "Bank Jatim",
                ["JATENG"]         = "Bank Jateng",
                ["AGRIS"]          = "Bank Agris",
                ["COMM"]           = "CommBank",
                ["NOBU"]           = "Nobu Bank",
                // E-wallets / QRIS (the "bank_code" equals the method name on the wire)
                ["QRIS"]           = "QRIS (Quick Response Code Indonesian Standard)",
                ["DANA"]           = "DANA",
                ["OVO"]            = "OVO",
                ["GOPAY"]          = "GoPay",
                ["LINKAJA"]        = "LinkAja",
                ["SHOPEEPAY"]      = "ShopeePay Indonesia",
            },

            // -----------------------------------------------------------------------------------
            // Malaysia (MYR) — top ~25 banks + EWALLET method codes.
            // -----------------------------------------------------------------------------------
            ["MYR"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["MBB"]                 = "Maybank",
                ["CIMB"]                = "CIMB Bank",
                ["PBB"]                 = "Public Bank",
                ["RHB"]                 = "RHB Bank",
                ["HLB"]                 = "Hong Leong Bank",
                ["AMMB"]                = "AmBank (AmOnline)",
                ["ABMB"]                = "Alliance Bank",
                ["ABB"]                 = "Affin Bank",
                ["BIMB"]                = "Bank Islam",
                ["BMMB"]                = "Bank Muamalat",
                ["HSBC"]                = "HSBC Bank Malaysia",
                ["OCBC"]                = "OCBC Bank Malaysia",
                ["UOB"]                 = "United Overseas Bank (Malaysia)",
                ["BOC"]                 = "Bank Of China (Malaysia)",
                ["CITI"]                = "Citibank Malaysia",
                ["STANDARD_CHARTERED"]  = "Standard Chartered Bank Malaysia",
                ["RAKYAT"]              = "Bank Rakyat",
                ["BKRM"]                = "Bank Kerjasama Rakyat Malaysia Berhad",
                ["AGROB"]               = "Agro Bank",
                ["MBSB"]                = "MBSB",
                ["BOOST_BANK"]          = "Boost Bank",
                ["GX"]                  = "GX Bank",
                ["AEON"]                = "AEON Bank",
                ["RYT"]                 = "RYT Bank",
                // E-wallets / common channels
                ["GRAB"]                = "Grab (GrabPay)",
                ["TNG"]                 = "Touch N Go",
                ["TNG_EWALLET"]         = "TNG eWallet",
                ["SHOPEEPAY"]           = "ShopeePay Malaysia",
                ["BOOST_EWALLET"]       = "Boost eWallet",
                ["BIG_PAY"]             = "BigPay",
                ["EWALLET"]             = "Generic E-Wallet (Pay247 routes to user choice)",
            },

            // -----------------------------------------------------------------------------------
            // Vietnam (VND) — the full numeric-code set (201..246) + named codes from Pay247 doc.
            // -----------------------------------------------------------------------------------
            ["VND"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["201"]                = "VietcomBank",
                ["203"]                = "BIDV Bank",
                ["204"]                = "Sacombank",
                ["205"]                = "MBBank",
                ["206"]                = "Eximbank",
                ["207"]                = "KienlongBank",
                ["208"]                = "Agribank",
                ["209"]                = "GPBank",
                ["210"]                = "HDBank",
                ["211"]                = "Indovina Bank",
                ["212"]                = "Maritime Bank (MSB)",
                ["213"]                = "Nam A Bank",
                ["214"]                = "OceanBank",
                ["215"]                = "ACB Bank",
                ["216"]                = "SeABank",
                ["217"]                = "ABBANK",
                ["218"]                = "BAC A BANK",
                ["219"]                = "BAOVIET Bank",
                ["220"]                = "Cake Digital Bank by VPBank",
                ["221"]                = "Vietnam Construction Bank (VCCB / CB)",
                ["222"]                = "CIMB Vietnam",
                ["223"]                = "DONG A BANK",
                ["224"]                = "HLBANK",
                ["225"]                = "HSBC Vietnam",
                ["226"]                = "LienVietPostBank (LPBank)",
                ["227"]                = "NCB",
                ["228"]                = "OCB",
                ["229"]                = "Public Bank Vietnam",
                ["230"]                = "PG Bank",
                ["231"]                = "PVcomBank",
                ["232"]                = "SaiGonBank",
                ["233"]                = "SCB",
                ["234"]                = "SHB",
                ["235"]                = "ShinhanBank (Vietnam) Ltd",
                ["236"]                = "Techcombank (TCB)",
                ["237"]                = "TPBank",
                ["238"]                = "United Overseas Bank (Vietnam) Limited",
                ["239"]                = "Vietnam International Bank (VIB)",
                ["240"]                = "Viet Capital Bank",
                ["241"]                = "VietABank",
                ["242"]                = "VietBank",
                ["243"]                = "VPBANK",
                ["244"]                = "Vietnam Russia Joint Venture Bank (VRB)",
                ["245"]                = "Vietinbank",
                ["246"]                = "Woori Bank Vietnam Limited",
                // Named codes
                ["VB"]                 = "VIET BANK",
                ["VAB"]                = "VIET A BANK",
                ["TIMO"]               = "TIMO",
                ["KOOKMIN_BANK_HN"]    = "KOOKMIN BANK Hanoi",
                ["KOOKMIN_BANK_HCM"]   = "KOOKMIN BANK HCM",
                ["KASIKORN_BANK"]      = "KASIKORN BANK",
                ["IBK_HN"]             = "IBK Hanoi",
                ["COOP_BANK"]          = "COOP BANK",
                ["BIDC"]               = "BIDC",
                ["BAB"]                = "Bắc Á",
                ["BNP"]                = "Bank BNP Paribas Vietnam",
                ["BVB"]                = "Bảo Việt",
                ["CTG"]                = "Công thương Việt Nam (VietinBank)",
                ["EAB"]                = "Đông Á",
                ["EIB"]                = "Xuất nhập khẩu Việt Nam (Eximbank)",
                ["KLB"]                = "Kiên Long",
                ["MVAS"]               = "Trung tâm DV số Mobifone",
                ["PBVN"]               = "Public Vietnam",
                ["PGB"]                = "Xăng dầu Petrolimex",
                ["PVB"]                = "Đại chúng Việt Nam (PVcomBank)",
                ["SC"]                 = "Standard Chartered Bank Vietnam",
                ["SGB"]                = "Sài Gòn công thương",
                ["VCCB"]               = "Bản Việt",
                ["VTLMONEY"]           = "Viettel Money",
                // QR / e-wallet pseudo-codes
                ["VIETQR"]             = "VietQR (interbank QR; bank chosen on cashier)",
            },

            // -----------------------------------------------------------------------------------
            // Philippines (PHP) — top banks + e-wallets. Vendor list has ~95 entries.
            // -----------------------------------------------------------------------------------
            ["PHP"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["GCASH"]    = "GCash",
                ["MAYA"]     = "Maya Bank (PayMaya)",
                ["BPI"]      = "BPI Bank",
                ["UNIBANK"]  = "BDO Unibank",
                ["MBT"]      = "Metropolitan Bank and Trust Co",
                ["LBOB"]     = "LANDBANK / OFBank",
                ["SBC"]      = "Security Bank Corporation",
                ["UBP"]      = "Union Bank of the Philippines",
                ["PNB"]      = "Philippine National Bank",
                ["CBC"]      = "China Banking Corporation",
                ["EWBC"]     = "East West Banking Corporation",
                ["RCBC"]     = "RCBC / DiskarTech",
                ["UCPB"]     = "United Coconut Planters Bank (UCPB)",
                ["PSB"]      = "Philippine Savings Bank",
                ["AUB"]      = "Asia United Bank Corporation",
                ["PBC"]      = "Philippine Bank of Communications",
                ["DBP"]      = "Development Bank of the Philippines",
                ["AB"]       = "ALLBANK (A Thrift Bank)",
                ["BC"]       = "Bank of Commerce",
                ["BNB"]      = "BDO Network Bank",
                ["CBS"]      = "China Bank Savings",
                ["COINS"]    = "Coins.ph (DCPay)",
                ["CTBC"]     = "CTBC Bank (Philippines) Corporation",
                ["ING"]      = "ING Bank N.V.",
                ["MBP"]      = "Maybank Philippines",
                ["PMP"]      = "PayMaya Philippines",
                ["RBB"]      = "Robinsons Bank Corporation",
                ["SB"]       = "Seabank",
                ["SP"]       = "ShopeePay Philippines",
                ["GP"]       = "GrabPay Philippines",
                ["TC"]       = "TayoCash",
                ["TDB"]      = "Tonik Bank",
                ["GOT"]      = "GoTyme Bank",
                ["UDB"]      = "UnionDigital Bank",
                ["UNOB"]     = "UNO Bank",
                ["OWNB"]     = "OWN Bank",
                ["PPS"]      = "PalawanPay",
                ["PNS"]      = "PNB Savings Bank",
                ["BOTPI"]    = "Bank Of The Philippine Islands",
                ["ALLIED"]   = "Allied Banking Corp",
            },
        };

    /// <summary>
    /// Bootstrap helper for seeding a new <c>_PaymentMethod</c> row's <see cref="Pay247Options.Banks"/>.
    /// Returns the vendor-published list as a <see cref="Pay247BankEntry"/> array ready to be
    /// serialised into <c>Configuration.Banks[currency]</c>. Currencies with no published table
    /// return an empty array.
    /// </summary>
    public static IReadOnlyList<Pay247BankEntry> GetSeedBanks(string currency)
    {
        if (!ByCurrency.TryGetValue(currency ?? string.Empty, out var table))
            return Array.Empty<Pay247BankEntry>();

        return table.Select(kv => new Pay247BankEntry { Code = kv.Key, Name = kv.Value }).ToArray();
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
