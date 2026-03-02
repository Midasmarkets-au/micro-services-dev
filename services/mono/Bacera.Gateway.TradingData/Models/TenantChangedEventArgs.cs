namespace Bacera.Gateway.TradingData;

[Serializable]
public class TenantChangedEventArgs : EventArgs
{
    public string Name { get; set; }
    public long TenantId { get; set; }
    public string AdditionalInfo { get; set; }

    public TenantChangedEventArgs(string name, long tenantId, string additionalInfo = "")
    {
        Name = name;
        TenantId = tenantId;
        AdditionalInfo = additionalInfo;
    }
}