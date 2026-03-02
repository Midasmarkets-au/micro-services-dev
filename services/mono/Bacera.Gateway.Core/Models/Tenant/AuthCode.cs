namespace Bacera.Gateway;

public partial class AuthCode
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public string Code { get; set; } = "";
    public string Event { get; set; } = "";
    public string MethodValue { get; set; } = "";
    public short Method { get; set; }
    public short Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ExpireOn { get; set; }

    public virtual Party Party { get; set; } = null!;
}