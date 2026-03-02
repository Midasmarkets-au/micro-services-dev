namespace Bacera.Gateway;

public partial class ChatMessageInbox
{
    public long ReceiverPartyId { get; set; }
    public long ChatMessageId { get; set; }

    public DateTime? DeliveredOn { get; set; }

    public virtual ChatMessage ChatMessage { get; set; } = null!;
    public virtual Party ReceiverParty { get; set; } = null!;
}