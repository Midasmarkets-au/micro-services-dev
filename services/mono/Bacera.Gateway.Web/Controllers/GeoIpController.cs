using Bacera.Gateway.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Controllers;

[Tags("Public/Geo Ip")]
public class GeoIpController : BaseController
{
    private readonly IGeoIp _geoIp;

    public GeoIpController(IGeoIp geoIp)
    {
        _geoIp = geoIp;
    }

    /// <summary>
    /// Get country for request ip
    /// </summary>
    /// <returns></returns>
    [HttpGet("country/current")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GeoIpResponse.Country))]
    public async Task<IActionResult> Location()
    {
        await Task.Delay(0);
        var ip = GetRemoteIpAddress();
        var data = _geoIp.CountryLookup(ip);
        return Ok(data);
    }

    /// <summary>
    /// Lookup country for ipv4/ipv6 Ip Address
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    [HttpGet("country/{ip}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GeoIpResponse.Country))]
    public async Task<IActionResult> Lookup(string ip)
    {
        await Task.Delay(0);
        var data = _geoIp.CountryLookup(ip);
        return Ok(data);
    }
}