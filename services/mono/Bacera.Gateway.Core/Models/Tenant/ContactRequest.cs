using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class ContactRequest
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public short IsArchived { get; set; }

    public DateTime CreatedOn { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Ip { get; set; } = null!;

    public string Content { get; set; } = null!;
}
