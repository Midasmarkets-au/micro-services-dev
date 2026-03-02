using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class FundType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();

    public virtual ICollection<PaymentServiceAccess> PaymentServiceAccesses { get; set; } =
        new List<PaymentServiceAccess>();

    public virtual ICollection<Rebate> Rebates { get; set; } = new List<Rebate>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();

    public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();

    public virtual ICollection<PaymentService> PaymentServices { get; set; } = new List<PaymentService>();
}