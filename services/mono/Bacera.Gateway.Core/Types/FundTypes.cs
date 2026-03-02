namespace Bacera.Gateway;

public enum FundTypes
{
    Wire = 1,
    Ips = 2,
    FundType3 = 3,
    FundType4 = 4,
    FundType5 = 5,
}

public static class FundTypesExtension
{
    public static bool IsValid(this FundTypes type) => Enum.IsDefined(type) && (int)type > 0;
}