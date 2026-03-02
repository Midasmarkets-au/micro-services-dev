using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Wallet
{
    public long Id { get; set; }

    public long PartyId { get; set; }

    public int FundType { get; set; }

    public int CurrencyId { get; set; }

    public long Balance { get; set; }

    public DateTime TalliedOn { get; set; }

    public int Sequence { get; set; }

    public string Number { get; set; } = null!;

    public short IsPrimary { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual FundType FundTypeNavigation { get; set; } = null!;

    public virtual Party Party { get; set; } = null!;

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
    public virtual ICollection<WalletAdjust> WalletAdjusts { get; set; } = new List<WalletAdjust>();
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    public virtual ICollection<WalletDailySnapshot> WalletDailySnapshots { get; set; } = new List<WalletDailySnapshot>();
    public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();

    public virtual ICollection<WalletPaymentMethodAccess> WalletPaymentMethodAccesses { get; set; } = new List<WalletPaymentMethodAccess>();
}