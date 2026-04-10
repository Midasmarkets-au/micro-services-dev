#nullable disable
using Bacera.Gateway.Core.Models.Tenant;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

public partial class TenantDbContext(DbContextOptions<TenantDbContext> options) : DbContext(options)
{
    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountComment> AccountComments { get; set; }
    public virtual DbSet<AccountDepositView> AccountDepositViews { get; set; }
    public virtual DbSet<AccountExtraRelation> AccountExtraRelations { get; set; }

    public virtual DbSet<AccountWithdrawalView> AccountWithdrawalViews { get; set; }

    public virtual DbSet<AccountTag> AccountTags { get; set; }

    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<TradeAccountLoginLog> TradeAccountLoginLogs { get; set; }
    public virtual DbSet<AccountAlias> AccountAliases { get; set; }
    public virtual DbSet<AccountLog> AccountLogs { get; set; }
    public virtual DbSet<AccountCheck> AccountChecks { get; set; }
    public virtual DbSet<AccountPoint> AccountPoints { get; set; }

    public virtual DbSet<AccountStat> AccountStats { get; set; }

    public virtual DbSet<AccountPointTransaction> AccountPointTransactions { get; set; }
    public virtual DbSet<AccountRebateView> AccountRebateViews { get; set; }
    public virtual DbSet<AccountTradeView> AccountTradeViews { get; set; }
    public virtual DbSet<AccountReport> AccountReports { get; set; }
    public virtual DbSet<AccountReportGroup> AccountReportGroups { get; set; }
    public virtual DbSet<AccountReportGroupLogin> AccountReportGroupLogins { get; set; }
    public virtual DbSet<AccountPaymentMethodAccess> AccountPaymentMethodAccesses { get; set; }

    public virtual DbSet<Action> Actions { get; set; }

    public virtual DbSet<Activity> Activities { get; set; }
    public virtual DbSet<AdjustBatch> AdjustBatches { get; set; }
    public virtual DbSet<AdjustRecord> AdjustRecords { get; set; }
    public virtual DbSet<ApiLog> ApiLogs { get; set; }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<Audit> Audits { get; set; }
    public virtual DbSet<AuthCode> AuthCodes { get; set; }

    public virtual DbSet<Case> Cases { get; set; }
    public virtual DbSet<CaseCategory> CaseCategories { get; set; }

    public virtual DbSet<CaseLanguage> CaseLanguages { get; set; }
    public virtual DbSet<Charge> Charges { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }
    public virtual DbSet<ChatMessage> ChatMessages { get; set; }
    public virtual DbSet<ChatMessageInbox> ChatMessageInboxes { get; set; }
    public virtual DbSet<ChatParticipant> ChatParticipants { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<CommunicateHistory> CommunicateHistories { get; set; }

    public virtual DbSet<Configuration> Configurations { get; set; }

    public virtual DbSet<ConfigurationSnapshot> ConfigurationSnapshots { get; set; }

    public virtual DbSet<ContactRequest> ContactRequests { get; set; }

    public virtual DbSet<CopyTrade> CopyTrades { get; set; }

    public virtual DbSet<Crypto> Cryptos { get; set; }

    public virtual DbSet<CryptoTransaction> CryptoTransactions { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Deposit> Deposits { get; set; }

    public virtual DbSet<ExchangeRate> ExchangeRates { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    // public virtual DbSet<EventAccount> EventAccounts { get; set; }
    public virtual DbSet<EventParty> EventParties { get; set; }
    public virtual DbSet<EventShopClientPoint> EventShopClientPoints { get; set; }
    public virtual DbSet<EventLanguage> EventLanguages { get; set; }
    public virtual DbSet<EventShopItem> EventShopItems { get; set; }
    public virtual DbSet<EventShopItemLanguage> EventShopItemLanguages { get; set; }
    public virtual DbSet<EventShopOrder> EventShopOrders { get; set; }
    public virtual DbSet<EventShopPoint> EventShopPoints { get; set; }
    public virtual DbSet<EventShopPointTransaction> EventShopPointTransactions { get; set; }
    public virtual DbSet<EventShopReward> EventShopRewards { get; set; }
    public virtual DbSet<EventShopRewardRebate> EventShopRewardRebates { get; set; }
    public virtual DbSet<FundType> FundTypes { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }
    public virtual DbSet<Lead> Leads { get; set; }

    public virtual DbSet<Ledger> Ledgers { get; set; }
    public virtual DbSet<LoginLog> LoginLogs { get; set; }
    public virtual DbSet<Matter> Matters { get; set; }

    public virtual DbSet<MatterType> MatterTypes { get; set; }

    public virtual DbSet<Medium> Media { get; set; }

    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<MessageRecord> MessageRecords { get; set; }

    public virtual DbSet<NotificationEvent> NotificationEvents { get; set; }

    public virtual DbSet<NotificationSubscription> NotificationSubscriptions { get; set; }

    public virtual DbSet<Party> Parties { get; set; }
    public virtual DbSet<PartyComment> PartyComments { get; set; }
    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<PartyRole> PartyRoles { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentInfo> PaymentInfos { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    // public virtual DbSet<PaymentMethodAccess> PaymentMethodAccesses { get; set; }
    public virtual DbSet<PaymentService> PaymentServices { get; set; }

    public virtual DbSet<PaymentServiceAccess> PaymentServiceAccesses { get; set; }
    public virtual DbSet<PayoutRecord> PayoutRecords { get; set; }

    // public virtual DbSet<Permission> Permissions { get; set; }
    //
    // public virtual DbSet<PermissionAccess> PermissionAccesses { get; set; }

    public virtual DbSet<Rebate> Rebates { get; set; }
    public virtual DbSet<RebateNew> RebateNews { get; set; }

    public virtual DbSet<RebateAgentRule> RebateAgentRules { get; set; }

    public virtual DbSet<RebateBaseSchema> RebateBaseSchemas { get; set; }

    public virtual DbSet<RebateBaseSchemaItem> RebateBaseSchemaItems { get; set; }

    public virtual DbSet<RebateBrokerRule> RebateBrokerRules { get; set; }

    public virtual DbSet<RebateClientRule> RebateClientRules { get; set; }

    public virtual DbSet<RebateDirectRule> RebateDirectRules { get; set; }

    public virtual DbSet<RebateDirectSchema> RebateDirectSchemas { get; set; }

    public virtual DbSet<RebateDirectSchemaItem> RebateDirectSchemaItems { get; set; }

    public virtual DbSet<RebateSchemaBundle> RebateSchemaBundles { get; set; }

    public virtual DbSet<Referral> Referrals { get; set; }

    public virtual DbSet<ReferralCode> ReferralCodes { get; set; }

    public virtual DbSet<ReportRequest> ReportRequests { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SalesRebate> SalesRebates { get; set; }

    public virtual DbSet<SalesRebateSchema> SalesRebateSchemas { get; set; }
    public virtual DbSet<Site> Sites { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<Supplement> Supplements { get; set; }

    public virtual DbSet<Symbol> Symbols { get; set; }
    public virtual DbSet<Topic> Topics { get; set; }
    
    public virtual DbSet<TopicContent> TopicContents { get; set; }

    public virtual DbSet<TradeAccount> TradeAccounts { get; set; }

    public virtual DbSet<TradeAccountPassword> TradeAccountPasswords { get; set; }
    
    public virtual DbSet<TradeAccountPasswordHistory> TradeAccountPasswordHistories { get; set; }

    public virtual DbSet<TradeAccountStatus> TradeAccountStatuses { get; set; }

    public virtual DbSet<TradeDemoAccount> TradeDemoAccounts { get; set; }

    public virtual DbSet<TradeRebate> TradeRebates { get; set; }

    public virtual DbSet<TradeService> TradeServices { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }
    public virtual DbSet<Refund> Refunds { get; set; }

    public virtual DbSet<TransferView> TransferViews { get; set; }

    public virtual DbSet<Transition> Transitions { get; set; }

    public virtual DbSet<Verification> Verifications { get; set; }

    public virtual DbSet<VerificationItem> VerificationItems { get; set; }

    public virtual DbSet<WalletAdjust> WalletAdjusts { get; set; }
    public virtual DbSet<WalletDailySnapshot> WalletDailySnapshots { get; set; }
    public virtual DbSet<WalletPaymentMethodAccess> WalletPaymentMethodAccesses { get; set; }
    public virtual DbSet<Wallet> Wallets { get; set; }

    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }

    public virtual DbSet<Withdrawal> Withdrawals { get; set; }

    public virtual DbSet<MoveData> MoveData { get; set; }

    public virtual DbSet<Document> Documents { get; set; }
    public virtual DbSet<HistoricalDocument> HistoricalDocuments { get; set; }
    public virtual DbSet<ContractSpec> ContractSpecs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MoveData>(entity =>
        {
            entity.ToTable("MoveData", "public");

            entity.Property(e => e.Data).HasDefaultValueSql("'{}'::json")
                .HasColumnType("json");
        });
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("accounts_pkey");

            entity.ToTable("_Account", "trd");

            entity.HasIndex(e => e.AgentAccountId, "IX__Account_Aid");

            entity.HasIndex(e => e.BrokerAccountId, "IX__Account_BrokerAccountId");

            entity.HasIndex(e => e.CurrencyId, "IX__Account_CurrencyId");

            entity.HasIndex(e => e.FundType, "IX__Account_FundType");

            entity.HasIndex(e => e.ReferrerAccountId, "IX_accounts_refer_id");

            entity.HasIndex(e => e.SalesAccountId, "IX_accounts_sales_id");

            entity.HasIndex(e => e.Code, "_Account_Code_index");

            entity.HasIndex(e => e.IsClosed, "_Account_IsClosed_index");

            entity.HasIndex(e => e.Level, "_Account_Level_index");
            entity.HasIndex(e => e.ReferPath, "_Account_ReferPath_index");

            entity.HasIndex(e => e.Uid, "_Account_Uid_ux").IsUnique();

            entity.HasIndex(e => e.Group, "trd_accounts_group_index");

            entity.HasIndex(e => e.PartyId, "trd_accounts_party_id_index");

            entity.HasIndex(e => e.HasLevelRule, "trd_accounts_has_level_rule_index");
            entity.HasIndex(e => e.SearchText, "_Account_search_text_index");
            entity.HasIndex(e => e.Permission, "_Account_permission_index");
            entity.HasIndex(e => e.SuspendedOn, "_Account_SuspendedOn_index");
            entity.HasIndex(e => e.ActiveOn, "_Account_ActiveOn_index");

            entity.HasIndex(e => e.SiteId, "_Account_SiteId_index");

            entity.Property(e => e.Id).HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.CurrencyId).HasDefaultValueSql("'-1'::integer");
            entity.Property(e => e.Group).HasMaxLength(60);
            entity.Property(e => e.Name).HasMaxLength(128);
            entity.Property(e => e.ReferCode).HasMaxLength(20);

            entity.Property(e => e.Permission)
                .HasMaxLength(30)
                .HasDefaultValueSql("'11111'::character varying");
            entity.Property(e => e.Role).HasDefaultValueSql("'0'::smallint");
            entity.Property(e => e.SiteId).HasDefaultValueSql("'0'::integer");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.AgentAccount).WithMany(p => p.InverseAgentAccount)
                .HasForeignKey(d => d.AgentAccountId)
                .HasConstraintName("_Account__Agent_Id_fk");

            entity.HasOne(d => d.BrokerAccount).WithMany(p => p.InverseBrokerAccount)
                .HasForeignKey(d => d.BrokerAccountId)
                .HasConstraintName("_Account__Account_Id_fk");

            entity.HasOne(d => d.Currency).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Account__Currency_Id_fk");

            entity.HasOne(d => d.FundTypeNavigation).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.FundType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Account__FundType_Id_fk");

            entity.HasOne(d => d.Party).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trd_accounts_party_id_foreign");

            entity.HasOne(d => d.ReferrerAccount).WithMany(p => p.InverseReferrerAccount)
                .HasForeignKey(d => d.ReferrerAccountId)
                .HasConstraintName("trd_accounts_refer_id_foreign");

            entity.HasOne(d => d.SalesAccount).WithMany(p => p.InverseSalesAccount)
                .HasForeignKey(d => d.SalesAccountId)
                .HasConstraintName("trd_accounts_sales_id_foreign");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.WalletId)
                .HasConstraintName("trd_accounts_wallet_id_foreign");

            entity.HasMany(d => d.Groups).WithMany(p => p.Accounts)
                .UsingEntity<Dictionary<string, object>>(
                    "AccountGroup",
                    r => r.HasOne<Group>().WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("_AccountGroup__Group_Id_fk"),
                    l => l.HasOne<Account>().WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("_AccountGroup__Accounts_Id_fk"),
                    j =>
                    {
                        j.HasKey("AccountId", "GroupId").HasName("_AccountGroup_pk");
                        j.ToTable("_AccountGroup", "trd");
                        j.HasIndex(new[] { "GroupId" }, "IX__AccountGroup_GroupId");
                    });

            entity.HasMany(d => d.AccountTags).WithMany(p => p.Accounts)
                .UsingEntity<Dictionary<string, object>>(
                    "AccountHasTag",
                    r => r.HasOne<AccountTag>().WithMany()
                        .HasForeignKey("AccountTagId")
                        .HasConstraintName("_AccountHasTag__AccountTag_Id_fk"),
                    l => l.HasOne<Account>().WithMany()
                        .HasForeignKey("AccountId")
                        .HasConstraintName("_AccountHasTag__Account_Id_fk"),
                    j =>
                    {
                        j.HasKey("AccountId", "AccountTagId").HasName("_AccountHasTag_pk");
                        j.ToTable("_AccountHasTag", "trd");
                    });
        });

        modelBuilder.Entity<AccountComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_AccountComment_pk");
            entity.ToTable("_AccountComment", "trd");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Account)
                .WithMany(p => p.AccountComments)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("_AccountComment__Account_Id_fk");

