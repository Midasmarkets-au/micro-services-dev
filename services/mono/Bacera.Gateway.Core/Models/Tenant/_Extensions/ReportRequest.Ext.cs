namespace Bacera.Gateway;

partial class ReportRequest
{
    public bool IsEmpty => Id == 0;
    public bool IsGenerated => GeneratedOn.HasValue;
    public bool IsExpired => ExpireOn.HasValue && ExpireOn.Value < DateTime.UtcNow;

    public static ReportRequest Build(long partyId, ReportRequestTypes type, string name, string query)
        => new()
        {
            PartyId = partyId,
            Type = (int)type,
            Name = name,
            Query = query
        };

    public static ReportRequest Build(long partyId, ReportRequestTypes type, string name, string query, int isFromApi = 0)
    => new()
    {
        PartyId = partyId,
        Type = (int)type,
        Name = name,
        Query = query,
        IsFromApi = isFromApi
    };

    public sealed class CreateSpec
    {
        public ReportRequestTypes Type { get; set; }
        public string Name { get; set; } = null!;
        public object Query { get; set; } = null!;
        public int IsFromApi { get; set; } = 1;
    }
}