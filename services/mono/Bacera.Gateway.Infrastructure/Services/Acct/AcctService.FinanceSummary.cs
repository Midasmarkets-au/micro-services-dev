using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services.Acct;

public partial class AcctService
{
    // Model for PaymentMethodGroup configuration
    private class PaymentMethodGroupConfig
    {
        public List<string> Key { get; set; } = new();
        public PaymentMethodGroupInfo Group { get; set; } = new();
        public string type { get; set; } = string.Empty; // "deposit" or "withdrawal"
    }

    // Model for above Group 
    private class PaymentMethodGroupInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public async Task<List<FinanceSummaryRow>> GetDepositWithdrawSummaryAsync(long? partyId = null)
    {
        var depositCompleted = (int)StateTypes.DepositCompleted;
        var withdrawalCompleted = (int)StateTypes.WithdrawalCompleted;

        // Load PaymentMethodGroups configuration
        var paymentMethodGroupsConfig = await LoadPaymentMethodGroupsConfigAsync();
        var paymentMethodIdToGroupMap = BuildPaymentMethodGroupMap(paymentMethodGroupsConfig);
        var paymentMethodIdToGroupIdMap = BuildPaymentMethodGroupIdMap(paymentMethodGroupsConfig);
        var paymentMethodIdToMethodTypeMap = BuildPaymentMethodTypeMap(paymentMethodGroupsConfig);

        // Get deposit groups - group by PaymentMethodId instead of FundType
        var depositsQ = tenantCtx.Deposits
            .Include(x => x.Payment)
                .ThenInclude(x => x.PaymentMethod)
            .Where(x => x.IdNavigation.StateId == depositCompleted && x.Payment != null && x.Payment.PaymentMethod != null);
        if (partyId.HasValue) depositsQ = depositsQ.Where(x => x.PartyId == partyId.Value);

        var depositGroups = await depositsQ
            .GroupBy(x => new { x.PartyId, PaymentMethodId = x.Payment!.PaymentMethod!.Id, x.CurrencyId })
            .Select(g => new
            {
                g.Key.PartyId,
                g.Key.PaymentMethodId,
                g.Key.CurrencyId,
                Amount = g.Sum(y => y.Amount)
            })
            .ToListAsync();

        // Get withdrawal groups - group by PaymentMethodId instead of FundType
        var withdrawalsQ = tenantCtx.Withdrawals
            .Include(x => x.Payment)
                .ThenInclude(x => x.PaymentMethod)
            .Where(x => x.IdNavigation.StateId == withdrawalCompleted && x.SourceAccountId > 0 && x.Payment != null && x.Payment.PaymentMethod != null);
        if (partyId.HasValue) withdrawalsQ = withdrawalsQ.Where(x => x.PartyId == partyId.Value);

        var withdrawalGroups = await withdrawalsQ
            .GroupBy(x => new { x.PartyId, PaymentMethodId = x.Payment!.PaymentMethod!.Id, x.CurrencyId })
            .Select(g => new
            {
                g.Key.PartyId,
                g.Key.PaymentMethodId,
                g.Key.CurrencyId,
                Amount = g.Sum(y => y.Amount)
            })
            .ToListAsync();

        // Get party Id and Email dictionary { id, email }
        var partyIds = depositGroups.Select(x => x.PartyId)
            .Union(withdrawalGroups.Select(x => x.PartyId))
            .Distinct()
            .ToList();

        var partyEmails = await tenantCtx.Parties
            .Where(x => partyIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Email })
            .ToDictionaryAsync(x => x.Id, x => x.Email ?? "");

        // Group by PartyId + PaymentMethodId and convert to USD
        var summaryMap = new Dictionary<(long partyId, long paymentMethodId, string type), FinanceSummaryRow>();

        // Process deposits
        foreach (var d in depositGroups)
        {
            var methodType = GetPaymentMethodType(d.PaymentMethodId, paymentMethodIdToMethodTypeMap);
            // Only process if methodType is "deposit"
            if (!string.Equals(methodType, "deposit", StringComparison.OrdinalIgnoreCase))
                continue;

            var groupName = GetPaymentMethodGroupName(d.PaymentMethodId, paymentMethodIdToGroupMap);
            var groupId = GetPaymentMethodGroupId(d.PaymentMethodId, paymentMethodIdToGroupIdMap);
            var key = (d.PartyId, d.PaymentMethodId, "Deposit");
            if (!summaryMap.TryGetValue(key, out var row))
            {
                row = new FinanceSummaryRow
                {
                    PartyId = d.PartyId,
                    Email = partyEmails.GetValueOrDefault(d.PartyId, string.Empty),
                    PaymentMethodId = d.PaymentMethodId,
                    PaymentGroupName = groupName,
                    PaymentGroupId = groupId,
                    Type = "Deposit",
                    DepositSumUsd = 0,
                    WithdrawSumUsd = 0
                };
                summaryMap[key] = row;
            }

            // Convert to USD using same logic as rebate calculation
            var amountInUsd = await ConvertToUsdAsync(d.Amount, (CurrencyTypes)d.CurrencyId);
            row.DepositSumUsd += amountInUsd;
        }

