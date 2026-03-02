namespace Bacera.Gateway;

public static class LanguageTypes
{
    public const string Chinese = "zh-cn";
    public const string English = "en-us";
    public const string Indonesian = "id-id";
    public const string Japanese = "jp-jp";
    public const string Malay = "ms-my";
    public const string Mongolian = "mn-mn";
    public const string Thai = "th-th";
    public const string TraditionalChineseHongKong = "zh-hk";
    public const string TraditionalChineseTaiWan = "zh-tw";
    public const string Vietnamese = "vi-vn";
    public const string Korean = "ko-kr";
    public const string Cambodia = "km-kh";
    public const string Spanish = "es-es";

    /// <summary>
    /// !!!!!!!!!!! Do not change the order of this list !!!!!!!!!!!
    /// </summary>
    public static readonly List<string> All = new()
    {
        Chinese, // 0
        English, // 1
        Indonesian, // 2
        Japanese, // 3
        Malay, // 4
        Mongolian, // 5
        Thai, // 6
        TraditionalChineseHongKong, // 7
        TraditionalChineseTaiWan, // 8
        Vietnamese, // 9
        Korean, // 10
        Cambodia, // 11
        Spanish, // 12
    };

    /// <summary>
    /// !!!!!!!!!!! Do not change the order of this list !!!!!!!!!!!
    /// </summary>
    public static readonly Dictionary<string, long> LangToId = new()
    {
        { Chinese, 0 },
        { English, 1 },
        { Indonesian, 2 },
        { Japanese, 3 },
        { Malay, 4 },
        { Mongolian, 5 },
        { Thai, 6 },
        { TraditionalChineseHongKong, 7 },
        { TraditionalChineseTaiWan, 8 },
        { Vietnamese, 9 },
        { Korean, 10 },
        { Cambodia, 11 },
        { Spanish, 12 },
    };

    public static readonly Dictionary<string, string> LangEnumToWord = new()
    {
        { Chinese, "Chinese" },
        { English, "English" },
        { Indonesian, "Indonesian" },
        { Japanese, "Japanese" },
        { Malay, "Malay" },
        { Mongolian, "Mongolian" },
        { Thai, "Thai" },
        { TraditionalChineseHongKong, "Traditional Chinese (Hong Kong)" },
        { TraditionalChineseTaiWan, "Traditional Chinese (Taiwan)" },
        { Vietnamese, "Vietnamese" },
        { Korean, "Korean" },
        { Cambodia, "Cambodia" },
        { Spanish, "Spanish" },
    };

    public const string AllLanguageRegEx =
        Chinese + "|" +
        English + "|" +
        Indonesian + "|" +
        Japanese + "|" +
        Malay + "|" +
        Mongolian + "|" +
        Thai + "|" +
        TraditionalChineseHongKong + "|" +
        TraditionalChineseTaiWan + "|" +
        Vietnamese + "|" +
        Korean + "|" +
        Cambodia + "|" +
        Spanish;

    public static bool IsExists(string language) => All.Contains(language);

    private static Dictionary<string, string> LanguageDictionary => new()
    {
        { "en", English },
        { "zh_CN", Chinese },
        { "id", Indonesian },
        { "ja", Japanese },
        { "ms", Malay },
        { "mn", Mongolian },
        { "th", Thai },
        { "zh-Hant", TraditionalChineseHongKong },
        { "vi", Vietnamese }, // in use now
        { "ko", Korean },
        { "km", Cambodia },
        { "es", Spanish },
    };

    public static string ParseWebsiteLanguage(string language) =>
        LanguageDictionary.GetValueOrDefault(language, LanguageTypes.English);

    // invert
    private static Dictionary<string, string> LanguageDictionaryInverted =>
        LanguageDictionary.ToDictionary(x => x.Value, x => x.Key);

    public static string ParseCrmLanguage(string language) =>
        LanguageDictionaryInverted.GetValueOrDefault(language, "en");
}