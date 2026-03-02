using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Medium
{
    public long Id { get; set; }

    public long? Pid { get; set; }

    public long TenantId { get; set; }

    public long PartyId { get; set; }

    public long RowId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? DeletedOn { get; set; }

    public long Length { get; set; }

    public string Guid { get; set; } = null!;

    public string Type { get; set; } = null!;
    public string Context { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string Url { get; set; } = null!;

    public virtual ICollection<Medium> InversePidNavigation { get; set; } = new List<Medium>();

    public virtual Medium? PidNavigation { get; set; }
    public virtual Party Party { get; set; } = null!;
}