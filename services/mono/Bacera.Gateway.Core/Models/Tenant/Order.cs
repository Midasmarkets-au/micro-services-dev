namespace Bacera.Gateway;

public partial class Order
{
    public long Id { get; set; }

    public long PartyId { get; set; }
    public long ProductId { get; set; }

    public long Number { get; set; }

    public long Amount { get; set; }

    public short Status { get; set; }

    public string Recipient { get; set; } = null!;

    public string Note { get; set; } = null!;

    public string? Supplement { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public virtual Party Party { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}