        // Process withdrawals
        foreach (var w in withdrawalGroups)
        {
            var methodType = GetPaymentMethodType(w.PaymentMethodId, paymentMethodIdToMethodTypeMap);
            // Only process if methodType is "withdrawal"
            if (!string.Equals(methodType, "withdrawal", StringComparison.OrdinalIgnoreCase))
                continue;

            var groupName = GetPaymentMethodGroupName(w.PaymentMethodId, paymentMethodIdToGroupMap);
            var groupId = GetPaymentMethodGroupId(w.PaymentMethodId, paymentMethodIdToGroupIdMap);
            var key = (w.PartyId, w.PaymentMethodId, "Withdrawal");
            if (!summaryMap.TryGetValue(key, out var row))
            {
                row = new FinanceSummaryRow
                {
                    PartyId = w.PartyId,
                    Email = partyEmails.GetValueOrDefault(w.PartyId, string.Empty),
                    PaymentMethodId = w.PaymentMethodId,
                    PaymentGroupName = groupName,
                    PaymentGroupId = groupId,
                    Type = "Withdrawal",
                    DepositSumUsd = 0,
                    WithdrawSumUsd = 0
                };
                summaryMap[key] = row;
            }

            // Convert to USD using same logic as rebate calculation
            var amountInUsd = await ConvertToUsdAsync(w.Amount, (CurrencyTypes)w.CurrencyId);
            row.WithdrawSumUsd += amountInUsd;
        }

        // Calculate need withdraw amount
        foreach (var row in summaryMap.Values)
        {
            row.NeedWithdrawAmountUsd = Math.Max(row.DepositSumUsd - row.WithdrawSumUsd, 0);
        }

