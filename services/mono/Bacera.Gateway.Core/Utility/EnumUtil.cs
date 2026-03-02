namespace Bacera.Gateway;

public static class EnumUtil
{

    public static IEnumerable<T> GetValues<T>()
        => Enum.GetValues(typeof(T)).Cast<T>();

    public static IEnumerable<string> GetNames<T>()
        => Enum.GetNames(typeof(T));
}