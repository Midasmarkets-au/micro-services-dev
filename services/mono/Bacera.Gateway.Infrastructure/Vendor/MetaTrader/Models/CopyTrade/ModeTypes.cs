namespace Bacera.Gateway.Vendor.MetaTrader;

public class ModeTypes
{
    public const string SameValue = "SV";
    public const string ByBalance = "BB";
    public const string ByEquity = "BE";
    public const string FixedVolume = "FV";
    public const string ByFreeMargin = "BFM";
    public static readonly string[] All = { "SV", "BB", "BE", "FV", "BFM" };
    public static bool IsValid(string mode) => All.Contains(mode);
    public static bool IsValueValid(string mode, int? value)
    {
        if (mode == FixedVolume && value is null or < 1)
            return false;
        return IsValid(mode);
    }
}