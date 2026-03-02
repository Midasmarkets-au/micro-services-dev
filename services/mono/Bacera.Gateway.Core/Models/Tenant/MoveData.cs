namespace Bacera.Gateway;

public class MoveData
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public long UserId { get; set; }
    public long AccountUid { get; set; }
    public long AccountId { get; set; }
    public string Data { get; set; } = "{}";
    public string Table { get; set; } = "";
    public long RowId { get; set; }
    public long NewId { get; set; }
}