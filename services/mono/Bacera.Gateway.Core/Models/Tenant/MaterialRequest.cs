using System;

namespace Bacera.Gateway;

public partial class MaterialRequest
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public int Status { get; set; }

    public string MaterialType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int? Quantity { get; set; }

    public string Attachments { get; set; } = "[]";

    public string Note { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public virtual Party Party { get; set; } = null!;
}
