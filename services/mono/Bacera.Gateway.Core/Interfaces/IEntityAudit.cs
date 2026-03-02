namespace Bacera.Gateway;

public interface IEntityAudit
{
    public long Id { get; set; }

    public int Type { get; set; }

    public long RowId { get; set; }

    public int Action { get; set; }

    public long PartyId { get; set; }

    public DateTime CreatedOn { get; set; }

    public string Environment { get; set; }

    public string Data { get; set; }

}