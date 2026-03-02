namespace Bacera.Gateway;

public partial class Product
{
    public long Id { get; set; }

    public short Type { get; set; }

    public short Status { get; set; }

    public string Name { get; set; } = null!;

    public long Point { get; set; }

    public long Total { get; set; }

    public string? Supplement { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public long? OperatorPartyId { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}