using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Matter
{
    public long Id { get; set; }

    public long? Pid { get; set; }

    public int Type { get; set; }

    public DateTime PostedOn { get; set; }

    public int StateId { get; set; }

    public DateTime StatedOn { get; set; }

    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

    public virtual Deposit? Deposit { get; set; }

    public virtual ICollection<Matter> InversePidNavigation { get; set; } = new List<Matter>();

    public virtual ICollection<Ledger> Ledgers { get; set; } = new List<Ledger>();

    public virtual Matter? PidNavigation { get; set; }

    public virtual Rebate? Rebate { get; set; }

    public virtual Transaction? Transaction { get; set; }
    public virtual Refund? Refund { get; set; }

    public virtual MatterType TypeNavigation { get; set; } = null!;

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();

    public virtual Withdrawal? Withdrawal { get; set; }
    public virtual WalletAdjust? WalletAdjust { get; set; }
}