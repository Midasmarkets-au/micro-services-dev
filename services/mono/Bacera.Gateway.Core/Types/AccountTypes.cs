namespace Bacera.Gateway;

public enum AccountTypes
{
    Unknown = 0,
    Individual = 1,
    Joint = 2,
    Corp = 3,
    Standard = 4,
    Pro = 5,
    Alpha = 6,
    SwapFreeStandard = 7,
    SwapFreePro = 8,
    SwapFreeAlpha = 9,
    WholesaleAlpha = 10,
    Advantage = 11,
    Affiliate = 12,
    SeaSTD = 13,
    AlphaPlus = 14,

    JpSTDCFD = 15,
    JpSTDIND = 16,
    JpSTDFX = 17,
    JpALPCFD = 18,
    JpALPIND = 19,
    JpALPFX = 20,

    SeaPro = 21,
    Elite = 22,
    WholesaleAdvantage = 23,
}

public static class AccountTypesUtils
{
    public static List<int> AllTypes =>
    [
        (int)AccountTypes.Unknown,
        (int)AccountTypes.Individual,
        (int)AccountTypes.Joint,
        (int)AccountTypes.Corp,
        (int)AccountTypes.Standard,
        (int)AccountTypes.Pro,
        (int)AccountTypes.Alpha,
        (int)AccountTypes.SwapFreeStandard,
        (int)AccountTypes.SwapFreePro,
        (int)AccountTypes.SwapFreeAlpha,
        (int)AccountTypes.WholesaleAlpha,
        (int)AccountTypes.Advantage,
        (int)AccountTypes.Affiliate,
        (int)AccountTypes.SeaSTD,
        (int)AccountTypes.AlphaPlus,

        (int)AccountTypes.JpSTDCFD,
        (int)AccountTypes.JpSTDIND,
        (int)AccountTypes.JpSTDFX,
        (int)AccountTypes.JpALPCFD,
        (int)AccountTypes.JpALPIND,
        (int)AccountTypes.JpALPFX,

        (int)AccountTypes.SeaPro,
        (int)AccountTypes.Elite,
        (int)AccountTypes.WholesaleAdvantage,
    ];
}