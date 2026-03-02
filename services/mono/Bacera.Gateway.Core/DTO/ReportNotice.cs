using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bacera.Gateway;

public class ReportNotice
{
    public long Total { get; set; }
    public List<object> Data { get; set; } = new();
    public string Report { get; set; } = string.Empty;

    public string ToJson() => JsonConvert.SerializeObject(this, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

    public static ReportNotice Build(string report, long total = 0, List<object>? data = null)
        => new()
        {
            Report = report,
            Total = total,
            Data = data ?? new List<object>()
        };
}