namespace Bacera.Gateway.Web.Response;

public class SymbolVolumeResponseModel
{
    public double Volume { get; set; }
    public string Symbol { get; set; } = string.Empty;

    public static SymbolVolumeResponseModel Of(string symbol, double volume) => new()
    {
        Symbol = symbol,
        Volume = volume
    };

    public static SymbolVolumeResponseModel Of(KeyValuePair<string, double> kv) => new()
    {
        Symbol = kv.Key,
        Volume = kv.Value,
    };
}