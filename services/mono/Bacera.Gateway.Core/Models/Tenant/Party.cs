using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

public partial class Party
{
    public long Id { get; set; }

    public long? Pid { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public int SiteId { get; set; }
    public short Status { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;
    public long Uid { get; set; }

    public string Email { get; set; } = null!;

    public bool EmailConfirmed { get; set; }

    [StringLength(128)] public string NativeName { get; set; } = string.Empty;
    [StringLength(64)] public string FirstName { get; set; } = string.Empty;
    [StringLength(64)] public string LastName { get; set; } = string.Empty;
    [StringLength(20)] public string Language { get; set; } = string.Empty;
    [StringLength(255)] public string Avatar { get; set; } = string.Empty;
    [StringLength(30)] public string TimeZone { get; set; } = string.Empty;
    [StringLength(20)] public string ReferCode { get; set; } = string.Empty;
    [StringLength(10)] public string CountryCode { get; set; } = string.Empty;
    [StringLength(10)] public string Currency { get; set; } = string.Empty;
    [StringLength(10)] public string CCC { get; set; } = string.Empty;
    [StringLength(64)] public string PhoneNumber { get; set; } = string.Empty;

    public DateOnly Birthday { get; set; } = default;
    public int Gender { get; set; } = 0;
    [StringLength(32)] public string Citizen { get; set; } = string.Empty;
    [StringLength(512)] public string Address { get; set; } = string.Empty;
    public int IdType { get; set; } = 0;
    [StringLength(128)] public string IdNumber { get; set; } = string.Empty;
    [StringLength(128)] public string IdIssuer { get; set; } = string.Empty;
    [StringLength(128)] public string RegisteredIp { get; set; } = string.Empty;
    [StringLength(128)] public string LastLoginIp { get; set; } = string.Empty;
    public DateOnly IdIssuedOn { get; set; } = default;
    public DateOnly IdExpireOn { get; set; } = default;

    public string Note { get; set; } = null!;
    public string SearchText { get; set; } = string.Empty;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    public virtual ICollection<AccountReport> AccountReports { get; set; } = new List<AccountReport>();

    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<CommunicateHistory> CommunicateHistoryOperatorParties { get; set; } =
        new List<CommunicateHistory>();

    public virtual ICollection<CommunicateHistory> CommunicateHistoryParties { get; set; } =
        new List<CommunicateHistory>();

    public virtual ICollection<CopyTrade> CopyTrades { get; set; } = new List<CopyTrade>();

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();

    public virtual ICollection<Party> InversePidNavigation { get; set; } = new List<Party>();

    public virtual ICollection<Invoice> InvoiceRecipientNavigations { get; set; } = new List<Invoice>();

    public virtual ICollection<Invoice> InvoiceSenderNavigations { get; set; } = new List<Invoice>();

    public virtual ICollection<Ledger> Ledgers { get; set; } = new List<Ledger>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<NotificationSubscription> NotificationSubscriptions { get; set; } =
        new List<NotificationSubscription>();

    public virtual ICollection<PartyRole> PartyRoles { get; set; } = new List<PartyRole>();

    public virtual ICollection<PaymentInfo> PaymentInfos { get; set; } = new List<PaymentInfo>();

    public virtual ICollection<PaymentServiceAccess> PaymentServiceAccesses { get; set; } =
        new List<PaymentServiceAccess>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Party? PidNavigation { get; set; }

    public virtual ICollection<RebateBaseSchema> RebateBaseSchemas { get; set; } = new List<RebateBaseSchema>();

    public virtual ICollection<RebateDirectRule> RebateDirectRuleConfirmedByNavigations { get; set; } =
        new List<RebateDirectRule>();

    public virtual ICollection<RebateDirectRule> RebateDirectRuleCreatedByNavigations { get; set; } =
        new List<RebateDirectRule>();

    public virtual ICollection<RebateDirectSchema> RebateDirectSchemaConfirmedByNavigations { get; set; } =
        new List<RebateDirectSchema>();

    public virtual ICollection<RebateDirectSchema> RebateDirectSchemaCreatedByNavigations { get; set; } =
        new List<RebateDirectSchema>();

    public virtual ICollection<RebateSchemaBundle> RebateSchemaBundles { get; set; } = new List<RebateSchemaBundle>();

    public virtual ICollection<Rebate> Rebates { get; set; } = new List<Rebate>();

    public virtual ICollection<ReferralCode> ReferralCodes { get; set; } = new List<ReferralCode>();

    public virtual ICollection<Referral> ReferralReferredParties { get; set; } = new List<Referral>();

    public virtual ICollection<Referral> ReferralReferrerParties { get; set; } = new List<Referral>();

    public virtual ICollection<ReportRequest> ReportRequests { get; set; } = new List<ReportRequest>();

    public virtual Site Site { get; set; } = null!;

    public virtual ICollection<TradeDemoAccount> TradeDemoAccounts { get; set; } = new List<TradeDemoAccount>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();


    public virtual ICollection<Verification> Verifications { get; set; } = new List<Verification>();

    public virtual ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();

    public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<PartyTag> PartyTags { get; set; } = new List<PartyTag>();
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
    public virtual ICollection<Case> AdminCases { get; set; } = new List<Case>();
    public virtual ICollection<AdjustBatch> AdjustBatches { get; set; } = new List<AdjustBatch>();
    public virtual ICollection<AdjustRecord> AdjustRecords { get; set; } = new List<AdjustRecord>();

    public virtual ICollection<AccountLog> OperatedAccountLogs { get; set; } = new List<AccountLog>();

    public virtual ICollection<EventParty> EventParties { get; set; } = new List<EventParty>();

    // public virtual ICollection<EventAccount> OperatedEventAccounts { get; set; } = new List<EventAccount>();
    public virtual ICollection<EventParty> OperatedEventParties { get; set; } = new List<EventParty>();
    public virtual ICollection<EventShopOrder> OperatedEventShopOrders { get; set; } = new List<EventShopOrder>();
    public virtual ICollection<EventShopReward> OperatedEventShopRewards { get; set; } = new List<EventShopReward>();
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    public virtual ICollection<AccountAlias> AccountAliases { get; set; } = new List<AccountAlias>();

    // public virtual ICollection<PaymentMethodAccess> PaymentMethodAccesses { get; set; } =
    //     new List<PaymentMethodAccess>();

    public virtual ICollection<SalesRebateSchema> OperatedSalesRebateSchemata { get; set; } =
        new List<SalesRebateSchema>();

    public virtual ICollection<MessageRecord> MessageRecords { get; set; } = new List<MessageRecord>();
    public virtual ICollection<PartyComment> PartyComments { get; set; } = new List<PartyComment>();
    public virtual ICollection<PartyComment> OperatedPartyComments { get; set; } = new List<PartyComment>();
    public virtual ICollection<AccountComment> OperatedAccountComments { get; set; } = new List<AccountComment>();
    public virtual ICollection<Crypto> OperatedCryptos { get; set; } = new List<Crypto>();
    public virtual ICollection<LoginLog> LoginLogs { get; set; } = new List<LoginLog>();
    public virtual ICollection<AuthCode> AuthCodes { get; set; } = new List<AuthCode>();
    public virtual ICollection<AccountCheck> OperatedAccountChecks { get; set; } = new List<AccountCheck>();
    public virtual ICollection<Comment> OperatedComments { get; set; } = new List<Comment>();

    public virtual ICollection<AccountPaymentMethodAccess> OperatedAccountPaymentMethodAccesses { get; set; } =
        new List<AccountPaymentMethodAccess>();

    public virtual ICollection<WalletPaymentMethodAccess> OperatedWalletPaymentMethodAccesses { get; set; } =
        new List<WalletPaymentMethodAccess>();

    public virtual ICollection<Medium> Medium { get; set; } = new List<Medium>();

    public virtual ICollection<ApiLog> ApiLogs { get; set; } = new List<ApiLog>();
    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();

    public virtual ICollection<PaymentMethod> OperatedPaymentMethods { get; set; } = new List<PaymentMethod>();

    public virtual ICollection<AccountExtraRelation> OperatedAccountExtraRelations { get; set; } =
        new List<AccountExtraRelation>();

    public virtual ICollection<Symbol> OperatedSymbols { get; set; } = new List<Symbol>();

    public virtual ICollection<AccountReportGroup> OperatedAccountReportGroups { get; set; } =
        new List<AccountReportGroup>();

    public virtual ICollection<PayoutRecord> OperatedPayoutRecords { get; set; } =
        new List<PayoutRecord>();

    public virtual ICollection<Chat> CreatedChats { get; set; } = new List<Chat>();
    public virtual ICollection<ChatParticipant> ParticipatedChats { get; set; } = new List<ChatParticipant>();
    public virtual ICollection<ChatMessage> SentChatMessages { get; set; } = new List<ChatMessage>();
    public virtual ICollection<ChatMessageInbox> ChatMessageInboxes { get; set; } = new List<ChatMessageInbox>();
}