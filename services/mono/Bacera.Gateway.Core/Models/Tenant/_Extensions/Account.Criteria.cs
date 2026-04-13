namespace Bacera.Gateway;

using M = Account;

partial class Account : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? Uid { get; set; }
        public List<long>? Uids { get; set; }
        public long? AccountNumber { get; set; }
        public List<long>? AccountNumbers { get; set; }
        public AccountRoleTypes? Role { get; set; }
        public List<short>? Roles { get; set; }
        public int? ServiceId { get; set; }

        public int? Level { get; set; }
        public string? Code { get; set; }
        public string? CodeUid { get; set; } // special Code which is used in Tenant search account page only
        public string? Email { get; set; }
        public long? PartyId { get; set; }
        public string? Keywords { get; set; }
        public string? SearchText { get; set; }
        public AccountTypes? Type { get; set; }
        public string? TagName { get; set; }
        public bool? HasTradeAccount { get; set; }
        public long? ReferrerAccountId { get; set; }
        public AccountStatusTypes? Status { get; set; }
        public List<AccountStatusTypes>? Statuses { get; set; }
        public string? Group { get; set; }
        public CurrencyTypes? CurrencyId { get; set; }
        public FundTypes? FundType { get; set; }
        public SiteTypes? SiteId { get; set; }
        public bool? IncludeClosed { get; set; }
        public long? GroupId { get; set; }
        public long? SalesId { get; set; }
        public long? SalesUid { get; set; }
        public long? AgentId { get; set; }
        public long? AgentUid { get; set; }
        public long? ParentAccountUid { get; set; }
        public bool? IncludeParentAccountUid { get; set; }
        public long? ChildParentAccountUid { get; set; }
        public int? RelativeLevel { get; set; }
        public bool? IsTopAgent { get; set; }
        public string? ReferPath { get; set; }

        public string? PathStartWith { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool? IsActive { get; set; }


        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Uid.Equals(Uid), Uid.HasValue);
            pool.Add(x => x.IsClosed == 0 && x.Status == 0, IncludeClosed is not true);
            pool.Add(x => x.Type == (short)Type!, Type.HasValue);
            pool.Add(x => x.Status == (short)Status!, Status.HasValue);
            pool.Add(x => Statuses!.Contains((AccountStatusTypes)x.Status), Statuses != null && Statuses.Any());
            pool.Add(x => x.Role == (short)Role!, Role.IsTangible());
            pool.Add(x => Roles!.Contains(x.Role), Roles != null && Roles.Any() && Roles.IsTangible());
            pool.Add(x => Uids!.Contains(x.Uid), Uids != null && Uids.Any() && Uids.IsTangible());
            pool.Add(x => x.PartyId == PartyId, PartyId.IsTangible());
            pool.Add(x => x.HasTradeAccount == HasTradeAccount, HasTradeAccount.HasValue);
            pool.Add(x => x.ReferrerAccountId == ReferrerAccountId, ReferrerAccountId.IsTangible());
            pool.Add(x => x.CurrencyId == (int)CurrencyId!, CurrencyId.HasValue);
            pool.Add(x => x.FundType == (int)FundType!, FundType.HasValue);
            pool.Add(x => x.Party.SiteId == (int)SiteId!, SiteId.HasValue);
            pool.Add(x => x.AccountNumber == AccountNumber!, AccountNumber.HasValue);
            pool.Add(x => x.ServiceId == ServiceId!, ServiceId.HasValue);
            pool.Add(x => x.Party.Email == Email!, Email != null);

            pool.Add(x => (x.Uid != ParentAccountUid)
                          && x.ReferPath.Contains(ParentAccountUid!.Value.ToString()),
                ParentAccountUid != null && IncludeParentAccountUid is not true);

            pool.Add(x => x.ReferPath.Contains(ParentAccountUid!.Value.ToString()),
                ParentAccountUid != null && IncludeParentAccountUid == true);

            pool.Add(x => x.Uid != ChildParentAccountUid
                          && x.ReferPath.Contains(ParentAccountUid!.Value.ToString())
                          && x.ReferPath.Contains(ChildParentAccountUid!.Value.ToString()),
                ChildParentAccountUid != null && ParentAccountUid != null);

            if (SearchText != null)
            {
                var texts = SearchText
                    .Split('&', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim());
                foreach (var text in texts)
                {
                    var accountNumbers = text.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Select(x => long.TryParse(x, out var result) ? result : (long?)null)
                        .Where(x => x.HasValue)
                        .Select(x => x!.Value)
                        .ToList();

                    if (accountNumbers.Count > 1)
                    {
                        // Check if any of the numbers are wallet IDs by checking if accounts exist with matching wallets
                        pool.Add(x => accountNumbers.Contains(x.AccountNumber) 
                            || accountNumbers.Contains(x.Uid)
                            || x.Party.Wallets.Any(w => accountNumbers.Contains(w.Id) 
                                && w.FundType == x.FundType 
                                && w.CurrencyId == x.CurrencyId));
                        continue;
                    }

                    // Single number could be AccountNumber, Uid, or WalletId
                    if (accountNumbers.Count == 1)
                    {
                        var singleNumber = accountNumbers[0];
                        pool.Add(x => x.AccountNumber == singleNumber 
                            || x.Uid == singleNumber
                            || x.Party.Wallets.Any(w => w.Id == singleNumber 
                                && w.FundType == x.FundType 
                                && w.CurrencyId == x.CurrencyId));
                        continue;
                    }

                    pool.Add(x => x.SearchText.ToLower().Contains(text.ToLower())
                                 || x.Party.LastLoginIp.Contains(text)
                                 || x.Party.RegisteredIp.Contains(text));
                }
            }

            pool.Add(x => AccountNumbers!.Contains(x.TradeAccount!.AccountNumber),
                AccountNumbers != null && AccountNumbers.Any());

            pool.Add(x => x.Tags.Any(t => t.Name == TagName), TagName != null);

            pool.Add(
                x => x.Role == (int)AccountRoleTypes.Agent && (x.AgentAccountId == null || x.AgentAccountId == x.Id),
                IsTopAgent is true);

            pool.Add(x => x.ReferPath == ReferPath!, !string.IsNullOrEmpty(ReferPath));
            pool.Add(x => x.ReferPath.StartsWith(PathStartWith!), !string.IsNullOrEmpty(PathStartWith));
            pool.Add(x => x.Level == Level, Level is > 0);

            pool.Add(x => x.Code == Code, Code != null);
            pool.Add(x => x.Group == Group, Group != null);

            pool.Add(x => x.SalesAccountId == SalesId, SalesId != null);
            pool.Add(x => x.AgentAccountId == AgentId, AgentId != null);
            // Use ReferPath to include the sales account itself and all its children
            pool.Add(x => x.ReferPath.Contains(SalesUid!.Value.ToString()), SalesUid != null);
            pool.Add(x => x.AgentAccount != null && x.AgentAccount.Uid == AgentUid, AgentUid != null);
            pool.Add(x => x.CreatedOn >= From, From != null);
            pool.Add(x => x.CreatedOn <= To, To != null);

            pool.Add(x => x.ActiveOn != null, IsActive == true);
            pool.Add(x => x.ActiveOn == null, IsActive == false);
        }
    }

    public sealed class ClientCriteria : BaseEntityCriteria<M>
    {
        public ClientCriteria()
        {
            SortField = nameof(Id);
        }

        public long? Uid { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public long? PartyId { get; set; }

        public bool HasTradeAccount { get; set; }
        public List<long>? Uids { get; set; }
        public long? AccountNumber { get; set; }
        public List<long>? AccountNumbers { get; set; }
        public AccountRoleTypes? Role { get; set; }
        public List<short>? Roles { get; set; }
        public int? ServiceId { get; set; }
        public bool? IncludeClosed { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.IsClosed == 0 && x.Status == 0, IncludeClosed is not true);

            pool.Add(x => x.Uid == Uid, Uid.HasValue);

            pool.Add(x => x.PartyId == PartyId, PartyId.HasValue);

            pool.Add(x => x.Role == (short)Role!, Role != null);

            pool.Add(x => Roles!.Contains(x.Role), Roles != null && Roles.Any() && Roles != null);

            pool.Add(x => Uids!.Contains(x.Uid), Uids != null && Uids.Any() && Uids != null);

            pool.Add(x => x.AccountNumber == AccountNumber!, AccountNumber.HasValue);

            pool.Add(x => x.ServiceId == ServiceId!, ServiceId.HasValue);

            pool.Add(x => AccountNumbers!.Contains(x.TradeAccount!.AccountNumber),
                AccountNumbers != null && AccountNumbers.Any());

            pool.Add(x => x.HasTradeAccount == HasTradeAccount && x.AccountNumber > 0, HasTradeAccount);
        }
    }


    public sealed class SalesCriteria : BaseEntityCriteria<M>
    {
        public SalesCriteria()
        {
            SortField = nameof(CreatedOn);
        }

        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
        public long SalesUid { get; set; }

        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
        public int QueryLevel { get; set; }

        public long? Uid { get; set; }

        /// <summary>
        /// For Sales to see child account's accounts
        /// </summary>
        public long? ParentAccountUid { get; set; }

        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
        public int? ParentAccountLevel { get; set; }

        public AccountRoleTypes? Role { get; set; }
        public List<short>? Roles { get; set; }

        public bool? IsActive { get; set; }
        public string? SearchText { get; set; }
        public bool? MultiLevel { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public dynamic? LevelAccountsInBetween { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Status == 0);
            pool.Add(x => x.ReferPath.Contains(SalesUid.ToString()) && x.Uid != SalesUid);

            pool.Add(x => x.Level == QueryLevel + 1, MultiLevel == false && ParentAccountUid == null);
            pool.Add(x => x.Level == ParentAccountLevel + 1, MultiLevel == false && ParentAccountLevel != null);

            pool.Add(x => x.ReferPath.Contains(ParentAccountUid!.Value.ToString()) && x.Uid != ParentAccountUid, ParentAccountUid != null);

            pool.Add(x => x.Uid == Uid, Uid != null);
            pool.Add(x => x.Role == (short)Role!, Role != null);
            pool.Add(x => Roles!.Contains(x.Role), Roles != null && Roles.Count != 0 && Roles != null);
            pool.Add(x => x.CreatedOn >= From, From != null);
            pool.Add(x => x.CreatedOn <= To, To != null);
            pool.Add(x => x.ActiveOn != null, IsActive == true);
            pool.Add(x => x.ActiveOn == null, IsActive == false);
            pool.Add(x => x.SearchText.ToLower().Contains(SearchText!.ToLower())
                         || x.Party.LastLoginIp.Contains(SearchText!)
                         || x.Party.RegisteredIp.Contains(SearchText!), SearchText != null);
        }
    }
}

public static class AccountCriteriaExtensions
{
    public static Account.Criteria IncludeClosed(this Account.Criteria criteria)
    {
        criteria.IncludeClosed = true;
        return criteria;
    }
}