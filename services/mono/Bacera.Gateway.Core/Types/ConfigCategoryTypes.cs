namespace Bacera.Gateway.Core.Types;

public static class ConfigCategoryTypes
{
    public const string Account = nameof(Account);
    public const string Party = nameof(Party);
    public const string Public = nameof(Public);

    public static List<string> All => new() { Account, Party, Public };

    public static bool IsValid(string value) => All.Select(x => x.ToLower()).Contains(value.ToLower());

    public static string ParseCategory(string value)
    {
        if (value.Equals(Account, StringComparison.CurrentCultureIgnoreCase)) return Account;
        if (value.Equals(Party, StringComparison.CurrentCultureIgnoreCase)) return Party;
        if (value.Equals(Public, StringComparison.CurrentCultureIgnoreCase)) return Public;

        return Public;
    }
}