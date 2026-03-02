namespace Bacera.Gateway;

public partial class CentralApiLog
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public int StatusCode { get; set; }
    public string ConnectionId { get; set; } = null!;
    public string Method { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? UserAgent { get; set; }

    public string? Referer { get; set; }
    public string? Parameters { get; set; }
    public string? RequestContent { get; set; }
    public string? ResponseContent { get; set; }
    public string? Ip { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    // public virtual Party Party { get; set; } = null!;
}