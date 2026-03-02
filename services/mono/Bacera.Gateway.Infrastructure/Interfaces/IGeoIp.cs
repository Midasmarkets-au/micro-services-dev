namespace Bacera.Gateway.Interfaces;

public interface IGeoIp
{
    GeoIpResponse.Country CountryLookup(string ip);
}