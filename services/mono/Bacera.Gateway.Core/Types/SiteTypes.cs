namespace Bacera.Gateway;

public enum SiteTypes
{
    Default = 0,
    BritishVirginIslands = 1,
    Australia = 2,
    China = 3,
    Taiwan = 4,
    Vietnam = 5,
    Japan = 6,
    Mongolia = 7,
    Malaysia = 8,
}

public static class SiteType
{
    public static int[] GetAll() => (int[])Enum.GetValues(typeof(SiteTypes));
}