//! Currency ids, mirroring mono's `Bacera.Gateway.CurrencyTypes`.
//!
//! These are ISO-4217 *numeric* codes, which is what is actually stored in
//! `trd."_Currency"."Id"` and therefore in every `"CurrencyId"` column
//! (`trd."_Account"`, `trd.trade_rebate_k8s`, ...). Anything that compares a
//! currency id against a literal must use these constants — a plain ordinal
//! (USD=1, USC=2, ...) silently matches nothing and makes every lookup fall
//! through to a default.

/// US dollar.
pub const USD: i32 = 840;

/// US cent. Not an ISO code — a mono-local extension for cent accounts,
/// fixed at 1 USD = 100 USC.
pub const USC: i32 = 841;

/// Maps a currency id to its ISO alpha code, as mono does via
/// `Enum.GetName(typeof(CurrencyTypes), ...)`.
///
/// Returns `None` for ids absent from the enum. Callers must not substitute a
/// default currency: pricing an unknown currency as USD yields a 1:1 rate that
/// is indistinguishable from a legitimate no-op conversion.
pub fn id_to_name(currency_id: i32) -> Option<&'static str> {
    let name = match currency_id {
        8 => "ALL",
        12 => "DZD",
        32 => "ARS",
        36 => "AUD",
        44 => "BSD",
        48 => "BHD",
        50 => "BDT",
        51 => "AMD",
        52 => "BBD",
        60 => "BMD",
        64 => "BTN",
        68 => "BOB",
        72 => "BWP",
        84 => "BZD",
        90 => "SBD",
        96 => "BND",
        104 => "MMK",
        108 => "BIF",
        116 => "KHR",
        124 => "CAD",
        132 => "CVE",
        136 => "KYD",
        144 => "LKR",
        152 => "CLP",
        156 => "CNY",
        170 => "COP",
        174 => "KMF",
        188 => "CRC",
        191 => "HRK",
        192 => "CUP",
        203 => "CZK",
        208 => "DKK",
        214 => "DOP",
        222 => "SVC",
        230 => "ETB",
        232 => "ERN",
        238 => "FKP",
        242 => "FJD",
        262 => "DJF",
        270 => "GMD",
        292 => "GIP",
        320 => "GTQ",
        324 => "GNF",
        328 => "GYD",
        332 => "HTG",
        340 => "HNL",
        344 => "HKD",
        348 => "HUF",
        352 => "ISK",
        356 => "INR",
        360 => "IDR",
        364 => "IRR",
        368 => "IQD",
        376 => "ILS",
        388 => "JMD",
        392 => "JPY",
        398 => "KZT",
        400 => "JOD",
        404 => "KES",
        408 => "KPW",
        410 => "KRW",
        414 => "KWD",
        417 => "KGS",
        418 => "LAK",
        422 => "LBP",
        426 => "LSL",
        430 => "LRD",
        434 => "LYD",
        446 => "MOP",
        454 => "MWK",
        458 => "MYR",
        462 => "MVR",
        480 => "MUR",
        484 => "MXN",
        496 => "MNT",
        498 => "MDL",
        504 => "MAD",
        512 => "OMR",
        516 => "NAD",
        524 => "NPR",
        532 => "ANG",
        533 => "AWG",
        548 => "VUV",
        554 => "NZD",
        558 => "NIO",
        566 => "NGN",
        578 => "NOK",
        586 => "PKR",
        590 => "PAB",
        598 => "PGK",
        600 => "PYG",
        604 => "PEN",
        608 => "PHP",
        634 => "QAR",
        643 => "RUB",
        646 => "RWF",
        654 => "SHP",
        682 => "SAR",
        690 => "SCR",
        694 => "SLL",
        702 => "SGD",
        704 => "VND",
        706 => "SOS",
        710 => "ZAR",
        728 => "SSP",
        748 => "SZL",
        752 => "SEK",
        756 => "CHF",
        760 => "SYP",
        764 => "THB",
        776 => "TOP",
        780 => "TTD",
        784 => "AED",
        788 => "TND",
        800 => "UGX",
        807 => "MKD",
        818 => "EGP",
        826 => "GBP",
        834 => "TZS",
        840 => "USD",
        841 => "USC",
        858 => "UYU",
        860 => "UZS",
        882 => "WST",
        886 => "YER",
        901 => "TWD",
        927 => "UYW",
        928 => "VES",
        929 => "MRU",
        930 => "STN",
        931 => "CUC",
        932 => "ZWL",
        933 => "BYN",
        934 => "TMT",
        936 => "GHS",
        938 => "SDG",
        940 => "UYI",
        941 => "RSD",
        943 => "MZN",
        944 => "AZN",
        946 => "RON",
        947 => "CHE",
        948 => "CHW",
        949 => "TRY",
        950 => "XAF",
        951 => "XCD",
        952 => "XOF",
        953 => "XPF",
        967 => "ZMW",
        968 => "SRD",
        969 => "MGA",
        970 => "COU",
        971 => "AFN",
        972 => "TJS",
        973 => "AOA",
        975 => "BGN",
        976 => "CDF",
        977 => "BAM",
        978 => "EUR",
        979 => "MXV",
        980 => "UAH",
        981 => "GEL",
        984 => "BOV",
        985 => "PLN",
        986 => "BRL",
        990 => "CLF",
        997 => "USN",
        999 => "PTS", // BCR Points
        _ => return None,
    };
    Some(name)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn constants_are_iso_numeric_codes() {
        assert_eq!(USD, 840);
        assert_eq!(USC, 841);
    }

    #[test]
    fn id_to_name_resolves_traded_currencies() {
        assert_eq!(id_to_name(840), Some("USD"));
        assert_eq!(id_to_name(841), Some("USC"));
        assert_eq!(id_to_name(978), Some("EUR"));
        assert_eq!(id_to_name(826), Some("GBP"));
        assert_eq!(id_to_name(392), Some("JPY"));
        assert_eq!(id_to_name(344), Some("HKD"));
        assert_eq!(id_to_name(36), Some("AUD"));
    }

    #[test]
    fn id_to_name_rejects_unknown_ids() {
        // Notably the old ordinal scheme (USD=1, USC=2, ...) must not resolve.
        assert_eq!(id_to_name(1), None);
        assert_eq!(id_to_name(2), None);
        assert_eq!(id_to_name(0), None);
        assert_eq!(id_to_name(-1), None);
        assert_eq!(id_to_name(12345), None);
    }
}
