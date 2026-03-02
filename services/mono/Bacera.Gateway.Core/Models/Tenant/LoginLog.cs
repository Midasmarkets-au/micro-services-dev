namespace Bacera.Gateway;

public class LoginLog
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public string UserAgent { get; set; } = null!;
    public string Referer { get; set; } = null!;
    public string IpAddress { get; set; } = null!;
    public DateTime CreatedOn { get; set; }

    public virtual Party Party { get; set; } = null!;
}