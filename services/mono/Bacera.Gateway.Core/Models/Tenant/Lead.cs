using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Lead
{
    public long Id { get; set; }

    public long? PartyId { get; set; }

    public int SourceType { get; set; }

    public int Status { get; set; }

    public int IsArchived { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Supplement { get; set; } = null!;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    public virtual Party? Party { get; set; }
}
