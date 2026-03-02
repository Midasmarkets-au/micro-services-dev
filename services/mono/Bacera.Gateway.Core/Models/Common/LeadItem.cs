namespace Bacera.Gateway;

public class LeadItem : ILeadable

{
    public long Id { get; set; }
    public int Status { get; set; }
    public string Data { get; set; } = string.Empty;
    public DateTime UpdatedOn { get; set; }
}

public static class LeadItemExtensions
{
    public static LeadItem ToLeadItem<T, TC>(this ILeadable<T, TC> lead)
        where T : struct, IConvertible
        where TC : struct, IConvertible
        => new()
        {
            Id = Convert.ToInt64(lead.Id),
            Status = Convert.ToInt32(lead.Status),
            UpdatedOn = lead.UpdatedOn
        };
}