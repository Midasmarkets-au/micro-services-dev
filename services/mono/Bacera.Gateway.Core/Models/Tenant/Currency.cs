using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Currency
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Entity { get; set; } = null!;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();

    public virtual ICollection<ExchangeRate> ExchangeRateFromCurrencies { get; set; } = new List<ExchangeRate>();

    public virtual ICollection<ExchangeRate> ExchangeRateToCurrencies { get; set; } = new List<ExchangeRate>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Ledger> Ledgers { get; set; } = new List<Ledger>();

    public virtual ICollection<PaymentServiceAccess> PaymentServiceAccesses { get; set; } =
        new List<PaymentServiceAccess>();

    public virtual ICollection<PaymentService> PaymentServices { get; set; } = new List<PaymentService>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Rebate> Rebates { get; set; } = new List<Rebate>();

    public virtual ICollection<TradeAccount> TradeAccounts { get; set; } = new List<TradeAccount>();

    public virtual ICollection<TradeRebate> TradeRebates { get; set; } = new List<TradeRebate>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();

    public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
}