using System.Net;

using Bacera.Gateway.Interfaces;

using MaxMind.GeoIP2;


namespace Bacera.Gateway.Vendor;

public class MaxMindGeoIp : IGeoIp, IDisposable
{
    private readonly DatabaseReader _reader;

    public MaxMindGeoIp(string countryMmdbPath)
    {
        if (!File.Exists(countryMmdbPath)) throw new FileNotFoundException();
        _reader = new DatabaseReader(countryMmdbPath);
    }

    public GeoIpResponse.Country CountryLookup(string ip)
    {
        var empty = GeoIpResponse.Country.Empty();
        if (false == IPAddress.TryParse(ip, out var ipAddress))
            return empty;

        if (false == _reader.TryCountry(ipAddress, out var response))
            return empty;

        if (response?.Country == null)
            return empty;

        return GeoIpResponse.Country
            .Build(
                ip,
                response.Country.IsoCode ?? "",
                response.Country.Name ?? "",
                new Dictionary<string, string>(response.Country.Names)
            );
    }

    public void Dispose()
    {
        _reader.Dispose();
        GC.SuppressFinalize(this);
    }
}