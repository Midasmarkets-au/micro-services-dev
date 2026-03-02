using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Response;

public class IpInfoViewModel
{
    // {
    //     "ip": "137.25.19.180",
    //     "hostname": "137-025-019-180.res.spectrum.com",
    //     "city": "Monterey Park",
    //     "region": "California",
    //     "country": "US",
    //     "loc": "34.0534,-118.1271",
    //     "org": "AS20115 Charter Communications",
    //     "postal": "91754",
    //     "timezone": "America/Los_Angeles"
    // }
    public string Ip { get; set; } = "";
    public string Hostname { get; set; } = "";
    public string City { get; set; } = "";
    public string Region { get; set; } = "";
    public string Country { get; set; } = "";

    [JsonPropertyName("loc"), JsonProperty("loc")]
    public string Location { get; set; } = "";

    [JsonPropertyName("org"), JsonProperty("org")]

    public string Organization { get; set; } = "";

    public string Postal { get; set; } = "";
    public string Timezone { get; set; } = "";

    public Dictionary<string, string> Ips { get; set; } = new();
}