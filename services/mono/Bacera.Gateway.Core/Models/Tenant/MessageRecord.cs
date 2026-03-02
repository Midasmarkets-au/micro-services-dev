namespace Bacera.Gateway;

public partial class MessageRecord
{
    public long Id { get; set; }

    public string Method { get; set; } = null!;

    public string Receiver { get; set; } = null!;

    public long ReceiverPartyId { get; set; }

    public string? Content { get; set; }

    public string Event { get; set; } = null!;

    public long EventId { get; set; }

    public short Status { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public virtual Party ReceiverParty { get; set; } = null!;
}