        return summaryMap.Values
            .OrderBy(x => x.PartyId)
            .ThenBy(x => x.PaymentMethodId)
            .ToList();
    }

    private async Task<List<PaymentMethodGroupConfig>> LoadPaymentMethodGroupsConfigAsync()
    {
        try
        {
            var config = await tenantCtx.Configurations
                .Where(x => x.Category == "Public" && x.RowId == 0 && x.Key == "PaymentMethodGroups")
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(config))
                return new List<PaymentMethodGroupConfig>();

            return JsonConvert.DeserializeObject<List<PaymentMethodGroupConfig>>(config) 
                ?? new List<PaymentMethodGroupConfig>();
        }
        catch
        {
            logger.LogWarning("Failed to load PaymentMethodGroups configuration");
            return new List<PaymentMethodGroupConfig>();
        }
    }

    private Dictionary<long, string> BuildPaymentMethodGroupMap(List<PaymentMethodGroupConfig> configs)
    {
        var map = new Dictionary<long, string>();
        foreach (var config in configs)
        {
            foreach (var keyStr in config.Key)
            {
                if (long.TryParse(keyStr, out var paymentMethodId))
                {
                    map[paymentMethodId] = config.Group.Name;
                }
            }
        }
        return map;
    }

    private string GetPaymentMethodGroupName(long paymentMethodId, Dictionary<long, string> groupMap)
    {
        return groupMap.TryGetValue(paymentMethodId, out var groupName) && !string.IsNullOrEmpty(groupName)
            ? groupName
            : paymentMethodId.ToString(); // Fallback to PaymentMethodId if not in config
    }

    private Dictionary<long, string> BuildPaymentMethodGroupIdMap(List<PaymentMethodGroupConfig> configs)
    {
        var map = new Dictionary<long, string>();
        foreach (var config in configs)
        {
            foreach (var keyStr in config.Key)
            {
                if (long.TryParse(keyStr, out var paymentMethodId))
                {
                    map[paymentMethodId] = config.Group.Id.ToString();
                }
            }
        }
        return map;
    }

    private string GetPaymentMethodGroupId(long paymentMethodId, Dictionary<long, string> groupIdMap)
    {
        return groupIdMap.TryGetValue(paymentMethodId, out var groupId) && !string.IsNullOrEmpty(groupId)
            ? groupId
            : string.Empty; // Return empty string if not in config
    }

    private Dictionary<long, string> BuildPaymentMethodTypeMap(List<PaymentMethodGroupConfig> configs)
    {
        var map = new Dictionary<long, string>();
        foreach (var config in configs)
        {
            foreach (var keyStr in config.Key)
            {
                if (long.TryParse(keyStr, out var paymentMethodId))
                {
                    map[paymentMethodId] = config.type;
                }
            }
        }
        return map;
    }

    private string GetPaymentMethodType(long paymentMethodId, Dictionary<long, string> methodTypeMap)
    {
        return methodTypeMap.TryGetValue(paymentMethodId, out var methodType) && !string.IsNullOrEmpty(methodType)
            ? methodType
            : string.Empty; // Return empty string if not in config
    }

    private async Task<List<long>> GetPaymentMethodIdsByGroupIdAsync(int groupId)
    {
        var configs = await LoadPaymentMethodGroupsConfigAsync();
        var paymentMethodIds = new List<long>();
        
        foreach (var config in configs)
        {
            // Match by groupId
            if (config.Group.Id == groupId)
            {
                foreach (var keyStr in config.Key)
                {
                    if (long.TryParse(keyStr, out var paymentMethodId))
                    {
                        paymentMethodIds.Add(paymentMethodId);
                    }
                }
            }
        }
        
        return paymentMethodIds;
    }

    private async Task<long> ConvertToUsdAsync(long amountInCents, CurrencyTypes fromCurrency)
    {
        if (fromCurrency == CurrencyTypes.USD)
            return amountInCents;

        // Check if this is a USC conversion (use programmatic rate)
        var (isUscConversion, uscRate) = CurrencyConversionHelper.GetUscConversionRate(fromCurrency, CurrencyTypes.USD);
        
        if (isUscConversion)
        {
            if (uscRate == -1.0m)
            {
                // Multi-step USC conversion, shouldn't happen for USC->USD but handle it
                var finalRate = await CurrencyConversionHelper.CalculateUscConversionRateDoubleAsync(
                    fromCurrency, 
                    CurrencyTypes.USD,
                    async (from, to) => await GetMtExchangeRateForFinanceSummary(from, to));
                return (long)Math.Round(amountInCents * finalRate, MidpointRounding.AwayFromZero);
            }
            
            // Direct USC conversion
            return (long)Math.Round(amountInCents * uscRate, MidpointRounding.AwayFromZero);
        }

        // Regular currency conversion using MT exchange rates
        var exchangeRate = await GetMtExchangeRateForFinanceSummary(fromCurrency, CurrencyTypes.USD);
        if (exchangeRate <= 0)
        {
            logger.LogWarning("Exchange rate not found for {FromCurrency} to USD, using rate 1.0", fromCurrency);
            return amountInCents; // Fallback to original amount
        }

        return (long)Math.Round(amountInCents * exchangeRate, MidpointRounding.AwayFromZero);
    }

    private async Task<double> GetMtExchangeRateForFinanceSummary(CurrencyTypes from, CurrencyTypes to)
    {
        if (from == to) return 1.0;

        // Use a default service ID for exchange rate lookup - you may want to make this configurable
        const int defaultServiceId = 1;

        var rate = await tenantCtx.ExchangeRates
            .Where(x => x.FromCurrencyId == (int)from && x.ToCurrencyId == (int)to)
            .Select(x => x.BuyingRate)
            .FirstOrDefaultAsync();

        if (rate > 0) return (double)rate;

        // Try reverse rate
        var reverseRate = await tenantCtx.ExchangeRates
            .Where(x => x.FromCurrencyId == (int)to && x.ToCurrencyId == (int)from)
            .Select(x => x.SellingRate)
            .FirstOrDefaultAsync();

        if (reverseRate > 0) return 1.0 / (double)reverseRate;

        logger.LogWarning("No exchange rate found for {From} to {To}", from, to);
        return -1.0; // Indicates no rate found
    }

    public async Task<Result<List<FinanceDetailRow>, FinanceDetailCriteria>> GetDepositWithdrawDetailsPaginatedAsync(
        FinanceDetailCriteria criteria)
    {
        var depositCompleted = (int)StateTypes.DepositCompleted;
        var withdrawalCompleted = (int)StateTypes.WithdrawalCompleted;
        
        // Load PaymentMethodGroups configuration to get group info
        var paymentMethodGroupsConfig = await LoadPaymentMethodGroupsConfigAsync();
        var groupConfig = paymentMethodGroupsConfig.FirstOrDefault(x => x.Group.Id == criteria.GroupId);
        
        if (groupConfig == null)
        {
            return Result<List<FinanceDetailRow>, FinanceDetailCriteria>.Of(new List<FinanceDetailRow>(), criteria);
        }
        
        var groupName = groupConfig.Group.Name;
        
        // Get payment method IDs for this group
        var paymentMethodIds = await GetPaymentMethodIdsByGroupIdAsync(criteria.GroupId);
        
        if (paymentMethodIds.Count == 0)
        {
            return Result<List<FinanceDetailRow>, FinanceDetailCriteria>.Of(new List<FinanceDetailRow>(), criteria);
        }

        // Get comprehensive party info with tags and comments
        var party = await tenantCtx.Parties
            .Include(x => x.PartyComments)
            .Include(x => x.Tags)
            .Where(x => x.Id == criteria.PartyId)
            .Select(x => new { 
                x.Id, 
                x.Uid,
                x.Email,
                x.Name,
                x.FirstName,
                x.LastName,
                x.NativeName,
                x.Avatar,
                x.Language,
                x.IdNumber,
                x.PhoneNumber,
                x.LastLoginIp,
                x.Status,
                PartyTags = x.Tags.Select(t => t.Name).ToList(),
                HasComment = x.PartyComments.Any()
            })
            .FirstOrDefaultAsync();

        if (party == null)
        {
            return Result<List<FinanceDetailRow>, FinanceDetailCriteria>.Of(new List<FinanceDetailRow>(), criteria);
        }

        // Get deposits with TARGET account information (where deposits are credited TO)
        var deposits = await tenantCtx.Deposits
            .Include(x => x.Payment)
                .ThenInclude(x => x.PaymentMethod)
            .Include(x => x.TargetAccount)
                .ThenInclude(x => x.TradeAccountStatus)
            .Include(x => x.TargetAccount)
                .ThenInclude(x => x.SalesAccount)
            .Include(x => x.TargetAccount)
                .ThenInclude(x => x.AgentAccount)
            .Include(x => x.TargetAccount)
                .ThenInclude(x => x.AccountComments)
            .Include(x => x.Party)
                .ThenInclude(x => x.Wallets)
            .Where(x => x.PartyId == criteria.PartyId)
            .Where(x => x.Payment != null && x.Payment.PaymentMethod != null && paymentMethodIds.Contains(x.Payment.PaymentMethod.Id))
            .Where(x => x.IdNavigation.StateId == depositCompleted)
            .Select(x => new FinancialTransactionInfo
            {
                Id = x.Id,
                Type = "Deposit",
                Amount = x.Amount,
                CurrencyId = x.CurrencyId,
                ReferenceNumber = x.ReferenceNumber ?? "",
                AccountId = x.TargetAccountId,
                AccountNumber = x.TargetAccount != null ? x.TargetAccount.AccountNumber : (long?)null,
                CreatedOn = x.IdNavigation.PostedOn,
                CompletedOn = x.IdNavigation.StatedOn,
                PaymentId = x.PaymentId,
                PaymentName = x.Payment != null && x.Payment.PaymentMethod != null ? x.Payment.PaymentMethod.Name : null,
                PaymentNumber = x.Payment != null ? x.Payment.Number : null,
                PaymentStatus = x.Payment != null ? x.Payment.Status : (int?)null,
                // For deposits: Target account is where funds are deposited TO
                TargetAccountId = x.TargetAccountId,
                TargetAccountNumber = x.TargetAccount != null ? x.TargetAccount.AccountNumber : (long?)null,
                TargetAccountType = x.TargetAccount != null ? TransactionAccountTypes.Account : TransactionAccountTypes.Wallet,
                TargetBalanceInCents = x.TargetAccount != null && x.TargetAccount.TradeAccountStatus != null 
                    ? x.TargetAccount.TradeAccountStatus.BalanceInCents 
                    : (x.Party.Wallets.Where(w => w.CurrencyId == x.CurrencyId).Select(w => w.BalanceInCents).FirstOrDefault()),
                TargetEquityInCents = x.TargetAccount != null && x.TargetAccount.TradeAccountStatus != null 
                    ? x.TargetAccount.TradeAccountStatus.EquityInCents 
                    : 0,
                TargetSalesGroupName = x.TargetAccount != null && x.TargetAccount.SalesAccount != null 
                    ? x.TargetAccount.SalesAccount.Code 
                    : string.Empty,
                TargetAgentGroupName = x.TargetAccount != null && x.TargetAccount.AgentAccount != null 
                    ? x.TargetAccount.AgentAccount.Group 
                    : string.Empty,
                TargetCurrencyId = x.TargetAccount != null ? (CurrencyTypes)x.TargetAccount.CurrencyId : (CurrencyTypes)x.CurrencyId,
                TargetHasComment = x.TargetAccount != null && x.TargetAccount.AccountComments.Any(),
                TargetDisplayNumber = x.TargetAccount != null 
                    ? x.TargetAccount.AccountNumber.ToString() 
                    : (x.Party.Wallets.Where(w => w.CurrencyId == x.CurrencyId).Select(w => w.Id.ToString()).FirstOrDefault() ?? ""),
                // Deposits don't have source accounts - they come from external sources
                HasSourceAccount = false
            })
            .ToListAsync();

        // Get withdrawals with SOURCE account information (where funds are withdrawn FROM)
        // Only get withdrawals if methodType is "withdrawal"
        var withdrawals = await tenantCtx.Withdrawals
            .Include(x => x.Payment)
                .ThenInclude(x => x.PaymentMethod)
            .Include(x => x.SourceAccount)
                .ThenInclude(x => x.TradeAccountStatus)
            .Include(x => x.SourceAccount)
                .ThenInclude(x => x.SalesAccount)
            .Include(x => x.SourceAccount)
                .ThenInclude(x => x.AgentAccount)
            .Include(x => x.SourceAccount)
                .ThenInclude(x => x.AccountComments)
            .Include(x => x.Party)
                .ThenInclude(x => x.Wallets)
            .Where(x => x.PartyId == criteria.PartyId && x.SourceAccountId > 0)
            .Where(x => x.Payment != null && x.Payment.PaymentMethod != null && paymentMethodIds.Contains(x.Payment.PaymentMethod.Id))
            .Where(x => x.IdNavigation.StateId == withdrawalCompleted)
            .Select(x => new FinancialTransactionInfo
            {
                Id = x.Id,
                Type = "Withdrawal",
                Amount = x.Amount,
                CurrencyId = x.CurrencyId,
                ReferenceNumber = x.ReferenceNumber ?? "",
                AccountId = x.SourceAccountId,
                AccountNumber = x.SourceAccount != null ? x.SourceAccount.AccountNumber : (long?)null,
                CreatedOn = x.IdNavigation.StatedOn,
                CompletedOn = x.IdNavigation.StatedOn,
                PaymentId = x.PaymentId,
                PaymentName = x.Payment != null && x.Payment.PaymentMethod != null ? x.Payment.PaymentMethod.Name : null,
                PaymentNumber = x.Payment != null ? x.Payment.Number : null,
                PaymentStatus = x.Payment != null ? x.Payment.Status : (int?)null,
                // For withdrawals: Source account is where funds are withdrawn FROM
                SourceAccountId = x.SourceAccountId,
                SourceAccountNumber = x.SourceAccount != null ? x.SourceAccount.AccountNumber : (long?)null,
                SourceAccountType = x.SourceAccount != null ? TransactionAccountTypes.Account : TransactionAccountTypes.Wallet,
                SourceBalanceInCents = x.SourceAccount != null && x.SourceAccount.TradeAccountStatus != null 
                    ? x.SourceAccount.TradeAccountStatus.BalanceInCents 
                    : (x.Party.Wallets.Where(w => w.CurrencyId == x.CurrencyId).Select(w => w.BalanceInCents).FirstOrDefault()),
                SourceEquityInCents = x.SourceAccount != null && x.SourceAccount.TradeAccountStatus != null 
                    ? x.SourceAccount.TradeAccountStatus.EquityInCents 
                    : 0,
                SourceSalesGroupName = x.SourceAccount != null && x.SourceAccount.SalesAccount != null 
                    ? x.SourceAccount.SalesAccount.Code 
                    : string.Empty,
                SourceAgentGroupName = x.SourceAccount != null && x.SourceAccount.AgentAccount != null 
                    ? x.SourceAccount.AgentAccount.Group 
                    : string.Empty,
                SourceCurrencyId = x.SourceAccount != null ? (CurrencyTypes)x.SourceAccount.CurrencyId : (CurrencyTypes)x.CurrencyId,
                SourceHasComment = x.SourceAccount != null && x.SourceAccount.AccountComments.Any(),
                SourceDisplayNumber = x.SourceAccount != null 
                    ? x.SourceAccount.AccountNumber.ToString() 
                    : (x.Party.Wallets.Where(w => w.CurrencyId == x.CurrencyId).Select(w => w.Id.ToString()).FirstOrDefault() ?? ""),
                // Withdrawals always have source accounts
                HasSourceAccount = true
            })
            .ToListAsync();

        // Combine both lists
        var combinedTransactions = deposits.Concat(withdrawals).ToList();

        // Apply sorting
        var sortedTransactions = criteria.SortField?.ToLower() switch
        {
            "createdon" => criteria.SortFlag 
                ? combinedTransactions.OrderByDescending(x => x.CreatedOn).ToList()
                : combinedTransactions.OrderBy(x => x.CreatedOn).ToList(),
            "completedon" => criteria.SortFlag
                ? combinedTransactions.OrderByDescending(x => x.CompletedOn).ToList()
                : combinedTransactions.OrderBy(x => x.CompletedOn).ToList(),
            "amount" => criteria.SortFlag
                ? combinedTransactions.OrderByDescending(x => x.Amount).ToList()
                : combinedTransactions.OrderBy(x => x.Amount).ToList(),
            "type" => criteria.SortFlag
                ? combinedTransactions.OrderByDescending(x => x.Type).ToList()
                : combinedTransactions.OrderBy(x => x.Type).ToList(),
            _ => criteria.SortFlag
                ? combinedTransactions.OrderByDescending(x => x.CreatedOn).ToList()
                : combinedTransactions.OrderBy(x => x.CreatedOn).ToList()
        };

        // Get total count before pagination
        criteria.Total = sortedTransactions.Count;
        criteria.PageCount = (int)Math.Ceiling(criteria.Total / (decimal)criteria.Size);
        criteria.HasMore = criteria.PageCount > criteria.Page;

        // Apply pagination
        var pagedData = sortedTransactions
            .Skip((criteria.Page - 1) * criteria.Size)
            .Take(criteria.Size)
            .ToList();

        // Create comprehensive user info with all required fields
        var userDisplayName = GetUserDisplayName(party.FirstName, party.LastName, party.Name, party.NativeName, party.Email);
        var userInfo = new DetailedUserInfo
        {
            Id = party.Id,
            Uid = party.Uid,
            PartyId = party.Id,
            Email = party.Email ?? "",
            Avatar = party.Avatar ?? "",
            Language = party.Language ?? "",
            IdNumber = party.IdNumber ?? "",
            Phone = party.PhoneNumber ?? "",
            LastLoginIp = party.LastLoginIp ?? "",
            Status = party.Status,
            HasComment = party.HasComment,
            IsInIpBlackList = false, // TODO: Implement IP blacklist check if needed
            IsInUserBlackList = false, // TODO: Implement user blacklist check if needed
            PartyTags = party.PartyTags,
            NativeName = party.NativeName ?? "",
            FirstName = party.FirstName ?? "",
            LastName = party.LastName ?? "",
            DisplayName = userDisplayName
        };

        // Convert to detail rows with USD conversion
        var detailRows = new List<FinanceDetailRow>();
        foreach (var item in pagedData)
        {
            var amountUsd = await ConvertToUsdAsync(item.Amount, (CurrencyTypes)item.CurrencyId);
            
            // Create appropriate account info based on transaction type
            AccountInfo? accountInfo = null;
            if (item.Type == "Deposit" && item.TargetAccountId.HasValue)
            {
                // For deposits, show the target account (where money was deposited)
                accountInfo = new AccountInfo
                {
                    Id = item.TargetAccountId.Value,
                    AccountType = item.TargetAccountType,
                    DisplayNumber = item.TargetDisplayNumber,
                    BalanceInCents = item.TargetBalanceInCents,
                    EquityInCents = item.TargetEquityInCents,
                    SalesGroupName = item.TargetSalesGroupName,
                    AgentGroupName = item.TargetAgentGroupName,
                    CurrencyId = item.TargetCurrencyId,
                    HasComment = item.TargetHasComment,
                    AccountRole = "Target" // Money deposited TO this account
                };
            }
            else if (item.Type == "Withdrawal" && item.HasSourceAccount && item.SourceAccountId.HasValue)
            {
                // For withdrawals, show the source account (where money was withdrawn from)
                accountInfo = new AccountInfo
                {
                    Id = item.SourceAccountId.Value,
                    AccountType = item.SourceAccountType,
                    DisplayNumber = item.SourceDisplayNumber,
                    BalanceInCents = item.SourceBalanceInCents,
                    EquityInCents = item.SourceEquityInCents,
                    SalesGroupName = item.SourceSalesGroupName,
                    AgentGroupName = item.SourceAgentGroupName,
                    CurrencyId = item.SourceCurrencyId,
                    HasComment = item.SourceHasComment,
                    AccountRole = "Source" // Money withdrawn FROM this account
                };
            }

            detailRows.Add(new FinanceDetailRow
            {
                PartyId = criteria.PartyId,
                Email = party.Email ?? "",
                UserName = userDisplayName,
                PartyTags = party.PartyTags,
                HasComment = party.HasComment,
                FundType = 0, // Not used when filtering by groupId
                FundTypeName = groupName, // Use group name instead of fund type name
                Id = item.Id,
                Type = item.Type,
                Amount = item.Amount,
                AmountUsd = amountUsd,
                CurrencyId = item.CurrencyId,
                CurrencyName = ((CurrencyTypes)item.CurrencyId).ToString(),
                ReferenceNumber = item.ReferenceNumber,
                AccountId = item.AccountId,
                AccountNumber = item.AccountNumber,
                CreatedOn = item.CreatedOn,
                CompletedOn = item.CompletedOn,
                // Payment information
                PaymentId = item.PaymentId,
                PaymentName = item.PaymentName ?? "",
                PaymentNumber = item.PaymentNumber ?? "",
                PaymentStatus = item.PaymentStatus,
                // New fields to match withdrawal API structure
                UpdatedOn = item.CompletedOn,
                StateId = 400, // Assuming completed state
                ExchangeRate = item.Type == "Withdrawal" ? 1.0m : null, // TODO: Get actual exchange rate if needed
                OperatorName = userDisplayName,
                Source = item.Type == "Withdrawal" ? accountInfo : null, // Source only for withdrawals
                User = userInfo, // Complete user information
                // Account information (source for withdrawals, target for deposits)
                //AccountInfo = accountInfo
            });
        }

        return Result<List<FinanceDetailRow>, FinanceDetailCriteria>.Of(detailRows, criteria);
    }

    private static string GetFundTypeName(int fundType) => fundType switch
    {
        (int)FundTypes.Ips => "IPS",
        (int)FundTypes.Wire => "Wire",
        _ => fundType.ToString()
    };

    private static string GetUserDisplayName(string? firstName, string? lastName, string? name, string? nativeName, string? email)
    {
        // Try FirstName + LastName first
        var fullName = $"{firstName ?? ""} {lastName ?? ""}".Trim();
        if (!string.IsNullOrEmpty(fullName) && fullName != email && !fullName.Contains("@"))
        {
            return fullName;
        }

        // Try NativeName if available and different from email
        if (!string.IsNullOrEmpty(nativeName) && nativeName != email && !nativeName.Contains("@"))
        {
            return nativeName;
        }

        // Try Name field if available and different from email
        if (!string.IsNullOrEmpty(name) && name != email && !name.Contains("@"))
        {
            return name;
        }

        // Fallback to email if nothing else is available
        return email ?? "Unknown User";
    }

    // Helper class for combined transaction information
    public class FinancialTransactionInfo
    {
        public long Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public long Amount { get; set; }
        public int CurrencyId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public long? AccountId { get; set; }
        public long? AccountNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime CompletedOn { get; set; }
        public long? PaymentId { get; set; }
        public string? PaymentName { get; set; }
        public string? PaymentNumber { get; set; }
        public int? PaymentStatus { get; set; }
        
        // Source information (for withdrawals)
        public long? SourceAccountId { get; set; }
        public long? SourceAccountNumber { get; set; }
        public TransactionAccountTypes SourceAccountType { get; set; }
        public double SourceBalanceInCents { get; set; }
        public double SourceEquityInCents { get; set; }
        public string SourceSalesGroupName { get; set; } = string.Empty;
        public string SourceAgentGroupName { get; set; } = string.Empty;
        public CurrencyTypes SourceCurrencyId { get; set; }
        public bool SourceHasComment { get; set; }
        public string SourceDisplayNumber { get; set; } = string.Empty;
        
        // Target information (for deposits)
        public long? TargetAccountId { get; set; }
        public long? TargetAccountNumber { get; set; }
        public TransactionAccountTypes TargetAccountType { get; set; }
        public double TargetBalanceInCents { get; set; }
        public double TargetEquityInCents { get; set; }
        public string TargetSalesGroupName { get; set; } = string.Empty;
        public string TargetAgentGroupName { get; set; } = string.Empty;
        public CurrencyTypes TargetCurrencyId { get; set; }
        public bool TargetHasComment { get; set; }
        public string TargetDisplayNumber { get; set; } = string.Empty;
        
        // Indicates whether this transaction has a source account (withdrawals do, deposits don't)
        public bool HasSourceAccount { get; set; }
    }

    public class FinanceSummaryRow
    {
        public long PartyId { get; set; }
        public string Email { get; set; } = string.Empty;
        public long PaymentMethodId { get; set; }
        public string PaymentGroupName { get; set; } = string.Empty; // UnionPay / CryptoId / etc.
        public string PaymentGroupId { get; set; } = string.Empty; // Defined in configuration 
        public string Type { get; set; } = string.Empty; // "Deposit" or "Withdrawal"
        public long DepositSumUsd { get; set; } // In USD cents
        public long WithdrawSumUsd { get; set; } // In USD cents
        public long NeedWithdrawAmountUsd { get; set; } // In USD cents
    }

    public class FinanceDetailResult
    {
        public long PartyId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int FundType { get; set; }
        public string FundTypeName { get; set; } = string.Empty;
        public List<FinanceDetailRow> Deposits { get; set; } = new();
        public List<FinanceDetailRow> Withdrawals { get; set; } = new();
    }

    public class AccountInfo
    {
        public long Id { get; set; }
        public TransactionAccountTypes AccountType { get; set; }
        public string DisplayNumber { get; set; } = string.Empty;
        public double BalanceInCents { get; set; }
        public double Balance => BalanceInCents / 100d;
        public double EquityInCents { get; set; }
        public double Equity => EquityInCents / 100d;
        public string SalesGroupName { get; set; } = string.Empty;
        public string AgentGroupName { get; set; } = string.Empty;
        public CurrencyTypes CurrencyId { get; set; }
        public bool HasComment { get; set; }
        public string AccountRole { get; set; } = string.Empty; // "Source" for withdrawals, "Target" for deposits
    }

    public class FinanceDetailRow
    {
        public long PartyId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<string> PartyTags { get; set; } = new();
        public bool HasComment { get; set; }
        public int FundType { get; set; }
        public string FundTypeName { get; set; } = string.Empty;
        public long Id { get; set; }
        public string Type { get; set; } = string.Empty; // "Deposit" or "Withdrawal"
        public long Amount { get; set; } // Original amount in original currency cents
        public long AmountUsd { get; set; } // Converted amount in USD cents
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public long? AccountId { get; set; }
        public long? AccountNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime CompletedOn { get; set; }
        
        // Payment information
        public long? PaymentId { get; set; }
        public string PaymentName { get; set; } = string.Empty; // PaymentService.Name
        public string PaymentNumber { get; set; } = string.Empty; // Payment.Number
        public int? PaymentStatus { get; set; } // Payment.Status

        // Account information: For deposits = target account, For withdrawals = source account
        //public AccountInfo? AccountInfo { get; set; }

        // New fields to match withdrawal API structure
        public DateTime UpdatedOn { get; set; }
        public int StateId { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public AccountInfo? Source { get; set; } // Source account (null for deposits)
        public DetailedUserInfo? User { get; set; } // Comprehensive user information
    }

    public class DetailedUserInfo
    {
        public long Id { get; set; }
        public long Uid { get; set; }
        public long PartyId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string LastLoginIp { get; set; } = string.Empty;
        public int Status { get; set; }
        public bool HasComment { get; set; }
        public bool IsInIpBlackList { get; set; }
        public bool IsInUserBlackList { get; set; }
        public List<string> PartyTags { get; set; } = new();
        public string NativeName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public class PaymentInfo
    {
        public long Id { get; set; }
        public string? Pid { get; set; }
        public long PartyId { get; set; }
        public short LedgerSide { get; set; }
        public int PaymentServiceId { get; set; }
        public string PaymentServiceName { get; set; } = string.Empty;
        public int PaymentServicePlatformId { get; set; }
        public string Number { get; set; } = string.Empty;
        public int CurrencyId { get; set; }
        public long Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int Status { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public decimal? ExchangeRate { get; set; }
    }

    public class FinanceDetailCriteria : Criteria
    {
        public FinanceDetailCriteria()
        {
            SortField = "CreatedOn";
            SortFlag = true; // DESC by default
            Size = 10;
        }

        public long PartyId { get; set; }
        public int GroupId { get; set; } // Customized groups defined in configuration
    }
}