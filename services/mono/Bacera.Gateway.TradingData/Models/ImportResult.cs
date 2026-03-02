namespace Bacera.Gateway.TradingData.Models;
[Serializable]
public class ImportResult
{
    public long TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public long TradeServiceId { get; set; }
    public string TradeServiceName { get; set; } = string.Empty;

    public int TotalRecords { get; set; }
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public string Note { get; set; } = string.Empty;

    public static ImportResult Create(
        long tenantId, string tenantName = "", string databaseName = "", long tradeServiceId = 0, string tradeServiceName = "",
        int totalRecords = 0, int inserted = 0, int updated = 0) =>
        new()
        {
            TenantId = tenantId,
            TenantName = tenantName,
            DatabaseName = databaseName,
            TradeServiceId = tradeServiceId,
            TradeServiceName = tradeServiceName,
            TotalRecords = totalRecords,
            Inserted = inserted,
            Updated = updated
        };
}