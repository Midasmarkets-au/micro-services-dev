using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Message
{
    public long Id { get; set; }

    public int Type { get; set; }

    public long PartyId { get; set; }

    public int SenderType { get; set; }

    public long SenderId { get; set; }

    public int ReferenceType { get; set; }

    public long ReferenceId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public DateTime? ReadOn { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;
}
