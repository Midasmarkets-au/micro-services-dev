namespace Bacera.Gateway;

public partial class ChatParticipant
{
    public long ChatId { get; set; }
    public long PartyId { get; set; }

    public virtual Chat Chat { get; set; } = null!;
    public virtual Party Party { get; set; } = null!;
}