            entity.HasOne(d => d.OperatorParty)
                .WithMany(p => p.OperatedAccountComments)
                .HasForeignKey(d => d.OperatorPartyId)
                .HasConstraintName("_AccountComment__OperatorParty_Id_fk");
        });

        modelBuilder.Entity<AccountDepositView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("AccountDeposit", "acct");
        });

        modelBuilder.Entity<AccountExtraRelation>(entity =>
        {
            entity.HasKey(e => new { e.ParentAccountId, e.ChildAccountId }).HasName("account_extra_relations_pkey");
            entity.ToTable("_AccountExtraRelation", "trd");

            entity.HasIndex(e => e.OperatorPartyId, "IX__AccountExtraRelation_OperatorPartyId");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.ParentAccount).WithMany(p => p.ParentAccountExtraRelations)
                .HasForeignKey(d => d.ParentAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountExtraRelation__Parent_Account_Id_fk");

            entity.HasOne(d => d.ChildAccount).WithMany(p => p.ChildAccountExtraRelations)
                .HasForeignKey(d => d.ChildAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountExtraRelation__Child_Account_Id_fk");

            entity.HasOne(d => d.OperatorParty).WithMany(p => p.OperatedAccountExtraRelations)
                .HasForeignKey(d => d.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountExtraRelation__Operator_Party_Id_fk");
        });

        modelBuilder.Entity<AccountWithdrawalView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("AccountWithdrawal", "acct");
        });

        modelBuilder.Entity<AccountTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_AccountTag_pk");

            entity.ToTable("_AccountTag", "trd");

            entity.HasIndex(e => e.Name, "_AccountTag_Name_ux").IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasDefaultValueSql("''::character varying");
        });

        modelBuilder.Entity<AccountAlias>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_AccountAlias_pk");

            entity.ToTable("_AccountAlias", "trd");

            entity.HasIndex(e => e.PartyId, "IX__AccountAlias_PartyId");
            entity.HasIndex(e => e.AccountId, "IX__AccountAlias_AccountId");
            entity.HasIndex(e => new { e.PartyId, e.AccountId }, " IX__AccountAlias_PartyId_AccountId_UX")
                .IsUnique();
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party).WithMany(p => p.AccountAliases)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountAlias__Party_Id_fk");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountAliases)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountAlias__Account_Id_fk");
        });

        modelBuilder.Entity<AccountLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_AccountLog_pk");

            entity.ToTable("_AccountLog", "trd");
            entity.HasIndex(e => e.AccountId, "IX__AccountLog_AccountId");
            entity.HasIndex(e => e.OperatorPartyId, "IX__AccountLog_OperatorPartyId");
            entity.HasIndex(e => e.CreatedOn, "IX__AccountLog_CreatedOn");
            entity.HasIndex(e => e.Action, "IX__AccountLog_Action");

            entity.Property(e => e.OperatorPartyId).HasDefaultValueSql("1");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountLogs)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountLog__Account_Id_fk");

            entity.HasOne(d => d.OperatorParty).WithMany(p => p.OperatedAccountLogs)
                .HasForeignKey(d => d.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountLog__Operator_Party_Id_fk");
        });

        modelBuilder.Entity<AccountCheck>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_AccountCheck_pk");
            entity.ToTable("_AccountCheck", "trd");

            entity.HasIndex(e => e.OperatorPartyId, "IX__AccountCheck_OperatorPartyId");
            entity.HasIndex(e => e.Type, "IX__AccountCheck_Type");
            entity.HasIndex(e => e.Status, "IX__AccountCheck_Status");
            entity.HasIndex(e => e.CreatedOn, "IX__AccountCheck_CreatedOn");
            entity.HasIndex(e => e.UpdatedOn, "IX__AccountCheck_UpdatedOn");

            entity.Property(e => e.Name).HasMaxLength(256).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.AccountNumberContent).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.OperatorParty).WithMany(p => p.OperatedAccountChecks)
                .HasForeignKey(d => d.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountCheck__Operator_Party_Id_fk");
        });
        // ib001testib001testib001testib001testib001testib001111111111
        modelBuilder.Entity<AccountPoint>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("_AccountPoint_pk");

            entity.ToTable("_AccountPoint", "trd");

            entity.Property(e => e.AccountId).ValueGeneratedNever();
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Account).WithOne(p => p.AccountPoint)
                .HasForeignKey<AccountPoint>(d => d.AccountId)
                .HasConstraintName("_AccountPoint__Account_Id_fk");
        });

        modelBuilder.Entity<AccountStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_AccountStat_pk");

            entity.ToTable("_AccountStat", "trd");
            entity.HasIndex(e => e.AccountId, "IX__AccountStat_AccountId");
            entity.Property(e => e.Date).HasColumnType("date");
            entity.HasIndex(e => new { e.AccountId, e.Date }, "IX_AccountStats_AccountId_Date").IsUnique();

            entity.HasOne(d => d.Account)
                .WithMany(d => d.AccountStats)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountStat__Account_Id_fk");
        });

        modelBuilder.Entity<AccountPointTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_AccountPointTransaction_pk");

            entity.ToTable("_AccountPointTransaction", "trd");

            entity.HasIndex(e => e.AccountId, "IX__AccountPointTransaction_AccountId");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Note)
                .HasMaxLength(1000)
                .HasDefaultValueSql("''::character varying");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountPointTransactions)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountPointTransaction__Account_Id_fk");
        });

        modelBuilder.Entity<AccountRebateView>(entity =>
        {
            entity.HasNoKey()
                .ToView("AccountRebate", "trd");
        });

        modelBuilder.Entity<AccountTradeView>(entity =>
        {
            entity.HasNoKey()
                .ToView("AccountTrade", "trd");
        });

        modelBuilder.Entity<AccountReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_AccountReport_pk");
            entity.ToTable("_AccountReport", "trd");

            entity.HasIndex(e => e.TenantId, "IX__AccountReport_TenantId");
            entity.HasIndex(e => e.ServiceId, "IX__AccountReport_ServiceId");
            entity.HasIndex(e => e.AccountId, "IX__AccountReport_AccountId");
            entity.HasIndex(e => e.AccountNumber, "IX__AccountReport_AccountNumber");
            entity.HasIndex(e => e.Date, "IX__AccountReport_Date");
            entity.HasIndex(e => e.Status, "IX__AccountReport_Status");
            entity.HasIndex(e => e.TryTime, "IX__AccountReport_TryTime");
            entity.HasIndex(e => e.Tries, "IX__AccountReport_Tries");
            entity.HasIndex(e => e.Type, "IX__AccountReport_Type");

            entity.HasIndex(e => new { e.AccountNumber, e.ServiceId, e.Date },
                "IX_AccountNumber_ServiceId_Date_Unique").IsUnique();

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountReports)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountReport__Account_Id_fk");

            entity.HasOne(d => d.Party).WithMany(p => p.AccountReports)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountReport__Party_Id_fk");
        });

        modelBuilder.Entity<AccountReportGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_AccountReportGroup_pk");
            entity.ToTable("_AccountReportGroup", "trd");

            entity.HasIndex(e => e.OperatorPartyId, "IX__AccountReportGroup_OperatorPartyId");
            entity.HasIndex(e => e.Group, "IX__AccountReportGroup_Group");
            entity.HasIndex(e => e.Category, "IX__AccountReportGroup_Category");

            entity.Property(e => e.Group).HasMaxLength(20);
            entity.Property(e => e.Category).HasMaxLength(20);
            entity.Property(e => e.MetaData).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.OperatorParty).WithMany(p => p.OperatedAccountReportGroups)
                .HasForeignKey(d => d.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountReportGroup__Operator_Party_Id_fk");

            entity.HasOne(d => d.Parent).WithMany(p => p.Children)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountReportGroup__Parent_Id_fk");
        });

        modelBuilder.Entity<AccountReportGroupLogin>(entity =>
        {
            entity.HasKey(e => new { e.AccountReportGroupId, e.Login }).HasName("account_report_group_logins_pkey");
            entity.ToTable("_AccountReportGroupLogin", "trd");

            entity.HasIndex(e => e.Login, "IX__AccountReportGroupLogin_Login");
            entity.HasIndex(e => e.AccountReportGroupId, "IX__AccountReportGroupLogin_AccountReportGroupId");

            entity.HasOne(d => d.AccountReportGroup).WithMany(p => p.AccountReportGroupLogins)
                .HasForeignKey(d => d.AccountReportGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AccountReportGroupLogin__AccountReportGroup_Id_fk");
        });

        modelBuilder.Entity<AccountPaymentMethodAccess>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.PaymentMethodId }).HasName("account_payment_method_access_pkey");
            entity.ToTable("_AccountPaymentMethodAccess", "acct");

            entity.HasIndex(e => e.Status, "IX_account_payment_method_access_status");
            entity.HasIndex(e => new { e.AccountId, e.PaymentMethodId },
                    "IX_account_payment_method_access_accountId_PaymentMethodId")
                .IsUnique();

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.AccountPaymentMethodAccesses)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_account_payment_method_access_payment_method_id_foreign");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountPaymentMethodAccesses)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_account_payment_method_access_account_id_foreign");

            entity.HasOne(d => d.OperatedParty).WithMany(p => p.OperatedAccountPaymentMethodAccesses)
                .HasForeignKey(d => d.OperatedPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_account_payment_method_access_operated_party_id_foreign");
        });

        modelBuilder.Entity<Action>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("actions_pkey");

            entity.ToTable("_Action", "core");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(64);
        });

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ix_activity_k8s_id");

            entity.ToTable("activity_k8s", "core");

            entity.HasIndex(e => e.ActionId, "ix_activity_k8s_action_id");

            entity.HasIndex(e => e.OnStateId, "ix_activity_k8s_on_state_id");

            entity.HasIndex(e => e.PartyId, "ix_activity_k8s_party_id");

            entity.HasIndex(e => e.ToStateId, "ix_activity_k8s_to_state_id");

            entity.HasIndex(e => e.MatterId, "ix_activity_k8s_matter_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MatterId).HasColumnName("matter_id");
            entity.Property(e => e.ActionId).HasColumnName("action_id");
            entity.Property(e => e.OnStateId).HasColumnName("on_state_id");
            entity.Property(e => e.ToStateId).HasColumnName("to_state_id");
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.PerformedOn).HasColumnName("performed_on").HasDefaultValueSql("now()");
            entity.Property(e => e.Data).HasColumnName("data").HasMaxLength(255);

            entity.HasOne(d => d.Matter).WithMany(p => p.Activities)
                .HasForeignKey(d => d.MatterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_activity_k8s_matter_id");

            entity.HasOne(d => d.Party).WithMany(p => p.Activities)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_activity_k8s_party_id");
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Address_pk");
            entity.ToTable("_Address", "core");

            entity.HasIndex(e => e.PartyId, "IX__Address_PartyId");
            entity.HasIndex(e => e.Name, "IX__Address_Name");
            entity.HasIndex(e => e.CCC, "IX__Address_CCC");
            entity.HasIndex(e => e.Phone, "IX__Address_Phone");
            entity.HasIndex(e => e.Country, "IX__Address_Country");
            entity.HasIndex(e => e.CreatedOn, "IX__Address_CreatedOn");
            entity.HasIndex(e => e.UpdatedOn, "IX__Address_UpdatedOn");
            entity.HasIndex(e => e.DeletedOn, "IX__Address_DeletedOn");

            entity.HasOne(d => d.Party)
                .WithMany(p => p.Addresses)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("_Address__Party_Id_fk");
        });

        modelBuilder.Entity<AdjustBatch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_AdjustBatch_pk");

            entity.ToTable("_AdjustBatch", "trd");

            entity.HasIndex(e => e.ServiceId, "IX_AdjustBatch_ServiceId");
            entity.HasIndex(e => e.Type, "IX_AdjustBatch_Type");
            entity.HasIndex(e => e.OperatorPartyId, "IX_AdjustBatch_OperatorPartyId");
            entity.HasIndex(e => e.Status, "IX_AdjustBatch_Status");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Result).HasDefaultValueSql("'{}'::json").HasColumnType("json");

            entity.HasOne(d => d.OperatorParty).WithMany(p => p.AdjustBatches)
                .HasForeignKey(d => d.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AdjustBatch__Operator_Party_Id_fk");

            entity.HasMany(d => d.AdjustRecords).WithOne(p => p.AdjustBatch)
                .HasForeignKey(d => d.AdjustBatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AdjustBatch__AdjustRecord_Id_fk");
        });

        modelBuilder.Entity<AdjustRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_AdjustRecord_pk");

            entity.ToTable("_AdjustRecord", "trd");

            entity.HasIndex(e => e.AdjustBatchId, "IX_AdjustRecord_AdjustBatchId");
            entity.HasIndex(e => e.AccountNumber, "IX_AdjustRecord_AccountNumber");
            entity.HasIndex(e => e.AccountId, "IX_AdjustRecord_AccountId");
            entity.HasIndex(e => e.Type, "IX_AdjustRecord_Type");
            entity.HasIndex(e => e.Ticket, "IX_AdjustRecord_Ticket");
            entity.HasIndex(e => e.Status, "IX_AdjustRecord_Status");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
            entity.HasOne(d => d.Account).WithMany(p => p.AdjustRecords)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AdjustRecord__Account_Id_fk");

            entity.HasOne(d => d.OperatorParty).WithMany(p => p.AdjustRecords)
                .HasForeignKey(d => d.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AdjustRecord__Operator_Party_Id_fk");
        });

        modelBuilder.Entity<ApiLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("apilogs_pkey");
            entity.ToTable("_ApiLog", "core");
            entity.HasIndex(e => e.Method, "_ApiLog_Method_index");
            entity.HasIndex(e => e.PartyId, "_ApiLog_PartyId_index");
            entity.HasIndex(e => e.Ip, "_ApiLog_Ip_index");
            entity.HasIndex(e => e.Action, "_ApiLog_Action_index");
            entity.HasIndex(e => e.StatusCode, "_ApiLog_StatusCode_index");
            entity.HasIndex(e => e.ConnectionId, "_ApiLog_ConnectionId_index");

            entity.Property(e => e.Method).HasMaxLength(12).HasDefaultValueSql("''::character varying")
                .HasColumnType("character varying");
            entity.Property(e => e.Action).HasMaxLength(256).HasDefaultValueSql("''::character varying")
                .HasColumnType("character varying");
            entity.Property(e => e.ConnectionId).HasMaxLength(256).HasDefaultValueSql("''::character varying")
                .HasColumnType("character varying");

            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party).WithMany(p => p.ApiLogs)
                .HasForeignKey(d => d.PartyId);
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("account_applications_pkey");

            entity.ToTable("_Application", "trd");

            entity.HasIndex(e => e.PartyId, "trd_account_applications_party_id_index");

            entity.Property(e => e.Id).HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.RejectedReason).HasMaxLength(255);
            entity.Property(e => e.Supplement).HasColumnType("json");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party).WithMany(p => p.Applications)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trd_account_applications_party_id_foreign");
        });

        modelBuilder.Entity<Audit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Audit_pk");

            entity.ToTable("_Audit", "core");

            entity.HasIndex(e => e.PartyId, "_Audit_PartyId_index");

            entity.HasIndex(e => new { e.Type, e.RowId }, "_Audit_Type_RowId_index");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
        });


        modelBuilder.Entity<AuthCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_AuthCode_pk");

            entity.ToTable("_AuthCode", "core");

            entity.HasIndex(e => e.PartyId, "_AuthCode_PartyId_index");
            entity.HasIndex(e => e.Code, "_AuthCode_Code_index");
            entity.HasIndex(e => e.Method, "_AuthCode_Method_index");
            entity.HasIndex(e => e.MethodValue, "_AuthCode_MethodValue_index");
            entity.HasIndex(e => e.Status, "_AuthCode_Status_index");
            entity.HasIndex(e => new { e.Method, e.MethodValue }, "_AuthCode_Method_MethodValue_index");

            entity.Property(e => e.Code)
                .HasMaxLength(64)
                .HasDefaultValueSql("''::character varying")
                .HasColumnType("character varying");

            entity.Property(e => e.Method)
                .HasMaxLength(20)
                .HasDefaultValueSql("''::character varying")
                .HasColumnType("character varying");

            entity.Property(e => e.MethodValue)
                .HasMaxLength(32)
                .HasDefaultValueSql("''::character varying")
                .HasColumnType("character varying");

            entity.Property(e => e.Event)
                .HasMaxLength(20)
                .HasDefaultValueSql("''::character varying")
                .HasColumnType("character varying");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.ExpireOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party).WithMany(p => p.AuthCodes)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AuthCode__Party_Id_fk");
        });

        modelBuilder.Entity<Charge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("charges_pkey");

            entity.ToTable("_Charge", "acct");

            entity.HasIndex(e => e.Apcode, "acct_charges_ap_code_unique").IsUnique();

            entity.HasIndex(e => e.Arcode, "acct_charges_ar_code_unique").IsUnique();

            entity.HasIndex(e => e.Code, "acct_charges_code_unique").IsUnique();

            entity.HasIndex(e => e.Name, "acct_charges_name_unique").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Apcode)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("APCode");
            entity.Property(e => e.Arcode)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("ARCode");
            entity.Property(e => e.Code)
                .HasMaxLength(12)
                .IsFixedLength();
            entity.Property(e => e.Name).HasMaxLength(64);
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Chat_pk");
            entity.ToTable("_Chat", "chat");
            entity.HasIndex(e => e.CreatorPartyId, "IX__Chat_CreatorPartyId");
            entity.HasIndex(e => e.Status, "IX__Chat_Status");
            entity.HasIndex(e => e.CreatedOn, "IX__Chat_CreatedOn");

            entity.Property(e => e.Name).HasMaxLength(64);


            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Token).HasMaxLength(50);
            entity.Property(e => e.MetaData).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.CreatorParty).WithMany(p => p.CreatedChats)
                .HasForeignKey(d => d.CreatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Chat__Creator_Party_Id_fk");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_ChatMessage_pk");
            entity.ToTable("_ChatMessage", "chat");
            entity.HasIndex(e => e.ChatId, "IX__ChatMessage_ChatId");
            entity.HasIndex(e => e.SenderPartyId, "IX__ChatMessage_SenderPartyId");
            entity.HasIndex(e => e.CreatedOn, "IX__ChatMessage_CreatedOn");

            entity.Property(e => e.Content).HasDefaultValueSql("''::text");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Chat).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_ChatMessage__Chat_Id_fk");

            entity.HasOne(d => d.SenderParty).WithMany(p => p.SentChatMessages)
                .HasForeignKey(d => d.SenderPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_ChatMessage__Sender_Party_Id_fk");
        });

        modelBuilder.Entity<ChatMessageInbox>(entity =>
        {
            entity.HasKey(e => new { e.ReceiverPartyId, e.ChatMessageId }).HasName("_ChatMessageInbox_pk");
            entity.ToTable("_ChatMessageInbox", "chat");

            entity.HasIndex(e => e.ChatMessageId, "IX__ChatMessageInbox_ChatMessageId");
            entity.HasIndex(e => e.ReceiverPartyId, "IX__ChatMessageInbox_ReceiverPartyId");

            entity.Property(e => e.DeliveredOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.ChatMessage).WithMany(p => p.ChatMessageInboxes)
                .HasForeignKey(d => d.ChatMessageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_ChatMessageInbox__ChatMessage_Id_fk");

            entity.HasOne(d => d.ReceiverParty).WithMany(p => p.ChatMessageInboxes)
                .HasForeignKey(d => d.ReceiverPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_ChatMessageInbox__Receiver_Party_Id_fk");
        });

        modelBuilder.Entity<ChatParticipant>(entity =>
        {
            entity.HasKey(e => new { e.ChatId, e.PartyId }).HasName("_ChatParticipant_pk");
            entity.ToTable("_ChatParticipant", "chat");

            entity.HasIndex(e => e.ChatId, "IX__ChatParticipant_ChatId");
            entity.HasIndex(e => e.PartyId, "IX__ChatParticipant_PartyId");

            entity.HasOne(d => d.Chat).WithMany(p => p.Participants)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_ChatParticipant__Chat_Id_fk");

            entity.HasOne(d => d.Party).WithMany(p => p.ParticipatedChats)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_ChatParticipant__Party_Id_fk");
        });
        
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Comment_pk");

            entity.ToTable("_Comment", "core");

            entity.HasIndex(e => new { e.Type, e.RowId }, "_Comment_Type_RowId_index");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party).WithMany(p => p.OperatedComments)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Comment__Party_Id_fk");
        });

        modelBuilder.Entity<CommunicateHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_CommunicateHistory_pk");

            entity.ToTable("_CommunicateHistory", "cms");

            entity.HasIndex(e => e.OperatorPartyId, "IX__CommunicateHistory_OperatorPartyId");

            entity.HasIndex(e => e.PartyId, "IX__CommunicateHistory_PartyId");

            entity.HasIndex(e => e.CreatedOn, "_CommunicateHistory_CreatedOn_index");

            entity.HasIndex(e => e.Type, "_CommunicateHistory_Type_index");

            entity.Property(e => e.Content).HasDefaultValueSql("''::text");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.OperatorPartyId).HasDefaultValueSql("1");

            entity.HasOne(d => d.OperatorParty).WithMany(p => p.CommunicateHistoryOperatorParties)
                .HasForeignKey(d => d.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_CommunicateHistory__Operator_Party_Id_fk");

            entity.HasOne(d => d.Party).WithMany(p => p.CommunicateHistoryParties)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_CommunicateHistory__Party_Id_fk");
        });

        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Configuration_pk");

            entity.ToTable("_Configuration", "core");

            entity.HasIndex(e => e.RowId, "IX__Configuration_RowId");
            entity.HasIndex(e => e.Key, "IX__Configuration_Key");
            entity.HasIndex(e => e.Category, "IX__Configuration_Category");
            entity.HasIndex(e => e.DataFormat, "IX__Configuration_DataFormat");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Name).HasMaxLength(32);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<ConfigurationSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_ConfigurationSnapshot_pk");

            entity.ToTable("_ConfigurationSnapshot", "core");

            entity.HasIndex(e => new { e.PartyId, e.SnapshotId }, "IX_ConfigurationSnapshot_UserId_SnapshotId");
            entity.HasIndex(e => new { e.ConfigName, e.RowId }, "IX_ConfigurationSnapshot_ConfigName_RowId");
            entity.HasIndex(e => e.CreatedOn, "IX_ConfigurationSnapshot_CreatedOn");

            entity.Property(e => e.SnapshotId).HasMaxLength(36);
            entity.Property(e => e.ConfigName).HasMaxLength(32);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.LastActivity).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<ContactRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_ContactRequest_pk");

            entity.ToTable("_ContactRequest", "cms");

            entity.Property(e => e.Content).HasDefaultValueSql("''::text");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Ip)
                .HasMaxLength(20)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(64)
                .HasDefaultValueSql("''::character varying");
        });

        modelBuilder.Entity<CopyTrade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_CopyTrade_pk");

            entity.ToTable("_CopyTrade", "trd");

            entity.HasIndex(e => e.SourceAccountId, "IX__CopyTrade_SourceAccountId");

            entity.HasIndex(e => e.TargetAccountId, "IX__CopyTrade_TargetAccountId");

            entity.HasIndex(e => e.PartyId, "_CopyTrade_PartyId_index");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Mode).HasMaxLength(4);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party).WithMany(p => p.CopyTrades)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_CopyTrade__Party_Id_fk");

            entity.HasOne(d => d.SourceAccount).WithMany(p => p.CopyTradeSourceAccounts)
                .HasForeignKey(d => d.SourceAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_CopyTrade__TradeAccount_Id_Source_fk");

            entity.HasOne(d => d.TargetAccount).WithMany(p => p.CopyTradeTargetAccounts)
                .HasForeignKey(d => d.TargetAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_CopyTrade__TradeAccount_Id_Target_fk");
        });

        modelBuilder.Entity<Crypto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("crypto_pkey");

            entity.ToTable("_Crypto", "acct");

            entity.HasIndex(e => e.Address, "acct_crypto_address_unique").IsUnique();
            entity.HasIndex(e => e.Status, "acct_crypto_status_index");
            entity.HasIndex(e => e.Type, "acct_crypto_type_index");
            entity.HasIndex(e => e.IsDeleted, "IX__Crypto_IsDeleted");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Address).HasMaxLength(256);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.OperatorParty).WithMany(p => p.OperatedCryptos)
                .HasForeignKey(d => d.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_crypto_operator_party_id_foreign");

            entity.HasOne(d => d.InUsePayment).WithOne(p => p.InUseCrypto)
                .HasForeignKey<Crypto>(d => d.InUsePaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_crypto_in_use_payment_id_foreign");
        });

        modelBuilder.Entity<CryptoTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("crypto_transactions_pkey");

            entity.ToTable("_CryptoTransaction", "acct");

            entity.HasIndex(e => e.Confirmed, "acct_crypto_transactions_confirmed_index");
            entity.HasIndex(e => e.FromAddress, "acct_crypto_transactions_from_address_index");
            entity.HasIndex(e => e.Status, "acct_crypto_transactions_status_index");
            entity.HasIndex(e => e.TransactionHash, "acct_crypto_transactions_transaction_hash_index");
            entity.HasIndex(e => e.CreatedOn, "acct_crypto_transactions_created_on_index");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Data).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
            entity.Property(e => e.TransactionHash).HasMaxLength(256);

            entity.HasOne(d => d.Crypto).WithMany(p => p.CryptoTransactions)
                .HasForeignKey(d => d.CryptoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_crypto_transactions_crypto_id_foreign");

            entity.HasOne(d => d.Payment).WithOne(p => p.CryptoTransaction)
                .HasForeignKey<CryptoTransaction>(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_crypto_transactions_payment_id_foreign");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("currencies_pkey");

            entity.ToTable("_Currency", "acct");

            entity.HasIndex(e => e.Code, "acct_currencies_code_unique").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code)
                .HasMaxLength(3)
                .IsFixedLength();
            entity.Property(e => e.Entity).HasMaxLength(64);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Deposit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Deposit_pk");

            entity.ToTable("_Deposit", "acct");

            entity.HasIndex(e => e.CurrencyId, "IX__Deposit_CurrencyId");

            entity.HasIndex(e => e.FundType, "IX__Deposit_FundType");

            entity.HasIndex(e => e.PartyId, "IX__Deposit_PartyId");

            entity.HasIndex(e => e.PaymentId, "IX__Deposit_PaymentId");

            entity.HasIndex(e => e.TargetAccountId, "IX__Deposit_TargetAccountId");

            entity.HasIndex(e => e.Type, "_Deposit_Type_index");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ReferenceNumber)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");

            entity.HasOne(d => d.Currency).WithMany(p => p.Deposits)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Deposit__Currency_Id_fk");

            entity.HasOne(d => d.FundTypeNavigation).WithMany(p => p.Deposits)
                .HasForeignKey(d => d.FundType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Deposit__FundType_Id_fk");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Deposit)
                .HasForeignKey<Deposit>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Deposit__Matters_Id_fk");

            entity.HasOne(d => d.Party).WithMany(p => p.Deposits)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Deposit__Party_Id_fk");

            entity.HasOne(d => d.Payment).WithMany(p => p.Deposits).HasForeignKey(d => d.PaymentId);

            entity.HasOne(d => d.TargetAccount).WithMany(p => p.Deposits)
                .HasForeignKey(d => d.TargetAccountId)
                .HasConstraintName("_Deposit__Account_Id_fk");
        });

        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_ExchangeRate_pk");

            entity.ToTable("_ExchangeRate", "trd");

            entity.HasIndex(e => new { e.FromCurrencyId, e.ToCurrencyId },
                "_ExchangeRate_FromCurrencyId_ToCurrencyId_uindex").IsUnique();

            entity.HasIndex(e => e.FromCurrencyId, "_ExchangeRate_FromCurrencyId_index");

            entity.HasIndex(e => new { e.FromCurrencyId, e.ToCurrencyId }, "_ExchangeRate_From_To_ux").IsUnique();

            entity.HasIndex(e => e.Name, "_ExchangeRate_Name_uindex").IsUnique();

            entity.HasIndex(e => e.ToCurrencyId, "_ExchangeRate_ToCurrencyId_index");

            entity.Property(e => e.AdjustRate).HasPrecision(16, 8);
            entity.Property(e => e.BuyingRate).HasPrecision(16, 8);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.SellingRate).HasPrecision(16, 8);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.FromCurrency).WithMany(p => p.ExchangeRateFromCurrencies)
                .HasForeignKey(d => d.FromCurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_ExchangeRate__Currency_Id_fk");

            entity.HasOne(d => d.ToCurrency).WithMany(p => p.ExchangeRateToCurrencies)
                .HasForeignKey(d => d.ToCurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_ExchangeRate__Currency_Id_fk2");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Event_pkey");

            entity.ToTable("_Event", "event");
            entity.HasIndex(e => e.CreatedOn, "IX__Event_CreatedOn");
            entity.HasIndex(e => e.ApplyStartOn, " IX__Event_ApplyStartOn");
            entity.HasIndex(e => e.ApplyEndOn, "IX__Event_ApplyEndOn");
            entity.HasIndex(e => e.StartOn, "IX__Event_StartOn");
            entity.HasIndex(e => e.EndOn, "IX__Event_EndOn");
            entity.HasIndex(e => e.Status, "IX__Event_Status");
            entity.HasIndex(e => e.Key, "IX__Event_Key");
            entity.HasIndex(e => e.AccessRoles, "IX__Event_AccessRoles");
            entity.HasIndex(e => e.AccessSites, "IX__Event_AccessSites");

            entity.Property(e => e.AccessRoles)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");

            entity.Property(e => e.AccessSites)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<EventLanguage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_EventLanguage_pkey");

            entity.ToTable("_EventLanguage", "event");
            entity.HasIndex(e => e.EventId, "IX__EventLanguage_EventId");
            entity.HasIndex(e => e.Language, "IX__EventLanguage_Language");
            entity.HasIndex(e => e.Title, "IX__EventLanguage_Title");

            entity.Property(e => e.Language)
                .HasDefaultValueSql("'en-us'::character varying");
            entity.Property(e => e.Images)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");
            entity.Property(e => e.Instruction)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'{}'::jsonb");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Event).WithMany(p => p.EventLanguages)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventLanguage__Event_Id_fk");
        });

        // modelBuilder.Entity<EventAccount>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("_EventAccount_pkey");
        //     entity.ToTable("_EventAccount", "event");
        //     entity.HasIndex(e => e.EventId, "IX__EventAccount_EventId");
        //     entity.HasIndex(e => e.AccountId, "IX__EventAccount_AccountId");
        //     entity.HasIndex(e => e.Status, "IX__EventAccount_Status");
        //
        //     entity.Property(e => e.OperatorPartyId).HasDefaultValueSql("1");
        //     entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
        //     entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
        //
        //     entity.HasOne(d => d.Event).WithMany(p => p.EventAccounts)
        //         .HasForeignKey(d => d.EventId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("_EventAccount__Event_Id_fk");
        //
        //     entity.HasOne(d => d.Account).WithMany(p => p.EventAccounts)
        //         .HasForeignKey(d => d.AccountId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("_EventAccount__Account_Id_fk");
        //
        //     entity.HasOne(d => d.OperatorParty).WithMany(p => p.EventAccounts)
        //         .HasForeignKey(d => d.OperatorPartyId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("_EventAccount__Operator_Party_Id_fk");
        // });

        modelBuilder.Entity<EventShopClientPoint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_EventShopClientPoint_pkey");
            entity.ToTable("_EventShopClientPoint", "event");
            entity.HasIndex(e => e.ChildAccountId, "IX__EventShopClientPoint_ClientAccountId");
            entity.HasIndex(e => e.ParentAccountId, "IX__EventShopClientPoint_ParentAccountId");
            entity.HasIndex(e => new { e.ChildAccountId, e.ParentAccountId },
                " IX__EventShopClientPoint_ChildAccountId_ParentAccountId_Unique").IsUnique();

            entity.HasIndex(e => e.ParentAccountRole, "IX__EventShopClientPoint_ParentAccountRole");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.ChildAccount).WithMany(p => p.ChildEventShopClientPoints)
                .HasForeignKey(d => d.ChildAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopClientPoint__ClientAccount_Id_fk");

            entity.HasOne(d => d.ParentAccount).WithMany(p => p.ParentEventShopClientPoints)
                .HasForeignKey(d => d.ParentAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopClientPoint__ParentAccount_Id_fk");
        });
        modelBuilder.Entity<EventParty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_EventParty_pkey");
            entity.ToTable("_EventParty", "event");
            entity.HasIndex(e => e.EventId, "IX__EventParty_EventId");
            entity.HasIndex(e => e.PartyId, "IX__EventParty_PartyId");
            entity.HasIndex(e => e.Status, "IX__EventParty_Status");

            entity.Property(e => e.OperatorPartyId).HasDefaultValueSql("1");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.Property(e => e.Settings)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");

            entity.HasOne(d => d.Event).WithMany(p => p.EventParties)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventParty__Event_Id_fk");

            entity.HasOne(d => d.Party).WithMany(p => p.EventParties)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventParty__Party_Id_fk");

            entity.HasOne(d => d.OperatorParty).WithMany(p => p.OperatedEventParties)
                .HasForeignKey(d => d.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventParty__Operator_Party_Id_fk");
        });
        //
        // modelBuilder.Entity<EventAccountSetting>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("_EventAccountSetting_pkey");
        //     entity.ToTable("_EventAccountSetting", "event");
        //     entity.HasIndex(e => e.EventAccountId, "IX__EventAccountSetting_EventAccountId");
        //     entity.HasIndex(e => e.Key, "IX__EventAccountSetting_Key");
        //
        //     entity.HasOne(d => d.EventAccount).WithMany(p => p.EventAccountSettings)
        //         .HasForeignKey(d => d.EventAccountId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("_EventAccountSetting__EventAccount_Id_fk");
        // });

        modelBuilder.Entity<EventShopPoint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_EventShopPoint_pkey");

            entity.ToTable("_EventShopPoint", "event");

            entity.HasIndex(e => e.EventPartyId, "IX__EventShopPoint_EventPartyId");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.EventParty)
                .WithOne(p => p.EventShopPoint)
                .HasForeignKey<EventShopPoint>(d => d.EventPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopPoint__EventParty_Id_fk");
        });

        modelBuilder.Entity<EventShopPointTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_EventShopPointTransaction_pkey");
            entity.ToTable("_EventShopPointTransaction", "event");

            entity.HasIndex(e => e.EventPartyId, "IX__EventShopPointTransaction_EventPartyId");
            entity.HasIndex(e => e.SourceType, "IX__EventShopPointTransaction_SourceType");
            entity.HasIndex(e => e.Status, "IX__EventShopPointTransaction_Status");
            entity.HasIndex(e => e.SourceId, "IX__EventShopPointTransaction_SourceId");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.EventParty)
                .WithMany(p => p.EventShopPointTransactions)
                .HasForeignKey(d => d.EventPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopPointTransaction__EventParty_Id_fk");

            entity.HasOne(d => d.Account)
                .WithMany(d => d.EventShopPointTransactions)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopPointTransaction__AccountId_fk");
        });

        modelBuilder.Entity<EventShopItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_EventShopItem_pkey");
            entity.ToTable("_EventShopItem", "event");

            entity.HasIndex(e => e.EventId, "IX__EventShopItem_EventId");
            entity.HasIndex(e => e.Status, "IX__EventShopItem_Status");
            entity.HasIndex(e => e.Type, "IX__EventShopItem_Type");
            entity.HasIndex(e => e.Category, "IX__EventShopItem_Category");

            entity.Property(e => e.AccessRoles)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");

            entity.Property(e => e.AccessSites)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");

            entity.Property(e => e.Configuration)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'{}'::jsonb");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Event)
                .WithMany(p => p.EventShopItems)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopItem__Event_Id_fk");
        });

        modelBuilder.Entity<EventShopItemLanguage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_EventShopItemLanguage_pkey");
            entity.ToTable("_EventShopItemLanguage", "event");

            entity.HasIndex(e => e.EventShopItemId, "IX__EventShopItemLanguage_EventShopItemId");
            entity.HasIndex(e => e.Language, "IX__EventShopItemLanguage_Language");
            entity.HasIndex(e => e.Title, "IX__EventShopItemLanguage_Title");
            entity.HasIndex(e => e.Description, "IX__EventShopItemLanguage_Description");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Language).HasDefaultValueSql("'en-us'::character varying");
            entity.Property(e => e.Images)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");

            entity.HasOne(d => d.EventShopItem)
                .WithMany(p => p.EventShopItemLanguages)
                .HasForeignKey(d => d.EventShopItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopItemLanguage__EventShopItem_Id_fk");
        });

        modelBuilder.Entity<EventShopOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_EventShopOrder_pkey");
            entity.ToTable("_EventShopOrder", "event");

            entity.HasIndex(e => e.EventPartyId, "IX__EventShopOrder_EventPartyId");
            entity.HasIndex(e => e.EventShopItemId, "IX__EventShopOrder_EventShopItemId");
            entity.HasIndex(e => e.Status, "IX__EventShopOrder_Status");
            entity.HasIndex(e => e.AddressId, " IX__EventShopOrder_AddressId");

            entity.Property(e => e.OperatorPartyId).HasDefaultValueSql("1");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Shipping)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");

            entity.HasOne(d => d.EventParty)
                .WithMany(p => p.EventShopOrders)
                .HasForeignKey(d => d.EventPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopOrder__EventParty_Id_fk");

            entity.HasOne(d => d.EventShopItem)
                .WithMany(p => p.EventShopOrders)
                .HasForeignKey(d => d.EventShopItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopOrder__EventShopItem_Id_fk");

            entity.HasOne(d => d.Address)
                .WithMany(p => p.EventShopOrders)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopOrder__Address_Id_fk");

            entity.HasOne(d => d.OperatorParty)
                .WithMany(p => p.OperatedEventShopOrders)
                .HasForeignKey(d => d.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopOrder__Operator_Party_Id_fk");
        });

        modelBuilder.Entity<EventShopReward>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_EventShopReward_pkey");
            entity.ToTable("_EventShopReward", "event");

            entity.HasIndex(e => e.EventPartyId, "IX__EventShopReward_EventPartyId");
            entity.HasIndex(e => e.EventShopItemId, "IX__EventShopReward_EventShopItemId");
            entity.HasIndex(e => e.Status, "IX__EventShopReward_Status");

            entity.Property(e => e.OperatorPartyId).HasDefaultValueSql("1");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.EffectiveTo).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.EventParty)
                .WithMany(p => p.EventShopRewards)
                .HasForeignKey(d => d.EventPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopReward__EventParty_Id_fk");

            entity.HasOne(d => d.EventShopItem)
                .WithMany(p => p.EventShopRewards)
                .HasForeignKey(d => d.EventShopItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopReward__EventShopItem_Id_fk");

            entity.HasOne(d => d.OperatorParty)
                .WithMany(p => p.OperatedEventShopRewards)
                .HasForeignKey(d => d.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopReward__Operator_Party_Id_fk");
        });

        modelBuilder.Entity<EventShopRewardRebate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_EventShopRewardRebate_pkey");
            entity.ToTable("_EventShopRewardRebate", "event");

            entity.HasIndex(e => e.EventShopRewardId, "IX__EventShopRewardRebate_EventShopRewardId");
            entity.HasIndex(e => e.Status, "IX__EventShopRewardRebate_Status");
            entity.HasIndex(e => e.Ticket, "IX__EventShopRewardRebate_TradeId");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.EventShopReward)
                .WithMany(p => p.EventShopRewardRebates)
                .HasForeignKey(d => d.EventShopRewardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_EventShopRewardRebate__EventShopReward_Id_fk");
        });

        modelBuilder.Entity<FundType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallet_types_pkey");

            entity.ToTable("_FundType", "acct");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(64);

            entity.HasMany(d => d.PaymentServices).WithMany(p => p.FundTypes)
                .UsingEntity<Dictionary<string, object>>(
                    "PaymentServiceFundType",
                    r => r.HasOne<PaymentService>().WithMany()
                        .HasForeignKey("PaymentServiceId")
                        .HasConstraintName("_PaymentServiceFundType__PaymentService_Id_fk"),
                    l => l.HasOne<FundType>().WithMany()
                        .HasForeignKey("FundTypeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("_PaymentServiceFundType__FundType_Id_fk"),
                    j =>
                    {
                        j.HasKey("FundTypeId", "PaymentServiceId").HasName("_PaymentServiceFundType_pk");
                        j.ToTable("_PaymentServiceFundType", "acct");
                        j.HasIndex(new[] { "PaymentServiceId" }, "IX__PaymentServiceFundType_PaymentServiceId");
                    });
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Group_pk");

            entity.ToTable("_Group", "trd");

            entity.HasIndex(e => new { e.OwnerAccountId, e.Type }, "_Group_AccountId_Type_uindex").IsUnique();

            entity.HasIndex(e => e.Name, "_Group_Name_uindex").IsUnique();

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Description).HasMaxLength(256);
            entity.Property(e => e.Name).HasMaxLength(64);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.OwnerAccount).WithMany(p => p.GroupsNavigation)
                .HasForeignKey(d => d.OwnerAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Group__Account_Id_fk");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("invoices_pkey");

            entity.ToTable("_Invoice", "acct");

            entity.HasIndex(e => e.CurrencyId, "IX_invoices_currency_id");

            entity.HasIndex(e => e.InvoicedOn, "acct_invoices_invoiced_at_index");

            entity.HasIndex(e => e.Number, "acct_invoices_number_index");

            entity.HasIndex(e => e.Recipient, "acct_invoices_recipient_index");

            entity.HasIndex(e => e.Sender, "acct_invoices_sender_index");

            entity.Property(e => e.Id).HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Number).HasMaxLength(64);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Currency).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_invoices_currency_id_foreign");

            entity.HasOne(d => d.RecipientNavigation).WithMany(p => p.InvoiceRecipientNavigations)
                .HasForeignKey(d => d.Recipient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_invoices_recipient_foreign");

            entity.HasOne(d => d.SenderNavigation).WithMany(p => p.InvoiceSenderNavigations)
                .HasForeignKey(d => d.Sender)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_invoices_sender_foreign");
        });

        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Lead_pk");

            entity.ToTable("_Lead", "trd");

            entity.HasIndex(e => e.CreatedOn, "_Lead_CreatedOn_index");

            entity.HasIndex(e => e.Email, "_Lead_Email_index");

            entity.HasIndex(e => e.PhoneNumber, "_Lead_PhoneNumber_index");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(64)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Supplement)
                .HasDefaultValueSql("'{}'::json")
                .HasColumnType("json");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasMany(d => d.Accounts).WithMany(p => p.Leads)
                .UsingEntity<Dictionary<string, object>>(
                    "LeadOwnerAccount",
                    r => r.HasOne<Account>().WithMany()
                        .HasForeignKey("AccountId")
                        .HasConstraintName("_LeadOwnerAccount__Account_Id_fk"),
                    l => l.HasOne<Lead>().WithMany()
                        .HasForeignKey("LeadId")
                        .HasConstraintName("_LeadOwnerAccount__Lead_Id_fk"),
                    j =>
                    {
                        j.HasKey("LeadId", "AccountId").HasName("_LeadOwnerAccount_pk");
                        j.ToTable("_LeadOwnerAccount", "trd");
                    });

            entity.HasOne(x => x.Party).WithMany(x => x.Leads).HasForeignKey(x => x.PartyId);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_pkey");
            entity.ToTable("_Product", "shop");

            entity.HasIndex(e => e.Type, "_Product_Type_index");
            entity.HasIndex(e => e.Status, "_Product_Status_index");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");

            entity.HasIndex(e => e.Point, "_Product_Point_index");
            entity.HasIndex(e => e.Total, "_Product_Total_index");

            entity.Property(e => e.Supplement)
                .HasDefaultValueSql("'{}'::json")
                .HasColumnType("json");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
            entity.HasIndex(e => e.OperatorPartyId, "_Product_OperatorPartyId_index");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_pkey");
            entity.ToTable("_Order", "shop");

            entity.HasIndex(e => e.PartyId, "order_party_id_index");
            entity.HasIndex(e => e.ProductId, "order_product_id_index");
            entity.HasIndex(e => e.Number, "order_number_index");
            entity.HasIndex(e => e.Amount, "order_amount_index");
            entity.HasIndex(e => e.Status, "order_status_index");

            entity.Property(e => e.Recipient).HasMaxLength(256).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Note).HasMaxLength(256).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Supplement)
                .HasDefaultValueSql("'{}'::json")
                .HasColumnType("json");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("shop_order_party_id_foreign");

            entity.HasOne(d => d.Product).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("shop_order_product_id_foreign");
        });

        modelBuilder.Entity<Ledger>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ledgers_pkey");

            entity.ToTable("_Ledger", "acct");

            entity.HasIndex(e => e.ChargeAmount, "acct_ledgers_charge_amount_index");

            entity.HasIndex(e => e.ChargeId, "acct_ledgers_charge_id_index");

            entity.HasIndex(e => e.CurrencyId, "acct_ledgers_currency_id_index");

            entity.HasIndex(e => e.InvoiceId, "acct_ledgers_invoice_id_index");

            entity.HasIndex(e => e.LedgerSide, "acct_ledgers_ledger_side_index");

            entity.HasIndex(e => e.MatterId, "acct_ledgers_matter_id_index");

            entity.HasIndex(e => e.PartyId, "acct_ledgers_party_id_index");

            entity.HasIndex(e => e.TalliedOn, "acct_ledgers_tallied_at_index");

            entity.Property(e => e.Id).HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.TalliedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Charge).WithMany(p => p.Ledgers)
                .HasForeignKey(d => d.ChargeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_ledgers_charge_id_foreign");

            entity.HasOne(d => d.Currency).WithMany(p => p.Ledgers)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_ledgers_currency_id_foreign");

            entity.HasOne(d => d.Matter).WithMany(p => p.Ledgers)
                .HasForeignKey(d => d.MatterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_ledgers_matter_id_foreign");

            entity.HasOne(d => d.Party).WithMany(p => p.Ledgers)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_ledgers_party_id_foreign");
        });

        modelBuilder.Entity<LoginLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_LoginLog_pkey");

            entity.ToTable("_LoginLog", "core");

            entity.HasIndex(e => e.IpAddress, "IX__LoginLog_IpAddress");
            entity.Property(e => e.IpAddress).HasMaxLength(64);

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party).WithMany(p => p.LoginLogs)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_LoginLog__Party_Id_fk");
        });
        modelBuilder.Entity<Matter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ix_matter_k8s_id");

            entity.ToTable("matter_k8s", "core");

            entity.HasIndex(e => e.Pid, "ix_matter_k8s_pid");

            entity.HasIndex(e => e.PostedOn, "ix_matter_k8s_posted_on");

            entity.HasIndex(e => e.StateId, "ix_matter_k8s_state_id");

            entity.HasIndex(e => e.StatedOn, "ix_matter_k8s_stated_on");

            entity.HasIndex(e => e.Type, "ix_matter_k8s_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Pid).HasColumnName("pid");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.PostedOn).HasColumnName("posted_on");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.StatedOn).HasColumnName("stated_on");

            entity.HasOne(d => d.PidNavigation).WithMany(p => p.InversePidNavigation)
                .HasForeignKey(d => d.Pid)
                .HasConstraintName("fk_matter_k8s_pid");

            entity.HasOne(d => d.TypeNavigation).WithMany(p => p.Matters)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_matter_k8s_type");
        });

        modelBuilder.Entity<MatterType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("matter_types_pkey");

            entity.ToTable("_MatterType", "core");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(64);
        });

        modelBuilder.Entity<Medium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Medium_pk");

            entity.ToTable("_Medium", "sto");

            entity.HasIndex(e => e.Guid, "IX__Medium_Guid").IsUnique();

            entity.HasIndex(e => e.Pid, "IX__Medium_Pid");
            entity.HasIndex(e => e.Context, "IX__Medium_Context");

            entity.Property(e => e.Id).HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.ContentType).HasMaxLength(128);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.FileName).HasMaxLength(256);
            entity.Property(e => e.Guid).HasMaxLength(64);
            entity.Property(e => e.Type).HasMaxLength(32);
            entity.Property(e => e.Context).HasMaxLength(64).HasDefaultValueSql("''::character varying");

            entity.HasOne(d => d.PidNavigation).WithMany(p => p.InversePidNavigation)
                .HasForeignKey(d => d.Pid)
                .HasConstraintName("_Medium__Medium_Id_fk");

            entity.HasOne(d => d.Party)
                .WithMany(p => p.Medium)
                .HasForeignKey(d => d.PartyId);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Message_pk");

            entity.ToTable("_Message", "cms");

            entity.HasIndex(e => e.PartyId, "IX__Message_PartyId");

            entity.Property(e => e.Content).HasDefaultValueSql("''::text");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Title)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party).WithMany(p => p.Messages)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Message__Party_Id_fk");
        });

        modelBuilder.Entity<MessageRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_MessageRecord_pkey");

            entity.ToTable("_MessageRecord", "sto");

            entity.HasIndex(e => e.ReceiverPartyId, "IX__MessageRecord_ReceiverPartyId");
            entity.HasIndex(e => e.Event, "IX__MessageRecord_Event");
            entity.HasIndex(e => e.Receiver, "IX__MessageRecord_Receiver");
            entity.HasIndex(e => e.Method, "IX__MessageRecord_Method");
            entity.HasIndex(e => e.Status, "IX__MessageRecord_Status");
            entity.HasIndex(e => e.CreatedOn, "IX__MessageRecord_CreatedOn");
            entity.HasIndex(e => new { e.ReceiverPartyId, e.Event, e.Method, e.CreatedOn, e.Receiver },
                "IX__MessageRecord_ReceiverPartyId_Receiver_Method_Event_Created");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()");

            entity.Property(e => e.Event).HasMaxLength(64);
            entity.Property(e => e.Method)
                .HasDefaultValueSql("'email'::character varying")
                .HasColumnType("character varying");
            entity.Property(e => e.Receiver).HasColumnType("character varying");
            entity.Property(e => e.UpdatedOn)
                .HasDefaultValueSql("now()");

            entity.HasOne(d => d.ReceiverParty).WithMany(p => p.MessageRecords)
                .HasForeignKey(d => d.ReceiverPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_MessageRecord__ReceiverParty_Id_fk");
        });

        modelBuilder.Entity<NotificationEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_NotificationEvent_pk");

            entity.ToTable("_NotificationEvent", "cms");

            entity.HasIndex(e => e.Code, "_NotificationEvent_Code_uindex").IsUnique();

            entity.HasIndex(e => e.Name, "_NotificationEvent_Name_uindex").IsUnique();

            entity.HasIndex(e => new { e.SubjectType, e.ChannelType, e.MethodType },
                "_NotificationEvent_SubjectType_ChannelType_MethodType_uindex").IsUnique();

            entity.Property(e => e.Code)
                .HasMaxLength(128)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.IsActivated).HasDefaultValueSql("1");
            entity.Property(e => e.ModuleName)
                .HasMaxLength(128)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Name).HasMaxLength(128);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<NotificationSubscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_NotificationSubscription_pk");

            entity.ToTable("_NotificationSubscription", "cms");

            entity.HasIndex(e => e.NotificationEventId, "IX__NotificationSubscription_NotificationEventId");

            entity.HasIndex(e => new { e.PartyId, e.NotificationEventId },
                "_NotificationSubscription_PartyId_NotificationEventId_uindex").IsUnique();

            entity.Property(e => e.IsActivated).HasDefaultValueSql("1");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.NotificationEvent).WithMany(p => p.NotificationSubscriptions)
                .HasForeignKey(d => d.NotificationEventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_NotificationSubscription__NotificationEvent_Id_fk");

            entity.HasOne(d => d.Party).WithMany(p => p.NotificationSubscriptions)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_NotificationSubscription__Party_Id_fk");
        });

        modelBuilder.Entity<Party>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("parties_pkey");

            entity.ToTable("_Party", "core");

            entity.HasIndex(e => e.SiteId, "IX__Party_SiteId");

            entity.HasIndex(e => e.Code, "core_parties_code_index");

            entity.HasIndex(e => e.Pid, "core_parties_pid_index");
            entity.HasIndex(e => e.NativeName, "core_parties_native_name_index");
            entity.HasIndex(e => e.Email, "core_parties_email_index");
            entity.HasIndex(e => e.Uid, "core_parties_uid_index");
            entity.HasIndex(e => e.Status, "core_parties_status_index");

            entity.Property(e => e.Id).HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(64);
            entity.Property(e => e.NativeName).HasMaxLength(128).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Email).HasMaxLength(128).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.FirstName).HasMaxLength(64).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.LastName).HasMaxLength(64).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Language).HasMaxLength(20).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Avatar).HasMaxLength(255).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.TimeZone).HasMaxLength(30).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.ReferCode).HasMaxLength(20).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.CountryCode).HasMaxLength(10).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.CCC).HasMaxLength(10).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.PhoneNumber).HasMaxLength(32).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Birthday).HasDefaultValueSql("now()");
            entity.Property(e => e.Gender).HasDefaultValueSql("0");
            entity.Property(e => e.Citizen).HasMaxLength(32).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Address).HasMaxLength(512).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.IdType).HasDefaultValueSql("0");
            entity.Property(e => e.IdNumber).HasMaxLength(128);
            entity.Property(e => e.IdIssuer).HasMaxLength(128);
            entity.Property(e => e.RegisteredIp).HasMaxLength(128).HasDefaultValueSql("''::character varying");
            entity.Property(e => e.LastLoginIp).HasMaxLength(128).HasDefaultValueSql("''::character varying");

            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.PidNavigation).WithMany(p => p.InversePidNavigation)
                .HasForeignKey(d => d.Pid)
                .HasConstraintName("core_parties_pid_foreign");

            entity.HasOne(d => d.Site).WithMany(p => p.Parties)
                .HasForeignKey(d => d.SiteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Party__Site_Id_fk");
        });

        modelBuilder.Entity<PartyComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_PartyComment_pk");

            entity.ToTable("_PartyComment", "core");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party)
                .WithMany(p => p.PartyComments)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("_PartyComment__Party_Id_fk");

            entity.HasOne(d => d.OperatorParty)
                .WithMany(p => p.OperatedPartyComments)
                .HasForeignKey(d => d.OperatorPartyId)
                .HasConstraintName("_PartyComment__OperatorParty_Id_fk");
        });

        modelBuilder.Entity<PartyTag>(entity =>
        {
            entity.HasKey(e => new { e.PartyId, e.TagId }).HasName("_PartyTag_pk");

            entity.ToTable("_PartyTag", "core");

            entity.HasOne(d => d.Party)
                .WithMany(p => p.PartyTags)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("_PartyTag__Party_Id_fk");

            entity.HasOne(d => d.Tag)
                .WithMany(p => p.PartyTags)
                .HasForeignKey(d => d.TagId)
                .HasConstraintName("_PartyTag__Tag_Id_fk");
        });

        modelBuilder.Entity<PartyRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_PartyRole_pk");

            entity.ToTable("_PartyRole", "core");

            entity.HasIndex(e => e.PartyId, "IX__PartyRole_PartyId");

            entity.HasIndex(e => e.RoleId, "IX__PartyRole_RoleId");

            entity.HasOne(d => d.Party).WithMany(p => p.PartyRoles)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_PartyRole__Party_Id_fk");

            entity.HasOne(d => d.Role).WithMany(p => p.PartyRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_PartyRole__Role_Id_fk");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payments_pkey");

            entity.ToTable("_Payment", "acct");

            entity.HasIndex(e => e.CurrencyId, "IX_payments_currency_id");

            entity.HasIndex(e => e.PaymentServiceId, "IX_payments_payment_method");

            entity.HasIndex(e => e.Number, "_Payment_Number_uindex").IsUnique();

            entity.HasIndex(e => e.ReferenceNumber, "_Payment_ReferenceNumber_index");

            entity.HasIndex(e => e.Number, "acct_payments_number_index");

            entity.HasIndex(e => e.Pid, "acct_payments_pid_index");

            entity.HasIndex(e => e.PartyId, "acct_payments_recipient_index");

            entity.Property(e => e.Id).HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Number).HasMaxLength(64);
            entity.Property(e => e.ReferenceNumber)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.CallbackBody).HasDefaultValueSql("'{}'::text");

            entity.HasOne(d => d.Currency).WithMany(p => p.Payments)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_payments_currency_id_foreign");

            entity.HasOne(d => d.Party).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_payments_recipient_foreign");

            // Removed old PaymentService FK - PaymentService table is deprecated
            // entity.HasOne(d => d.PaymentService).WithMany(p => p.Payments)
            //     .HasForeignKey(d => d.PaymentServiceId)
            //     .OnDelete(DeleteBehavior.ClientSetNull)
            //     .HasConstraintName("_Payment__PaymentService_Id_fk");

            // PaymentServiceId actually references PaymentMethod.Id (not PaymentService)
            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Payment__PaymentMethod_Id_fk");

            entity.HasOne(d => d.PidNavigation).WithMany(p => p.InversePidNavigation)
                .HasForeignKey(d => d.Pid)
                .HasConstraintName("acct_payments_pid_foreign");
        });

        modelBuilder.Entity<PaymentInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_PaymentInfo_pk");

            entity.ToTable("_PaymentInfo", "acct");

            entity.HasIndex(e => e.PartyId, "IX__PaymentInfo_PartyId");

            entity.HasIndex(e => e.PaymentServiceId, "IX__PaymentInfo_PaymentServiceId");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Info).HasDefaultValueSql("'{}'::text");
            entity.Property(e => e.Name).HasMaxLength(64);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party).WithMany(p => p.PaymentInfos)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_PaymentInfo__Party_Id_fk");

            entity.HasOne(d => d.PaymentService).WithMany(p => p.PaymentInfos)
                .HasForeignKey(d => d.PaymentServiceId)
                .HasConstraintName("_PaymentInfo__PaymentService_Id_fk");
        });


        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_PaymentMethod_pk");

            entity.ToTable("_PaymentMethod", "acct");

            entity.HasIndex(e => e.Group, "IX__PaymentMethod_Group");
            entity.HasIndex(e => e.MethodType, "IX__PaymentMethod_MethodType");
            entity.HasIndex(e => e.Status, "IX__PaymentMethod_Status");
            entity.HasIndex(e => e.MinValue, "IX__PaymentMethod_MinValue");
            entity.HasIndex(e => e.MaxValue, "IX__PaymentMethod_MaxValue");
            entity.HasIndex(e => e.InitialValue, "IX__PaymentMethod_InitialValue");
            entity.HasIndex(e => e.DeletedOn, "IX__PaymentMethod_DeletedOn");
            entity.HasIndex(e => e.IsDeleted, "IX__PaymentMethod_IsDeleted");

            entity.HasIndex(e => new { e.Group, e.MethodType, e.Status }, "IX__PaymentMethod_Group_MethodType_Status");

            entity.Property(e => e.CommentCode)
                .HasMaxLength(6)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Configuration)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.AvailableCurrencies)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.CurrencyId).HasDefaultValueSql("'-1'::integer");
            entity.Property(e => e.Group)
                .HasMaxLength(32)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Logo)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.MethodType)
                .HasMaxLength(12)
                .HasDefaultValueSql("'deposit'::character varying");
            entity.Property(e => e.Name).HasMaxLength(32);
            entity.Property(e => e.Note).HasDefaultValueSql("''::text");
            entity.Property(e => e.Percentage).HasDefaultValueSql("100");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.OperatorPartyId).HasDefaultValueSql("1");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.OperatorParty).WithMany(p => p.OperatedPaymentMethods)
                .HasForeignKey(e => e.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_PaymentMethod__OperatorParty_Id_fk");
        });

        // modelBuilder.Entity<PaymentMethodAccess>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("_PaymentMethodAccess_pkey");
        //
        //     entity.ToTable("_PaymentMethodAccess", "acct");
        //
        //     entity.HasIndex(e => new { e.Model, e.RowId }, "_PaymentMethodAccess_Model_RowId_index");
        //
        //     entity.HasIndex(e => new { e.PartyId, e.Model, e.RowId }, "_PaymentMethodAccess_PartyId_Model_RowId_index");
        //
        //     entity.HasIndex(e => e.PartyId, "_PaymentMethodAccess_PartyId_index");
        //
        //     entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
        //     entity.Property(e => e.Model)
        //         .HasMaxLength(12)
        //         .HasDefaultValueSql("'account'::character varying");
        //     entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
        //
        //     entity.HasOne(d => d.PaymentMethod).WithMany(p => p.PaymentMethodAccess)
        //         .HasForeignKey(d => d.PaymentMethodId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("_PaymentMethodAccess__PaymentMethod_Id_fk");
        //
        //     entity.HasOne(d => d.Party).WithMany(p => p.PaymentMethodAccesses)
        //         .HasForeignKey(d => d.PartyId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("_PaymentMethodAccess__PartyId_fk");
        // });

        modelBuilder.Entity<PaymentService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_PaymentService_pk");

            entity.ToTable("_PaymentService", "acct");

            entity.HasIndex(e => e.CurrencyId, "IX__PaymentService_CurrencyId");

            entity.Property(e => e.CategoryName)
                .HasMaxLength(32)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.CommentCode)
                .HasMaxLength(6)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Configuration).HasDefaultValueSql("'{}'::text");
            entity.Property(e => e.CurrencyId).HasDefaultValueSql("'-1'::integer");
            entity.Property(e => e.Description)
                .HasMaxLength(1024)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Name).HasMaxLength(32);

            entity.HasOne(d => d.Currency).WithMany(p => p.PaymentServices)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_PaymentService__Currency_Id_fk");
        });

        modelBuilder.Entity<PaymentServiceAccess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_PaymentServiceAccess_pk");

            entity.ToTable("_PaymentServiceAccess", "acct");

            entity.HasIndex(e => e.CurrencyId, "IX__PaymentServiceAccess_CurrencyId");

            entity.HasIndex(e => e.FundType, "IX__PaymentServiceAccess_FundType");

            entity.HasIndex(e => e.PaymentServiceId, "IX__PaymentServiceAccess_PaymentServiceId");

            entity.HasIndex(e => new { e.PartyId, e.FundType, e.CurrencyId, e.PaymentServiceId },
                "_PaymentServiceAccess_PartyId_FundType_CurrencyId_PaymentServic").IsUnique();

            entity.HasIndex(e => e.PartyId, "_PaymentServiceAccess_PartyId_index");

            entity.Property(e => e.CurrencyId).HasDefaultValueSql("'-1'::integer");

            entity.HasOne(d => d.Currency).WithMany(p => p.PaymentServiceAccesses)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_PaymentServiceAccess__Currency_Id_fk");

            entity.HasOne(d => d.FundTypeNavigation).WithMany(p => p.PaymentServiceAccesses)
                .HasForeignKey(d => d.FundType)
                .HasConstraintName("_PaymentServiceAccess__FundType_Id_fk");

            entity.HasOne(d => d.Party).WithMany(p => p.PaymentServiceAccesses)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("_PaymentServiceAccess__Party_Id_fk");

            entity.HasOne(d => d.PaymentService).WithMany(p => p.PaymentServiceAccesses)
                .HasForeignKey(d => d.PaymentServiceId)
                .HasConstraintName("_PaymentServiceAccess__PaymentService_Id_fk");
        });

        modelBuilder.Entity<PayoutRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_PayoutRecord_pk");

            entity.ToTable("_PayoutRecord", "acct");

            entity.HasIndex(e => e.BatchUid, "IX__PayoutRecord_BatchUid");
            entity.HasIndex(e => e.BankName, "IX__PayoutRecord_BankName");
            entity.HasIndex(e => e.BankCode, "IX__PayoutRecord_BankCode");
            entity.HasIndex(e => e.BranchName, "IX__PayoutRecord_BranchName");
            entity.HasIndex(e => e.AccountName, "IX__PayoutRecord_AccountName");
            entity.HasIndex(e => e.BankNumber, "IX__PayoutRecord_BankNumber");
            entity.HasIndex(e => e.Currency, "IX__PayoutRecord_Currency");
            entity.HasIndex(e => e.Status, "IX__PayoutRecord_Status");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Info).HasDefaultValueSql("'{}'::jsonb").HasColumnType("jsonb");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.PayoutRecords)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_PayoutRecord__PaymentMethod_Id_fk");

            entity.HasOne(o => o.OperatorParty)
                .WithMany(c => c.OperatedPayoutRecords)
                .HasForeignKey(o => o.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false);
        });

        // modelBuilder.Entity<Permission>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("_Permission_pk");
        //
        //     entity.ToTable("_Permission", "core");
        //
        //     entity.HasIndex(e => e.Action, "_Permission_Action_index");
        //     entity.HasIndex(e => e.Method, "_Permission_Method_index");
        //     entity.HasIndex(e => e.Category, "_Permission_Category_index");
        //
        //     entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
        //     entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
        // });
        //
        // modelBuilder.Entity<PermissionAccess>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("_PermissionAccess_pk");
        //
        //     entity.ToTable("_PermissionAccess", "core");
        //
        //     entity.HasIndex(e => e.Model, "_PermissionAccess_Model_index");
        //     entity.HasIndex(e => e.ModelId, "_PermissionAccess_ModelId_index");
        //     entity.HasIndex(e => e.PermissionId, "_PermissionAccess_Permission_index");
        //
        //     entity.HasOne(d => d.Permission).WithMany(p => p.PermissionAccesses)
        //         .HasForeignKey(d => d.PermissionId)
        //         .HasConstraintName("_PermissionAccess__Permission_Id_fk");
        // });

        modelBuilder.Entity<RebateNew>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("rebate_new_pkey");

            entity.ToTable("_RebateNew", "public");

            entity.HasIndex(e => e.FundType, "IX__Rebate_New_FundType");

            entity.HasIndex(e => e.TradeRebateId, "IX__Rebate_New_TradeRebateId_index");

            entity.HasIndex(e => e.AccountId, "IX__Rebate_New_AccountId_index");

            entity.HasIndex(e => e.CurrencyId, "IX__Rebate_New_CurrencyId_index");
        });

        modelBuilder.Entity<Rebate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ix_rebate_k8s_id");

            entity.ToTable("rebate_k8s", "trd");

            entity.HasIndex(e => e.FundType, "ix_rebate_k8s_fund_type");

            entity.HasIndex(e => e.HoldUntilOn, "ix_rebate_k8s_hold_until_on");

            entity.HasIndex(e => e.TradeRebateId, "ix_rebate_k8s_trade_rebate_id");

            entity.HasIndex(e => e.AccountId, "ix_rebate_k8s_account_id");

            entity.HasIndex(e => e.CurrencyId, "ix_rebate_k8s_currency_id");

            entity.HasIndex(e => e.PartyId, "ix_rebate_k8s_party_id");

            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.PartyId).HasColumnName("party_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.FundType).HasColumnName("fund_type");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.TradeRebateId).HasColumnName("trade_rebate_id");
            entity.Property(e => e.HoldUntilOn).HasColumnName("hold_until_on");
            entity.Property(e => e.Information).HasColumnName("information");

            entity.HasOne(d => d.Account).WithMany(p => p.Rebates)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rebate_k8s_account_id");

            entity.HasOne(d => d.Currency).WithMany(p => p.Rebates)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rebate_k8s_currency_id");

            entity.HasOne(d => d.FundTypeNavigation).WithMany(p => p.Rebates)
                .HasForeignKey(d => d.FundType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rebate_k8s_fund_type");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Rebate)
                .HasForeignKey<Rebate>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rebate_k8s_matter_id");

            entity.HasOne(d => d.Party).WithMany(p => p.Rebates)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rebate_k8s_party_id");

            entity.HasOne(d => d.TradeRebate).WithMany(p => p.Rebates)
                .HasForeignKey(d => d.TradeRebateId)
                .HasConstraintName("fk_rebate_k8s_trade_rebate_id");
        });

        modelBuilder.Entity<RebateAgentRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_AgentRebateRule_Id_uindex");

            entity.ToTable("_RebateAgentRule", "trd");

            entity.HasIndex(e => e.AgentAccountId, "_AgentRebateRule_AgentAccountId_uindex").IsUnique();

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.LevelSetting)
                .HasDefaultValueSql("'{}'::json")
                .HasColumnType("json");
            entity.Property(e => e.Schema)
                .HasDefaultValueSql("'[]'::json")
                .HasColumnType("json");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.AgentAccount).WithOne(p => p.RebateAgentRule)
                .HasForeignKey<RebateAgentRule>(d => d.AgentAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_AgentRebateRule__Account_Id_fk");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("_RebateAgentRule__RebateAgentRule_Id_fk");
        });

        modelBuilder.Entity<RebateBaseSchema>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_RebateBaseSchema_Id_uindex");

            entity.ToTable("_RebateBaseSchema", "trd");

            entity.HasIndex(e => e.CreatedBy, "_RebateBaseSchema_CreatedBy_index");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Note)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RebateBaseSchemas)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_RebateBaseSchema__Party_Id_fk");
        });

        modelBuilder.Entity<RebateBaseSchemaItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_RebateBaseSchemaItem_pk");

            entity.ToTable("_RebateBaseSchemaItem", "trd");

            entity.HasIndex(e => e.RebateBaseSchemaId, "_RebateBaseSchemaItem_RebateRuleId_index");

            entity.HasIndex(e => e.SymbolCode, "_RebateBaseSchemaItem_SymbolCode_index");

            entity.Property(e => e.Commission).HasPrecision(16, 2);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Pips).HasPrecision(16, 2);
            entity.Property(e => e.Rate).HasPrecision(16, 2);
            entity.Property(e => e.SymbolCode).HasMaxLength(64);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.RebateBaseSchema).WithMany(p => p.RebateBaseSchemaItems)
                .HasForeignKey(d => d.RebateBaseSchemaId)
                .HasConstraintName("_RebateBaseSchemaItem__RebateBaseSchema_Id_fk");
        });

        modelBuilder.Entity<RebateBrokerRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_BrokerRebateRule_pk");

            entity.ToTable("_RebateBrokerRule", "trd");

            entity.HasIndex(e => e.BrokerAccountId, "_BrokerRebateRule_pk2").IsUnique();

            entity.Property(e => e.AllowAccountRoles).HasColumnType("json");
            entity.Property(e => e.AllowAccountTypes).HasColumnType("json");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Schema).HasColumnType("json");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.BrokerAccount).WithOne(p => p.RebateBrokerRule)
                .HasForeignKey<RebateBrokerRule>(d => d.BrokerAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_BrokerRebateRule__Account_Id_fk");
        });

        modelBuilder.Entity<RebateClientRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_ClientRebateRule_pk");

            entity.ToTable("_RebateClientRule", "trd");

            entity.HasIndex(e => e.ClientAccountId, "_ClientRebateRule_pk2").IsUnique();

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.Property(e => e.Schema)
                .HasDefaultValueSql("'{}'::json")
                .HasColumnType("json");

            entity.HasOne(d => d.ClientAccount).WithOne(p => p.RebateClientRule)
                .HasForeignKey<RebateClientRule>(d => d.ClientAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_ClientRebateRule__Account_Id_fk");

            entity.HasOne(d => d.RebateDirectSchema).WithMany(p => p.RebateClientRules)
                .HasForeignKey(d => d.RebateDirectSchemaId)
                .HasConstraintName("_RebateClientRule__RebateDirectSchema_Id_fk");
        });

        modelBuilder.Entity<RebateDirectRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_RebateDirectRule_pk");

            entity.ToTable("_RebateDirectRule", "trd");

            entity.HasIndex(e => e.ConfirmedBy, "_RebateDirectRule_ConfirmedBy_index");

            entity.HasIndex(e => e.CreatedBy, "_RebateDirectRule_CreatedBy_index");

            entity.HasIndex(e => e.SourceTradeAccountId, "_RebateDirectRule_SourceTradeAccountId_index");

            entity.HasIndex(e => new { e.TargetAccountId, e.SourceTradeAccountId },
                "_RebateDirectRule_TargetAccountId_SourceTradeAccountId_uindex").IsUnique();

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.ConfirmedByNavigation).WithMany(p => p.RebateDirectRuleConfirmedByNavigations)
                .HasForeignKey(d => d.ConfirmedBy)
                .HasConstraintName("_RebateDirectRule__Party_Id_fk2");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RebateDirectRuleCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_RebateDirectRule__Party_Id_fk");

            entity.HasOne(d => d.RebateDirectSchema).WithMany(p => p.RebateDirectRules)
                .HasForeignKey(d => d.RebateDirectSchemaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_RebateDirectRule__RebateDirectSchema_Id_fk");

            entity.HasOne(d => d.SourceTradeAccount).WithMany(p => p.RebateDirectRules)
                .HasForeignKey(d => d.SourceTradeAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_RebateDirectRule__TradeAccount_Id_fk");

            entity.HasOne(d => d.TargetAccount).WithMany(p => p.RebateDirectRules)
                .HasForeignKey(d => d.TargetAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_RebateDirectRule__Account_Id_fk");
        });

        modelBuilder.Entity<RebateDirectSchema>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_RebateDirectSchema_pk");

            entity.ToTable("_RebateDirectSchema", "trd");

            entity.HasIndex(e => e.ConfirmedBy, "_RebateDirectSchema_ConfirmedBy_index");

            entity.HasIndex(e => e.CreatedBy, "_RebateDirectSchema_CreatedBy_index");

            entity.HasIndex(e => e.Name, "_RebateDirectSchema_Name_uindex").IsUnique();

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Note)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.ConfirmedByNavigation).WithMany(p => p.RebateDirectSchemaConfirmedByNavigations)
                .HasForeignKey(d => d.ConfirmedBy)
                .HasConstraintName("_RebateDirectSchema__Party_Id_fk");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RebateDirectSchemaCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_RebateDirectSchema__Party_Id_fk2");
        });

        modelBuilder.Entity<RebateDirectSchemaItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_RebateDirectSchemaItem_pk");

            entity.ToTable("_RebateDirectSchemaItem", "trd");

            entity.HasIndex(e => e.RebateDirectSchemaId, "_RebateDirectSchemaItem_RebateRuleId_index");

            entity.HasIndex(e => e.SymbolCode, "_RebateDirectSchemaItem_SymbolCode_index");

            entity.Property(e => e.Commission).HasPrecision(16, 2);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Pips).HasPrecision(16, 2);
            entity.Property(e => e.Rate).HasPrecision(16, 2);
            entity.Property(e => e.SymbolCode).HasMaxLength(64);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.RebateDirectSchema).WithMany(p => p.RebateDirectSchemaItems)
                .HasForeignKey(d => d.RebateDirectSchemaId)
                .HasConstraintName("_RebateDirectSchemaItem__RebateDirectSchema_Id_fk");
        });

        modelBuilder.Entity<RebateSchemaBundle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_RebateSchemaBundle_pk");

            entity.ToTable("_RebateSchemaBundle", "trd");

            entity.HasIndex(e => e.CreatedBy, "_RebateSchemaBundle_CreatedBy_index");

            entity.HasIndex(e => new { e.Name, e.Type }, "_RebateSchemaBundle_Name_Type_uindex").IsUnique();

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Data)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.Name).HasMaxLength(64);
            entity.Property(e => e.Note)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RebateSchemaBundles)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_RebateSchemaBundle__Party_Id_fk");
        });

        modelBuilder.Entity<Referral>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("referrals_pkey");

            entity.ToTable("_Referral", "core");

            entity.HasIndex(e => e.ReferralCodeId, "core_referrals_referral_code_id_index");

            entity.HasIndex(e => e.ReferredPartyId, "core_referrals_referred_party_id_index");

            entity.HasIndex(e => e.ReferrerPartyId, "core_referrals_referrer_party_id_index");
            entity.HasIndex(e => e.Module, "core_referrals_module_index");

            entity.Property(e => e.Id).HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Module)
                .HasMaxLength(128)
                .HasDefaultValueSql("''::character varying");

            entity.HasOne(d => d.ReferralCode).WithMany(p => p.Referrals)
                .HasForeignKey(d => d.ReferralCodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("core_referrals_referral_code_id_foreign");

            entity.HasOne(d => d.ReferredParty).WithMany(p => p.ReferralReferredParties)
                .HasForeignKey(d => d.ReferredPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("core_referrals_referred_party_id_foreign");

            entity.HasOne(d => d.ReferrerParty).WithMany(p => p.ReferralReferrerParties)
                .HasForeignKey(d => d.ReferrerPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("core_referrals_referrer_party_id_foreign");
        });

        modelBuilder.Entity<ReferralCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("referral_codes_pkey");

            entity.ToTable("_ReferralCode", "core");

            entity.HasIndex(e => e.Code, "core_referral_codes_code_unique").IsUnique();

            entity.HasIndex(e => e.PartyId, "core_referral_codes_party_id_index");

            entity.HasIndex(e => e.ServiceType, "core_referral_codes_service_type_index");
            entity.HasIndex(e => e.Status, "core_referral_codes_status_index");

            entity.HasIndex(e => e.IsDefault, "core_referral_codes_is_default_index");

            entity.HasIndex(e => e.IsAutoCreatePaymentMethod, "core_referral_codes_is_auto_create_paymentmethod_index");

            entity.Property(e => e.Id).HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.Code).HasMaxLength(64);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Summary)
                .HasDefaultValueSql("'{}'::json")
                .HasColumnType("json");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party).WithMany(p => p.ReferralCodes)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("core_referral_codes_party_id_foreign");

            entity.HasOne(d => d.Account).WithMany(p => p.ReferralCodes)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("core_referral_codes_account_id_foreign");
        });

        modelBuilder.Entity<ReportRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_ReportRequest_pk");

            entity.ToTable("_ReportRequest", "sto");

            entity.HasIndex(e => e.PartyId, "IX__ReportRequest_PartyId");

            entity.HasIndex(e => e.CreatedOn, "_ReportRequest_CreatedOn_index");

            entity.HasIndex(e => e.Type, "_ReportRequest_Type_index");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.FileName).HasDefaultValueSql("''::text");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Query).HasDefaultValueSql("'{}'::text");

            entity.HasOne(d => d.Party).WithMany(p => p.ReportRequests)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_ReportRequest__Party_Id_fk");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Role_pk");

            entity.ToTable("_Role", "core");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(32);
        });

        modelBuilder.Entity<SalesRebate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_SalesRebate_pkey");

            entity.ToTable("_SalesRebate", "trd");

            entity.HasIndex(e => e.TradeRebateId, "IX__SalesRebate_TradeRebateId");

            entity.HasIndex(e => e.SalesAccountId, "_SalesRebate_SalesAccountId_index");

            entity.Property(e => e.Amount).HasComment("单位是0.01分");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.RebateType)
                .HasDefaultValueSql("'unknow'::character varying")
                .HasColumnType("character varying");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.SalesAccount).WithMany(p => p.SalesRebateSalesAccounts)
                .HasForeignKey(d => d.SalesAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_SalesRebate_SalesAccountId_fkey");

            entity.HasOne(d => d.TradeAccount).WithMany(p => p.SalesRebateTradeAccounts)
                .HasForeignKey(d => d.TradeAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_SalesRebate_TradeAccountId_fk");

            entity.HasOne(d => d.TradeRebate).WithMany(p => p.SalesRebates)
                .HasForeignKey(d => d.TradeRebateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_SalesRebate_TradeRebateId_fkey");

            entity.HasOne(d => d.WalletAdjust).WithMany(p => p.SalesRebates)
                .HasForeignKey(d => d.WalletAdjustId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_SalesRebate_WalletAdjustId_fkey");
        });

        modelBuilder.Entity<SalesRebateSchema>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_SalesRebateSchema_pkey");

            entity.ToTable("_SalesRebateSchema", "trd");
            entity.HasIndex(e => e.SalesType, "_SalesRebateSchema_SalesType_index");
            entity.HasIndex(e => e.Schedule, "_SalesRebateSchema_Schedule_index");

            entity.Property(e => e.AlphaRebate).HasDefaultValueSql("0");
            entity.Property(e => e.OperatorPartyId).HasDefaultValueSql("1");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.ExcludeAccount)
                .HasDefaultValueSql("'[]'::json")
                .HasColumnType("json");
            entity.Property(e => e.ExcludeSymbol)
                .HasDefaultValueSql("'[]'::json")
                .HasColumnType("json");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.RebateAccount).WithMany(p => p.SalesRebateSchemaRebateAccounts)
                .HasForeignKey(d => d.RebateAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_SalesRebateSchema_RebateAccountId_fkey");

            entity.HasOne(d => d.SalesAccount).WithMany(p => p.SalesRebateSchemaSalesAccounts)
                .HasForeignKey(d => d.SalesAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_SalesRebateSchema_SalesAccountId_fkey");

            entity.HasOne(d => d.OperatorParty).WithMany(p => p.OperatedSalesRebateSchemata)
                .HasForeignKey(d => d.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_SalesRebateSchema_OperatorPartyId_fkey");

            // entity.HasOne(d => d.RebateAccount).WithMany(p => p.SalesRebateSchema)
            //     .HasForeignKey(d => d.RebateAccountId)
            //     .HasConstraintName("_SalesRebateSchema_RebateAccountId_fkey");
        });


        modelBuilder.Entity<Site>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Site_pk");

            entity.ToTable("_Site", "core");

            entity.HasIndex(e => e.Name, "_Site_Name_uindex").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(64);
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("states_pkey");

            entity.ToTable("_State", "core");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(64);
        });

        modelBuilder.Entity<Supplement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("supplements_pkey");

            entity.ToTable("_Supplement", "core");

            entity.HasIndex(e => new { e.RowId, e.Type }, "core_supplements_row_id_type_unique").IsUnique();

            entity.Property(e => e.Id).HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Data)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Symbol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Symbol_pk");

            entity.ToTable("_Symbol", "trd");

            entity.HasIndex(e => e.CategoryId, "IX__Symbol_CategoryId");
            entity.HasIndex(e => e.Category, "IX__Symbol_Category");
            // code index
            entity.HasIndex(e => e.Code, "IX__Symbol_Code");
            
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.OperatorPartyId).HasDefaultValueSql("1");

            entity.HasOne(d => d.OperatorParty).WithMany(p => p.OperatedSymbols)
                .HasForeignKey(d => d.OperatorPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Symbol_OperatorPartyId_fkey");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Tag_pk");

            entity.ToTable("_Tag", "core");

            entity.HasIndex(e => e.Name, "_Tag_Name_index");
            entity.HasIndex(e => e.Type, "_Tag_Type_index");

            entity.HasMany(d => d.Parties).WithMany(p => p.Tags)
                .UsingEntity<Dictionary<string, object>>(
                    "PartyWithTag",
                    r => r.HasOne<Party>().WithMany()
                        .HasForeignKey("PartyId")
                        .HasConstraintName("_PartyWithTag__Party_Id_fk"),
                    l => l.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("_PartyWithTag__Tag_Id_fk"),
                    j =>
                    {
                        j.HasKey("PartyId", "TagId").HasName("_PartyWithTag_pk");
                        j.ToTable("_PartyWithTag", "core");
                    });

            entity.HasMany(d => d.Accounts).WithMany(p => p.Tags)
                .UsingEntity<Dictionary<string, object>>(
                    "AccountWithTag",
                    r => r.HasOne<Account>().WithMany()
                        .HasForeignKey("AccountId")
                        .HasConstraintName("_AccountWithTag__Account_Id_fk"),
                    l => l.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("_AccountWithTag__Tag_Id_fk"),
                    j =>
                    {
                        j.HasKey("AccountId", "TagId").HasName("_AccountWithTag_pk");
                        j.ToTable("_AccountWithTag", "trd");
                    });
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Topic_pk");

            entity.ToTable("_Topic", "cms");

            entity.HasIndex(e => new { e.EffectiveFrom, e.EffectiveTo }, "_Topic_EffectiveFrom_EffectiveTo_index");

            entity.HasIndex(e => e.Type, "_Topic_Type_index");

            entity.Property(e => e.Id).HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.AdditionalInformation).HasDefaultValueSql("''::text");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.EffectiveFrom).HasDefaultValueSql("now()");
            entity.Property(e => e.EffectiveTo).HasDefaultValueSql("now()");
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<TopicContent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_TopicContent_pk");

            entity.ToTable("_TopicContent", "cms");

            entity.HasIndex(e => e.TopicId, "IX__TopicContent_TopicId");

            entity.HasIndex(e => new { e.Language, e.TopicId }, "_TopicContent_TopicId_Language_UX").IsUnique();

            entity.Property(e => e.Id).HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.Author).HasMaxLength(128);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Language).HasMaxLength(10);
            entity.Property(e => e.Subtitle)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Topic).WithMany(p => p.TopicContents)
                .HasForeignKey(d => d.TopicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_TopicContent__Topic_Id_fk");
        });

        modelBuilder.Entity<TradeAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("trade_accounts_pkey");

            entity.ToTable("_TradeAccount", "trd");

            entity.HasIndex(e => e.CurrencyId, "IX_trade_accounts_currency_id");

            entity.HasIndex(e => new { e.ServiceId, e.AccountNumber },
                "trd_trade_accounts_service_id_account_number_unique").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Currency).WithMany(p => p.TradeAccounts)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trd_trade_accounts_currency_id_foreign");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.TradeAccount)
                .HasForeignKey<TradeAccount>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trd_trade_accounts_id_foreign");

            entity.HasOne(d => d.RebateBaseSchema).WithMany(p => p.TradeAccounts)
                .HasForeignKey(d => d.RebateBaseSchemaId)
                .HasConstraintName("_TradeAccount__RebateRuleTemplate_Id_fk");

            entity.HasOne(d => d.Service).WithMany(p => p.TradeAccounts)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trd_trade_accounts_service_id_foreign");
        });

        modelBuilder.Entity<TradeAccountPassword>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_TradeAccountPassword_pkey");

            entity.ToTable("_TradeAccountPassword", "trd");

            // Indexes
            entity.HasIndex(e => e.AccountId, "IX_TradeAccountPassword_AccountId");
            entity.HasIndex(e => e.AccountNumber, "IX_TradeAccountPassword_AccountNumber").IsUnique();
            entity.HasIndex(e => e.ServiceId, "IX_TradeAccountPassword_ServiceId");

            // Column configurations
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.AccountId).IsRequired();
            entity.Property(e => e.AccountNumber).IsRequired();
            entity.Property(e => e.ServiceId).IsRequired();
            
            entity.Property(e => e.InitialMainPassword)
                .HasMaxLength(500);
            entity.Property(e => e.InitialInvestorPassword)
                .HasMaxLength(500);
            entity.Property(e => e.InitialPhonePassword)
                .HasMaxLength(500); // Changed from 100 to 500 to accommodate encrypted data
            
            entity.Property(e => e.MainPasswordChangedCount)
                .HasDefaultValue(0);
            entity.Property(e => e.InvestorPasswordChangedCount)
                .HasDefaultValue(0);
            
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn)
                .HasDefaultValueSql("now()");

            // Foreign Keys
            entity.HasOne(d => d.Account)
                .WithMany()
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TradeAccountPassword_Account");

            entity.HasOne(d => d.TradeService)
                .WithMany()
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TradeAccountPassword_TradeService");
        });

        modelBuilder.Entity<TradeAccountPasswordHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_TradeAccountPasswordHistory_pkey");

            entity.ToTable("_TradeAccountPasswordHistory", "trd");

            // Indexes
            entity.HasIndex(e => e.AccountId, "IX_TradeAccountPasswordHistory_AccountId");
            entity.HasIndex(e => e.ChangedOn, "IX_TradeAccountPasswordHistory_ChangedOn")
                .IsDescending();
            entity.HasIndex(e => new { e.AccountId, e.PasswordType }, 
                "IX_TradeAccountPasswordHistory_AccountId_PasswordType");
            entity.HasIndex(e => e.OperatorPartyId, "IX_TradeAccountPasswordHistory_OperatorPartyId");

            // Column configurations
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.AccountId).IsRequired();
            entity.Property(e => e.AccountNumber).IsRequired();
            
            entity.Property(e => e.PasswordType)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.OperationType)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.OperatorRole)
                .HasMaxLength(50);
            entity.Property(e => e.Reason)
                .HasMaxLength(500);
            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(500);
            
            entity.Property(e => e.Success)
                .HasDefaultValue(true);
            entity.Property(e => e.ChangedOn)
                .HasDefaultValueSql("now()");
            
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50);
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500);

            // Foreign Keys
            entity.HasOne(d => d.Account)
                .WithMany()
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TradeAccountPasswordHistory_Account");

            entity.HasOne(d => d.OperatorParty)
                .WithMany()
                .HasForeignKey(d => d.OperatorPartyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_TradeAccountPasswordHistory_OperatorParty");
        });

        modelBuilder.Entity<TradeAccountLoginLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TradeAccountLoginLog_pkey");

            entity.ToTable("_TradeAccountLoginLog", "trd");

            entity.HasIndex(e => e.AccountNumber, "_TradeAccountLoginLog_AccountNumber_index");

            entity.HasIndex(e => e.Ip, "_TradeAccountLoginLog_Ip_index");

            entity.HasIndex(e => e.LoginTime, "_TradeAccountLoginLog_LoginTime_index");

            entity.Property(e => e.Ip)
                .HasMaxLength(64)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.LoginTime).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<TradeAccountStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("trade_account_status_pkey");

            entity.ToTable("_TradeAccountStatus", "trd");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Balance).HasDefaultValueSql("'0'::double precision");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Credit).HasDefaultValueSql("'0'::double precision");
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Equity).HasDefaultValueSql("'0'::double precision");
            entity.Property(e => e.Group)
                .HasMaxLength(64);

            entity.Property(e => e.InterestRate).HasDefaultValueSql("'0'::double precision");
            entity.Property(e => e.Margin).HasDefaultValueSql("'0'::double precision");
            entity.Property(e => e.MarginFree).HasDefaultValueSql("'0'::double precision");
            entity.Property(e => e.MarginLevel).HasDefaultValueSql("'0'::double precision");
            entity.Property(e => e.PrevBalance).HasDefaultValueSql("'0'::double precision");
            entity.Property(e => e.PrevMonthBalance).HasDefaultValueSql("'0'::double precision");
            entity.Property(e => e.ReadOnlyCode)
                .HasMaxLength(64)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Taxes).HasDefaultValueSql("'0'::double precision");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.IdTradeNavigation)
                .WithOne(p => p.TradeAccountStatus)
                .HasForeignKey<TradeAccountStatus>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trd_trade_account_status_id_foreign");

            entity.HasOne(d => d.IdNavigation)
                .WithOne(p => p.TradeAccountStatus)
                .HasForeignKey<TradeAccountStatus>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trd_account_status_id_foreign");
        });

        modelBuilder.Entity<TradeDemoAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("trade_demo_accounts_pkey");

            entity.ToTable("_TradeDemoAccount", "trd");

            entity.HasIndex(e => e.PartyId, "IX_trade_demo_accounts_party_id");

            entity.HasIndex(e => new { e.ServiceId, e.AccountNumber },
                "trd_trade_demo_accounts_service_id_account_number_unique").IsUnique();

            entity.Property(e => e.Id).HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.CountryCode)
                .HasMaxLength(32)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Email)
                .HasMaxLength(128)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.ExpireOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(128)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.ReferralCode)
                .HasMaxLength(512)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party).WithMany(p => p.TradeDemoAccounts)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trd_trade_demo_accounts_party_id_foreign");

            entity.HasOne(d => d.Service).WithMany(p => p.TradeDemoAccounts)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trd_trade_demo_accounts_service_id_foreign");
        });

        modelBuilder.Entity<TradeRebate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ix_trade_rebate_k8s_id");

            entity.ToTable("trade_rebate_k8s", "trd");

            entity.HasIndex(e => e.CurrencyId, "ix_trade_rebate_k8s_currency_id");

            entity.HasIndex(e => e.AccountNumber, "ix_trade_rebate_k8s_account_number");

            entity.HasIndex(e => e.Action, "ix_trade_rebate_k8s_action");

            entity.HasIndex(e => e.Status, "ix_trade_rebate_k8s_status");

            entity.HasIndex(e => e.TimeStamp, "ix_trade_rebate_k8s_time_stamp");

            entity.HasIndex(e => e.AccountId, "ix_trade_rebate_k8s_account_id");
            entity.HasIndex(e => e.ReferPath, "ix_trade_rebate_k8s_refer_path");

            entity.HasIndex(e => new { e.TradeServiceId, e.Ticket, e.ClosedOn }, "ux_trade_rebate_k8s_ticket_service_closed")
                .IsUnique();

            entity.HasIndex(e => e.Commission, "ix_trade_rebate_k8s_commission");
            entity.HasIndex(e => e.Swaps, "ix_trade_rebate_k8s_swaps");
            entity.HasIndex(e => e.Profit, "ix_trade_rebate_k8s_profit");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.TradeServiceId).HasColumnName("trade_service_id");
            entity.Property(e => e.Ticket).HasColumnName("ticket");
            entity.Property(e => e.AccountNumber).HasColumnName("account_number");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.Volume).HasColumnName("volume");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.RuleType).HasColumnName("rule_type");
            entity.Property(e => e.CreatedOn).HasColumnName("created_on").HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasColumnName("updated_on").HasDefaultValueSql("now()");
            entity.Property(e => e.ClosedOn).HasColumnName("closed_on").HasDefaultValueSql("now()");
            entity.Property(e => e.OpenedOn).HasColumnName("opened_on").HasDefaultValueSql("now()");
            entity.Property(e => e.TimeStamp).HasColumnName("time_stamp");
            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.DealId).HasColumnName("deal_id").HasDefaultValue(0);
            entity.Property(e => e.Symbol).HasColumnName("symbol").HasMaxLength(20);
            entity.Property(e => e.ReferPath).HasColumnName("refer_path").HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Commission).HasColumnName("commission");
            entity.Property(e => e.Swaps).HasColumnName("swaps");
            entity.Property(e => e.OpenPrice).HasColumnName("open_price");
            entity.Property(e => e.ClosePrice).HasColumnName("close_price");
            entity.Property(e => e.Profit).HasColumnName("profit");
            entity.Property(e => e.Reason).HasColumnName("reason");

            entity.HasOne(d => d.Currency).WithMany(p => p.TradeRebates)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trade_rebate_k8s_currency_id");

            entity.HasOne(d => d.Account).WithMany(p => p.TradeRebates)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("fk_trade_rebate_k8s_account_id");

            entity.HasOne(d => d.TradeService).WithMany(p => p.TradeRebates)
                .HasForeignKey(d => d.TradeServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trade_rebate_k8s_trade_service_id");
        });

        modelBuilder.Entity<TradeService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("trade_services_pkey");

            entity.ToTable("_TradeService", "trd");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Configuration).HasDefaultValueSql("''::text");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Description)
                .HasMaxLength(128)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.IsAllowAccountCreation).HasDefaultValueSql("1");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Platform).HasDefaultValueSql("'0'::smallint");
            entity.Property(e => e.Priority).HasDefaultValueSql("1");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Transaction_pk");

            entity.ToTable("_Transaction", "acct");

            entity.HasIndex(e => e.CurrencyId, "IX__Transaction_CurrencyId");

            entity.HasIndex(e => e.FundType, "IX__Transaction_FundType");

            entity.HasIndex(e => e.PartyId, "IX__Transaction_PartyId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.ReferenceNumber)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");

            entity.HasOne(d => d.Currency).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Transaction__Currency_Id_fk");

            entity.HasOne(d => d.FundTypeNavigation).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.FundType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Transaction__FundType_Id_fk");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Transaction)
                .HasForeignKey<Transaction>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Transaction__Matter_Id_fk");

            entity.HasOne(d => d.Party).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Transaction__Party_Id_fk");
        });

        modelBuilder.Entity<Refund>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Refund_pk");

            entity.ToTable("_Refund", "acct");

            entity.HasIndex(e => e.PartyId, "IX__Refund_PartyId");
            entity.HasIndex(e => e.TargetType, "IX__Refund_TargetType");
            entity.HasIndex(e => e.TargetId, "IX__Refund_TargetId");
            entity.HasIndex(e => e.CurrencyId, "IX__Refund_CurrencyId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Comment)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");

            entity.HasOne(d => d.Currency).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Refund__Currency_Id_fk");

            entity.HasOne(d => d.FundTypeNavigation).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.FundType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Refund__FundType_Id_fk");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Refund)
                .HasForeignKey<Refund>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Refund__Matter_Id_fk");

            entity.HasOne(d => d.Party).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Refund__Party_Id_fk");
        });

        modelBuilder.Entity<TransferView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TransferView", "acct");
        });

        modelBuilder.Entity<Transition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transitions_pkey");

            entity.ToTable("_Transition", "core");

            entity.HasIndex(e => e.ActionId, "IX_transitions_action_id");

            entity.HasIndex(e => e.OnStateId, "IX_transitions_on_state_id");

            entity.HasIndex(e => e.ToStateId, "IX_transitions_to_state_id");

            entity.HasIndex(e => new { e.RoleId, e.OnStateId, e.ToStateId, e.ActionId }, "_Transition_pk").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Action).WithMany(p => p.Transitions)
                .HasForeignKey(d => d.ActionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("core_transitions_action_id_foreign");

            entity.HasOne(d => d.OnState).WithMany(p => p.TransitionOnStates)
                .HasForeignKey(d => d.OnStateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("core_transitions_on_state_id_foreign");

            entity.HasOne(d => d.Role).WithMany(p => p.Transitions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Transition__Role_Id_fk");

            entity.HasOne(d => d.ToState).WithMany(p => p.TransitionToStates)
                .HasForeignKey(d => d.ToStateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("core_transitions_to_state_id_foreign");
        });

        modelBuilder.Entity<Verification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Verification_pk");

            entity.ToTable("_Verification", "core");

            entity.HasIndex(e => new { e.PartyId, e.Type }, "_Verification_PartyId_Type_pk").IsUnique();

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Note).HasDefaultValueSql("''::text");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party).WithMany(p => p.Verifications)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Verification__Party_Id_fk");
        });

        modelBuilder.Entity<VerificationItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_VerificationItem_pk");

            entity.ToTable("_VerificationItem", "core");

            entity.HasIndex(e => e.Category, "_VerificationItem_Category_index");

            // entity.HasIndex(e => new { e.VerificationId, e.Category }, "_VerificationItem_Vid_Category_pk").IsUnique();

            entity.Property(e => e.Category).HasMaxLength(20);
            entity.Property(e => e.Content)
                .HasDefaultValueSql("'{}'::json")
                .HasColumnType("json");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Verification).WithMany(p => p.VerificationItems)
                .HasForeignKey(d => d.VerificationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_VerificationItem__Verification_Id_fk");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallets_pkey");

            entity.ToTable("_Wallet", "acct");

            entity.HasIndex(e => e.CurrencyId, "IX_wallets_currency_id");

            entity.HasIndex(e => e.FundType, "IX_wallets_type");

            entity.HasIndex(e => e.Number, "_Wallet_Number_uindex").IsUnique();

            entity.HasIndex(e => e.PartyId, "acct_wallets_party_id_index");

            entity.HasIndex(e => new { e.PartyId, e.FundType, e.CurrencyId },
                "acct_wallets_party_id_type_currency_id_unique").IsUnique();

            entity.Property(e => e.Id).HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.Number)
                .HasMaxLength(64)
                .HasDefaultValueSql("''::character varying");
            entity.Property(e => e.Sequence).HasDefaultValueSql("999");
            entity.Property(e => e.TalliedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Currency).WithMany(p => p.Wallets)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_wallets_currency_id_foreign");

            entity.HasOne(d => d.FundTypeNavigation).WithMany(p => p.Wallets)
                .HasForeignKey(d => d.FundType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_wallets_type_foreign");

            entity.HasOne(d => d.Party).WithMany(p => p.Wallets)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_wallets_party_id_foreign");
        });

        modelBuilder.Entity<WalletAdjust>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallet_adjust_pkey");
            entity.ToTable("_WalletAdjust", "acct");

            entity.HasIndex(e => e.WalletId, "IX_wallet_adjusts_wallet_id");
            entity.HasIndex(e => e.SourceType, " IX_wallet_adjusts_source_type");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.Comment)
                .HasMaxLength(64)
                .HasDefaultValueSql("''::character varying");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletAdjusts)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_wallet_adjusts_wallet_id_foreign");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.WalletAdjust)
                .HasForeignKey<WalletAdjust>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_wallet_adjusts_id_foreign");
        });

        modelBuilder.Entity<WalletDailySnapshot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("WalletDailySnapshot_pkey");
            entity.ToTable("_WalletDailySnapshot", "acct");

            entity.HasIndex(e => e.WalletId, "IX_wallet_daily_snapshots_wallet_id");
            entity.Property(e => e.SnapshotDate).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletDailySnapshots)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_wallet_daily_snapshots_wallet_id_foreign");
        });

        modelBuilder.Entity<WalletPaymentMethodAccess>(entity =>
        {
            entity.HasKey(e => new { e.WalletId, e.PaymentMethodId }).HasName("wallet_payment_method_access_pkey");
            entity.ToTable("_WalletPaymentMethodAccess", "acct");

            entity.HasIndex(e => e.Status, "IX_wallet_payment_method_access_status");
            entity.HasIndex(e => new { e.WalletId, e.PaymentMethodId },
                    "IX_wallet_payment_method_access_wallet_id_payment_method_id_unique")
                .IsUnique();

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.WalletPaymentMethodAccesses)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_wallet_payment_method_access_payment_method_id_foreign");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletPaymentMethodAccesses)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_wallet_payment_method_access_wallet_id_foreign");

            entity.HasOne(d => d.OperatedParty).WithMany(p => p.OperatedWalletPaymentMethodAccesses)
                .HasForeignKey(d => d.OperatedPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("acct_wallet_payment_method_access_operated_party_id_foreign");
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ix_wallet_transaction_k8s_id");

            entity.ToTable("wallet_transaction_k8s", "acct");

            entity.HasIndex(e => e.InvoiceId, "ix_wallet_transaction_k8s_invoice_id");

            entity.HasIndex(e => e.MatterId, "ix_wallet_transaction_k8s_matter_id");

            entity.HasIndex(e => e.WalletId, "ix_wallet_transaction_k8s_wallet_id");

            entity.Property(e => e.Id).HasColumnName("id").HasIdentityOptions(10000L, null, null, null, null, null);
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.MatterId).HasColumnName("matter_id");
            entity.Property(e => e.PrevBalance).HasColumnName("prev_balance");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CreatedOn).HasColumnName("created_on").HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasColumnName("updated_on").HasDefaultValueSql("now()");

            entity.HasOne(d => d.Invoice).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("fk_wallet_transaction_k8s_invoice_id");

            entity.HasOne(d => d.Matter).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.MatterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_wallet_transaction_k8s_matter_id");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_wallet_transaction_k8s_wallet_id");
        });

        modelBuilder.Entity<Withdrawal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Withdrawal_pk");

            entity.ToTable("_Withdrawal", "acct");

            entity.HasIndex(e => e.PartyId, "IX__Withdrawal_PartyId");

            entity.HasIndex(e => e.PaymentId, "IX__Withdrawal_PaymentId");

            entity.HasIndex(e => e.SourceAccountId, "IX__Withdrawal_SourceTradeAccountId");

            entity.HasIndex(e => e.CurrencyId, "_Withdrawal_CurrencyId_index");

            entity.HasIndex(e => e.FundType, "_Withdrawal_FundType_index");

            entity.HasIndex(e => e.ApprovedOn, "_Withdrawal_ApprovedOn_index");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ExchangeRate).HasPrecision(16, 8);
            entity.Property(e => e.ReferenceNumber)
                .HasMaxLength(256)
                .HasDefaultValueSql("''::character varying");

            entity.HasOne(d => d.Currency).WithMany(p => p.Withdrawals)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Withdrawal__Currency_Id_fk");

            entity.HasOne(d => d.FundTypeNavigation).WithMany(p => p.Withdrawals)
                .HasForeignKey(d => d.FundType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Withdrawal__FundType_Id_fk");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Withdrawal)
                .HasForeignKey<Withdrawal>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Withdrawal__Matters_Id_fk");

            entity.HasOne(d => d.Party).WithMany(p => p.Withdrawals)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Withdrawal__Party_Id_fk");

            entity.HasOne(d => d.Payment).WithMany(p => p.Withdrawals)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Withdrawal__Payment_Id_fk");

            entity.HasOne(d => d.SourceAccount).WithMany(p => p.Withdrawals)
                .HasForeignKey(d => d.SourceAccountId)
                .HasConstraintName("_Withdrawal__Account_Id_fk");

            entity.HasOne(d => d.SourceWallet).WithMany(p => p.Withdrawals)
                .HasForeignKey(d => d.SourceWalletId)
                .HasConstraintName("_Withdrawal__Wallet_Id_fk");
        });

        modelBuilder.Entity<Case>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_Case_pk");
            entity.ToTable("_Case", "sto");

            entity.HasIndex(e => e.ReplyId, "IX__Case_ReplyId");
            entity.HasIndex(e => e.PartyId, "IX__Case_PartyId");
            entity.HasIndex(e => e.CaseId, "IX__Case_CaseId");
            entity.HasIndex(e => e.Status, "IX__Case_Status");
            entity.HasIndex(e => e.CategoryId, "IX__Case_CategoryId");
            entity.HasIndex(e => e.IsAdmin, "IX__Case_IsAdmin");

            entity.Property(e => e.CaseId).HasDefaultValueSql("''::text");
            entity.Property(e => e.Data).HasDefaultValueSql("'{}'::json").HasColumnType("json");
            entity.Property(e => e.Files).HasDefaultValueSql("'[]'::json").HasColumnType("json");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Party).WithMany(p => p.Cases)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Case__Party_Id_fk");

            entity.HasOne(d => d.AdminParty).WithMany(p => p.AdminCases)
                .HasForeignKey(d => d.AdminPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Case__Admin_Party_Id_fk");

            entity.HasOne(d => d.Category).WithMany(p => p.Cases)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("_Case__Category_Id_fk");

            entity.HasOne(d => d.Reply).WithMany(p => p.InverseReply)
                .HasForeignKey(d => d.ReplyId)
                .HasConstraintName("_Case__Reply_Id_fk");

            entity.HasMany(d => d.CaseLanguages).WithOne(p => p.Case)
                .HasForeignKey(d => d.CaseId)
                .HasConstraintName("_Case__Language_Id_fk");
        });

        modelBuilder.Entity<CaseCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_CaseCategory_pk");
            entity.ToTable("_CaseCategory", "sto");

            entity.HasIndex(e => e.ParentId, "IX__CaseCategory_ParentId");
            entity.HasIndex(e => e.Name, "IX__CaseCategory_Name");
            entity.HasIndex(e => e.Status, "IX__CaseCategory_Status");
            entity.HasIndex(e => e.Role, "IX__CaseCategory_Role");

            entity.Property(e => e.Name).HasMaxLength(64);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedOn).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Parent).WithMany(p => p.ChildCaseCategories)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("_CaseCategory__Parent_Id_fk");
        });

        modelBuilder.Entity<CaseLanguage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("_CaseLanguage_pk");
            entity.ToTable("_CaseLanguage", "sto");

            entity.HasIndex(e => e.CaseId, "IX__CaseLanguage_CaseId");
            entity.HasIndex(e => e.Language, "IX__CaseLanguage_Language");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}