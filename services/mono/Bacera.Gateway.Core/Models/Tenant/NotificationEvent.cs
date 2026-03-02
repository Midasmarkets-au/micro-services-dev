using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class NotificationEvent
{
    public long Id { get; set; }

    public int SubjectType { get; set; }

    public int MethodType { get; set; }

    public int ChannelType { get; set; }

    public short IsActivated { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string ModuleName { get; set; } = null!;

    public virtual ICollection<NotificationSubscription> NotificationSubscriptions { get; set; } = new List<NotificationSubscription>();
}
