namespace Bacera.Gateway;

public class GeoIpResponse
{
    public class Country
    {
        public string Ip { get; set; } = string.Empty;
        public string? IsoCode { get; init; }
        public string? Name { get; init; }
        public Dictionary<string, string>? Names { get; init; }

        public static Country Build(string ip, string countryCode, string name, Dictionary<string, string> names)
            => new() { Ip = ip, IsoCode = countryCode, Name = name, Names = names };

        public bool IsEmpty() => string.IsNullOrEmpty(IsoCode) || string.IsNullOrEmpty(Name);
        public static Country Empty() => new();
    }
}