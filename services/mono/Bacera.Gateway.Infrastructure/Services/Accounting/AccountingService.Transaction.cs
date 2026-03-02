using Bacera.Gateway.Agent;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway;

partial class AccountingService
{
    public Task<bool> TransactionPendingCheckAsync(long partyId)
        => _tenantDbContext.Transactions
            .Where(x => x.PartyId == partyId)
            .Where(x => x.IdNavigation.StateId == (int)StateTypes.TransferCreated ||
                        x.IdNavigation.StateId == (int)StateTypes.TransferAwaitingApproval)
            .AnyAsync();

    public async Task<bool> TransactionTryCancelAsync(long id, long operatorPartyId)
    {
        var item = await _tenantDbContext.Transactions
            .FirstOrDefaultAsync(x => x.Id == id);

        var result = item != null
                     && (await TransitAsync(item, ActionTypes.TransferCancel, operatorPartyId)).Item1;

        if (result) _logger.LogInformation("Transaction: {Id} cancelled by {PartyId}", id, operatorPartyId);
        else _logger.LogInformation("Transaction: {Id} failed to cancel by {PartyId}", id, operatorPartyId);

        return result;
    }

    public async Task<bool> TransactionRejectByTenantAsync(Transaction item, long operatorPartyId)
    {
        if (item.Id == 0) return false;

        var result = await TransitAsync(item, ActionTypes.TransferReject, operatorPartyId);
        if (!result.Item1)
        {
            _logger.LogInformation("Transaction: {Id} failed to reject by {PartyId}", item.Id, operatorPartyId);
            return false;
        }

        _logger.LogInformation("Transaction: {Id} rejected by {PartyId}", item.Id, operatorPartyId);
        return true;
    }

    /// <summary>
    /// -1: Wallet not found
    /// -2: Balance not enough
    /// -3: TradeAccount not found
    ///
    /// -4: Currency not match
    /// -5: FundType not match
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="walletId"></param>
    /// <param name="tradeAccountId"></param>
    /// <param name="amount"></param>
    /// <param name="operatorPartyId"></param>
    /// <returns></returns>
    public async Task<Tuple<TransactionCheckStatus, Transaction>> TransactionCreateFromWalletToTradeAccountAsync(
        long partyId,
        long walletId,
        long tradeAccountId,
        long amount, long operatorPartyId)
    {
        var (status, wallet, account) =
            await GetWalletAndTradeAccountForTransfer(partyId, walletId, tradeAccountId, amount);
        if (status != TransactionCheckStatus.Passed)
            return Tuple.Create(status, new Transaction());
        if (wallet.CurrencyId != account.CurrencyId)
            return Tuple.Create(TransactionCheckStatus.CurrencyNotMatch, new Transaction());
        if (wallet.FundType != account.IdNavigation.FundType)
            return Tuple.Create(TransactionCheckStatus.FundTypeNotMatch, new Transaction());

        var transaction = Transaction.Build(
            partyId,
            TransactionAccountTypes.Wallet,
            walletId,
            partyId,
            TransactionAccountTypes.Account,
            tradeAccountId,
            LedgerSideTypes.Debit, amount, (FundTypes)wallet.FundType,
            (CurrencyTypes)wallet.CurrencyId
        );
        transaction.IdNavigation.AddActivity(operatorPartyId, ActionTypes.TransferCreateWithApprove, 0,
            StateTypes.TransferAwaitingApproval);
        await _tenantDbContext.Transactions.AddAsync(transaction);
        await _tenantDbContext.SaveChangesAsync();

        _logger.LogInformation("Transaction: {Id} created by {PartyId}", transaction.Id, operatorPartyId);

        return Tuple.Create(TransactionCheckStatus.Passed, transaction);
    }

    /// <summary>
    /// -1: Wallet not found
    /// -2: Balance not enough
    /// -3: TradeAccount not found
    ///
    /// -4: Currency not match
    /// -5: FundType not match
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="tradeAccountId"></param>
    /// <param name="walletId"></param>
    /// <param name="amount"></param>
    /// <param name="operatorPartyId"></param>
    /// <returns></returns>
    public async Task<Tuple<TransactionCheckStatus, Transaction>> TransactionCreateFromTradeAccountToWalletAsync(
        long partyId,
        long tradeAccountId, long walletId,
        long amount, long operatorPartyId)
    {
        var (status, wallet, account) = await GetWalletAndTradeAccountForTransfer(partyId, walletId, tradeAccountId);
        if (status != TransactionCheckStatus.Passed) return Tuple.Create(status, new Transaction());
        if (wallet.CurrencyId != account.CurrencyId)
            return Tuple.Create(TransactionCheckStatus.CurrencyNotMatch, new Transaction());
        if (wallet.FundType != account.IdNavigation.FundType)
            return Tuple.Create(TransactionCheckStatus.FundTypeNotMatch, new Transaction());

        var transaction = Transaction.Build(
            partyId,
            TransactionAccountTypes.Account,
            tradeAccountId,
            partyId,
            TransactionAccountTypes.Wallet,
            walletId,
            LedgerSideTypes.Debit, amount, (FundTypes)wallet.FundType,
            (CurrencyTypes)wallet.CurrencyId
        );
        transaction.IdNavigation.AddActivity(operatorPartyId, ActionTypes.TransferCreateWithApprove, 0,
            StateTypes.TransferAwaitingApproval);
        await _tenantDbContext.Transactions.AddAsync(transaction);
        await _tenantDbContext.SaveChangesAsync();

        _logger.LogInformation("Transaction: {Id} created by {PartyId}", transaction.Id, operatorPartyId);

        return Tuple.Create(TransactionCheckStatus.Passed, transaction);
    }

    public async Task<Tuple<TransactionCheckStatus, Transaction>> TransactionCreateBetweenTradeAccountAsync(
        long partyId, long sourceTradeAccountId, long targetTradeAccountId,
        long amount, long operatorPartyId)
    {
        var (status, sourceTradeAccount, targetTradeAccount) =
            await GetTradeAccountsForTransferBetween(partyId, sourceTradeAccountId, targetTradeAccountId, amount);
        if (status != TransactionCheckStatus.Passed) return Tuple.Create(status, new Transaction());

        if (sourceTradeAccount.CurrencyId != targetTradeAccount.CurrencyId)
            //return Tuple.Create(TransactionCheckStatus.CurrencyNotMatch, new Transaction());

        if (sourceTradeAccount.IdNavigation.FundType != targetTradeAccount.IdNavigation.FundType)
            return Tuple.Create(TransactionCheckStatus.FundTypeNotMatch, new Transaction());

        var transaction = Transaction.Build(
            partyId,
            TransactionAccountTypes.Account,
            sourceTradeAccountId,
            partyId,
            TransactionAccountTypes.Account,
            targetTradeAccountId,
            LedgerSideTypes.Debit, amount, (FundTypes)sourceTradeAccount.IdNavigation.FundType,
            (CurrencyTypes)sourceTradeAccount.CurrencyId
        );

        transaction.IdNavigation.AddActivity(operatorPartyId, ActionTypes.TransferCreateWithApprove, 0,
            StateTypes.TransferAwaitingApproval);

        await _tenantDbContext.Transactions.AddAsync(transaction);
        await _tenantDbContext.SaveChangesAsync();

        _logger.LogInformation("Transaction: {Id} created by {PartyId}", transaction.Id, operatorPartyId);
        return Tuple.Create(TransactionCheckStatus.Passed, transaction);
    }

    /// <summary>
    /// -1: Wallet not found
    /// -2: Balance not enough
    /// -3: TradeAccount not found
    ///
    /// -4: Currency not match
    /// -5: FundType not match
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="walletId"></param>
    /// <param name="tradeAccountId"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public async Task<TransactionCheckStatus> CanTransactionFromWalletToTradeAccountAsync(long partyId, long walletId,
        long tradeAccountId,
        long amount)
    {
        var (status, wallet, account) =
            await GetWalletAndTradeAccountForTransfer(partyId, walletId, tradeAccountId, amount);
        if (status != TransactionCheckStatus.Passed) return status;
        //if (wallet.CurrencyId != account.CurrencyId) return TransactionCheckStatus.CurrencyNotMatch;
        if (wallet.FundType != account.IdNavigation.FundType) return TransactionCheckStatus.FundTypeNotMatch;
        return TransactionCheckStatus.Passed;
    }


    /// <summary>
    /// -1: Wallet not found
    /// -2: Balance not enough
    /// -3: TradeAccount not found
    ///
    /// -4: Currency not match
    /// -5: FundType not match
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="tradeAccountId"></param>
    /// <param name="walletId"></param>
    /// <returns></returns>
    public async Task<TransactionCheckStatus> CanTransactionFromTradeAccountToWalletAsync(long partyId,
        long tradeAccountId, long walletId)
    {
        var (status, wallet, account) = await GetWalletAndTradeAccountForTransfer(partyId, walletId, tradeAccountId);
        if (status != TransactionCheckStatus.Passed) return status;

        if (wallet.CurrencyId != account.CurrencyId) return TransactionCheckStatus.CurrencyNotMatch;
        if (wallet.FundType != account.IdNavigation.FundType) return TransactionCheckStatus.FundTypeNotMatch;
        return TransactionCheckStatus.Passed;
    }

    /// <summary>
    /// -2: Balance not enough
    /// -3: TradeAccount not found
    ///
    /// -4: Currency not match
    /// -5: FundType not match
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="sourceTradeAccountId"></param>
    /// <param name="targetTradeAccountId"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public async Task<TransactionCheckStatus> CanTransactionBetweenTradeAccountAsync(long partyId,
        long sourceTradeAccountId,
        long targetTradeAccountId,
        long amount)
    {
        var (status, sourceTradeAccount, targetTradeAccount) =
            await GetTradeAccountsForTransferBetween(partyId, sourceTradeAccountId, targetTradeAccountId);

        if (status != TransactionCheckStatus.Passed) return status;

        if (sourceTradeAccount.CurrencyId != targetTradeAccount.CurrencyId)
            return TransactionCheckStatus.CurrencyNotMatch;
        if (sourceTradeAccount.IdNavigation.FundType != targetTradeAccount.IdNavigation.FundType)
            return TransactionCheckStatus.FundTypeNotMatch;
        return TransactionCheckStatus.Passed;
    }


    /// <summary>
    /// -1: Wallet not found
    /// -2: Balance not enough
    /// -3: TradeAccount not found
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="walletId"></param>
    /// <param name="tradeAccountId"></param>
    /// <param name="balance"></param>
    /// <returns></returns>
    private async Task<Tuple<TransactionCheckStatus, Wallet, TradeAccount>> GetWalletAndTradeAccountForTransfer(
        long partyId,
        long walletId,
        long tradeAccountId, long balance = 0)
    {
        var wallet = await _tenantDbContext.Wallets
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Id == walletId)
            .Select(x => new Wallet
                { Id = x.Id, CurrencyId = x.CurrencyId, PartyId = partyId, Balance = x.Balance, FundType = x.FundType })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (wallet == null)
            return Tuple.Create(TransactionCheckStatus.WalletNotFound, new Wallet(), new TradeAccount());

        var account = await _tenantDbContext.TradeAccounts
            .Where(x => x.IdNavigation.PartyId == partyId)
            .Where(x => x.Id == tradeAccountId)
            .Include(x => x.IdNavigation)
            .Select(x => new TradeAccount { Id = x.Id, CurrencyId = x.CurrencyId, IdNavigation = x.IdNavigation })
            .AsNoTracking()
            .FirstOrDefaultAsync();
        return account == null
            ? Tuple.Create(TransactionCheckStatus.TradeAccountNotFound, new Wallet(), new TradeAccount())
            : Tuple.Create(TransactionCheckStatus.Passed, wallet, account);
    }

    private async Task<Tuple<TransactionCheckStatus, TradeAccount, TradeAccount>> GetTradeAccountsForTransferBetween(
        long partyId,
        long sourceTradeAccountId,
        long targetTradeAccountId, long balance = 0)
    {
        var accounts = await _tenantDbContext.TradeAccounts
            .Where(x => x.IdNavigation.PartyId == partyId)
            .Where(x => x.Id == targetTradeAccountId || x.Id == sourceTradeAccountId)
            .Include(x => x.IdNavigation)
            .Select(x => new TradeAccount { Id = x.Id, CurrencyId = x.CurrencyId, IdNavigation = x.IdNavigation })
            .AsNoTracking()
            .ToListAsync();

        var sourceTradeAccount = accounts.SingleOrDefault(x => x.Id == sourceTradeAccountId);
        var targetTradeAccount = accounts.SingleOrDefault(x => x.Id == targetTradeAccountId);

        if (targetTradeAccount == null || sourceTradeAccount == null)
            return Tuple.Create(TransactionCheckStatus.TradeAccountNotFound, new TradeAccount(), new TradeAccount());

        return Tuple.Create(TransactionCheckStatus.Passed, sourceTradeAccount, targetTradeAccount);
    }

    private async Task<Transaction> TransactionCreateAsync(long senderPartyId,
        TransactionAccountTypes senderAccountType,
        long senderAccountId,
        long receiverPartyId,
        TransactionAccountTypes receiverAccountType,
        long receiverAccountId,
        LedgerSideTypes ledgerSide, FundTypes fundType, long amount,
        CurrencyTypes currency,
        long operatorPartyId, bool approvalNeeded = true, long? pid = null)
    {
        var transaction = Transaction.Build(
            senderPartyId,
            senderAccountType,
            senderAccountId,
            receiverPartyId,
            receiverAccountType,
            receiverAccountId,
            ledgerSide, amount, fundType,
            currency
        );

        transaction.IdNavigation.AddActivity(operatorPartyId, ActionTypes.TransferCreate);

        transaction.IdNavigation.Pid = pid;
        transaction.IdNavigation.StateId = approvalNeeded
                ? (int)StateTypes.TransferAwaitingApproval
                : (int)StateTypes.TransferCreated
            ;

        await _tenantDbContext.Transactions.AddAsync(transaction);
        await _tenantDbContext.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction> TransactionGetAsync(long id)
    {
        var model = await _tenantDbContext.Transactions
            .Include(x => x.IdNavigation)
            .FirstOrDefaultAsync(x => x.Id.Equals(id));
        return model ?? new Transaction();
    }

    public async Task<Transaction> TransactionGetForPartyAsync(long id, long partyId)
    {
        var model = await _tenantDbContext.Transactions
            .Where(x => x.PartyId.Equals(partyId))
            .FirstOrDefaultAsync(x => x.Id.Equals(id));
        return model ?? new Transaction();
    }

    public async Task<Result<List<Transaction>, Transaction.Criteria>> TransactionQueryAsync(
        Transaction.Criteria criteria)
    {
        var items = await _tenantDbContext.Transactions
            .Include(x => x.IdNavigation)
            .PagedFilterBy(criteria)
            .ToListAsync();
        return Result<List<Transaction>, Transaction.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<TransactionViewModel>, Transaction.Criteria>> TransactionQueryForTenantAsync(
        Transaction.Criteria criteria, bool hideEmail = false)
    {
        var items = await _tenantDbContext.Transactions
            .PagedFilterBy(criteria)
            .ToTenantViewModel(hideEmail)
            .ToListAsync();
        // await FulfillUserForViewModel(items);
        await FulfillBalance(items);
        await FulfillHasComment(items);
        return Result<List<TransactionViewModel>, Transaction.Criteria>.Of(items, criteria);
    }

    //TODO: use Account Group instead of Account Agent Id
    public async Task<Result<List<TransactionForAgentViewModel>, Transaction.Criteria>>
        TransactionQueryForParentAsync(Transaction.Criteria criteria)
    {
        if (criteria.ParentAccountUid == null)
            return Result<List<TransactionForAgentViewModel>, Transaction.Criteria>.Of([], criteria);

        if (criteria.Target != null)
        {
            if (long.TryParse(criteria.Target, out var accountNumber))
            {
                criteria.AccountNumber = accountNumber;
                // criteria.ParentAccountUid = null;
            }
            else
            {
                var uid = await _tenantDbContext.Accounts
                    .Where(x => x.Group == criteria.Target && x.Role != (int)AccountRoleTypes.Client)
                    .Where(x => x.ReferPath.Contains(criteria.ParentAccountUid!.Value.ToString()))
                    .Select(x => x.Uid)
                    .FirstOrDefaultAsync();
                if (uid == 0)
                {
                    uid = await _tenantDbContext.AccountExtraRelations
                        .Where(x => x.ParentAccount.Uid == criteria.ParentAccountUid)
                        .Where(x => x.ChildAccount.Group == criteria.Target)
                        .Select(x => x.ChildAccount.Uid)
                        .FirstOrDefaultAsync();
                }

                if (uid != 0) criteria.ParentAccountUid = uid;
                else criteria.ParentAccountUid = -1;
            }
        }
        var query = from t in _tenantDbContext.Transactions.FilterBy(criteria)
            // join party in _tenantDbContext.Parties on t.PartyId equals party.Id
            join m in _tenantDbContext.Matters on t.Id equals m.Id
            join receiverAccount in _tenantDbContext.Accounts on new
                    { AccountType = t.TargetAccountType, Id = t.TargetAccountId } equals new
                    { AccountType = (int)TransactionAccountTypes.Account, receiverAccount.Id }
                into receiverTradeAccountGroup
            from receiverTradeAccount in receiverTradeAccountGroup.DefaultIfEmpty()
            join senderAccount in _tenantDbContext.Accounts on new
                    { AccountType = t.SourceAccountType, Id = t.SourceAccountId } equals new
                    { AccountType = (int)TransactionAccountTypes.Account, senderAccount.Id }
                into senderTradeAccountGroup
            from senderTradeAccount in senderTradeAccountGroup.DefaultIfEmpty()
            where criteria.ParentAccountUid == null ||
                  receiverTradeAccount.ReferPath.Contains(criteria.ParentAccountUid.ToString() ?? "") ||
                  senderTradeAccount.ReferPath.Contains(criteria.ParentAccountUid.ToString() ?? "")
            where criteria.AccountNumber == null ||
                  receiverTradeAccount.AccountNumber == criteria.AccountNumber ||
                  senderTradeAccount.AccountNumber == criteria.AccountNumber
            select new TransactionForAgentViewModel
            {
                Id = t.Id,
                Amount = t.Amount,
                PartyId = t.PartyId,
                CurrencyId = t.CurrencyId,
                CreatedOn = t.CreatedOn,
                StateId = (StateTypes)m.StateId,
                SourceAccount = receiverTradeAccount == null
                    ? new AccountBasicViewModel()
                    : new AccountBasicViewModel
                    {
                        Uid = receiverTradeAccount.Uid,
                        CurrencyId = (CurrencyTypes)receiverTradeAccount.CurrencyId,
                        AccountNumber = receiverTradeAccount.AccountNumber,
                        Group = receiverTradeAccount.AgentAccount != null
                            ? receiverTradeAccount.AgentAccount!.Group
                            : string.Empty,
                    },
                TargetAccount = senderTradeAccount == null
                    ? new AccountBasicViewModel()
                    : new AccountBasicViewModel
                    {
                        Uid = senderTradeAccount.Uid,
                        CurrencyId = (CurrencyTypes)senderTradeAccount.CurrencyId,
                        AccountNumber = senderTradeAccount.AccountNumber,
                        Group = senderTradeAccount.AgentAccount != null
                            ? senderTradeAccount.AgentAccount!.Group
                            : string.Empty,
                    },
                User = t.Party.ToParentBasicViewModel()
            };
        criteria.Total = await query.CountAsync();
        
        // Calculate total amount in a common currency (USD)
        var transactionsForSum = await query
            .Select(x => new { x.Amount, x.CurrencyId })
            .ToListAsync();

        decimal totalAmountInUSD = 0;
        foreach (var tx in transactionsForSum)
        {
            var currency = (CurrencyTypes)tx.CurrencyId;
            decimal amountInUSD;

            if (currency == CurrencyTypes.USD)
            {
                // USD: Amount is already scaled, convert to USD units
                amountInUSD = tx.Amount / 1_000_000m;
            }
            else if (currency == CurrencyTypes.USC)
            {
                // USC: scaled amount -> USC units -> USD
                var uscUnits = tx.Amount.ToCentsFromScaled();
                amountInUSD = uscUnits / 100m; // 1 USC = 0.01 USD
            }
            else
            {
                // Other currencies: convert via exchange rate to USD
                var exchangeRate = await GetExchangeRateAsync(currency, CurrencyTypes.USD);
                if (exchangeRate?.SellingRate > 0)
                {
                    var amountInCurrencyUnits = tx.Amount / 1_000_000m; // scaled to units
                    amountInUSD = amountInCurrencyUnits * exchangeRate.SellingRate;
                }
                else
                {
                    // Skip if no exchange rate available
                    continue;
                }
            }

            totalAmountInUSD += amountInUSD;
        }

        // Convert back to scaled format for consistency
        criteria.TotalAmount = (long)(totalAmountInUSD * 1_000_000m);

        var items = await query.PageBy(criteria).ToListAsync();
        // await FulfillUserForTradeAccountTransactionResponseModel(items);
        return Result<List<TransactionForAgentViewModel>, Transaction.Criteria>.Of(items, criteria);
    }

    // private async Task FulfillUserForTradeAccountTransactionResponseModel(
    //     List<TransactionForAgentViewModel> items)
    // {
    //     var partyIds = items.Select(x => x.PartyId).Distinct().ToList();
    //
    //     var users = await _authDbContext.Users
    //         .Where(x => partyIds.Contains(x.PartyId) && x.TenantId == _tenantId)
    //         .ToSalesBasicViewModel()
    //         .ToListAsync();
    //
    //     foreach (var item in items)
    //     {
    //         var user = users.FirstOrDefault(x => x.PartyId == item.PartyId);
    //         if (user != null) item.User = user;
    //     }
    // }

    public async Task<Result<List<Transaction.ClientResponseModel>, Transaction.Criteria>>
        TransactionQueryForClientAsync(long currentPartyId, Transaction.Criteria criteria)
    {
        var query = from transaction in _tenantDbContext.Transactions.PagedFilterBy(criteria)
            join st in _tenantDbContext.TradeAccounts on transaction.SourceAccountId equals st.Id into sourceGroup
            from sourceTradeAccount in sourceGroup.DefaultIfEmpty()
            join tt in _tenantDbContext.TradeAccounts on transaction.TargetAccountId equals tt.Id into targetGroup
            from targetTradeAccount in targetGroup.DefaultIfEmpty()
            select new Transaction.ClientResponseModel
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                CurrencyId = transaction.CurrencyId,
                FundType = (FundTypes)transaction.FundType,
                StateId = transaction.IdNavigation.StateId,
                StatedOn = transaction.IdNavigation.StatedOn,
                SourceAccountType = transaction.SourceAccountType,
                TargetAccountType = transaction.TargetAccountType,
                SourceAccountNumber = sourceTradeAccount != null ? sourceTradeAccount.AccountNumber : 0,
                TargetAccountNumber = targetTradeAccount != null ? targetTradeAccount.AccountNumber : 0,
            };
        var items = await query.ToListAsync();
        return Result<List<Transaction.ClientResponseModel>, Transaction.Criteria>.Of(items, criteria);
    }

    public async Task<bool> TransactionExistsForReceiverAccountAsync(long partyId, int receiverAccountType,
        long receiverAccountId) =>
        await _tenantDbContext.Transactions
            .Where(x => x.TargetAccountType == receiverAccountType)
            .Where(x => x.TargetAccountId == receiverAccountId)
            .Where(x => x.PartyId == partyId)
            .AnyAsync();

    private async Task FulfillBalance(List<TransactionViewModel> trans)
    {
        var walletIds = trans
            .Where(x => x.TargetAccountType == TransactionAccountTypes.Wallet)
            .Select(x => x.TargetAccountId).ToList();
        walletIds.AddRange(trans
            .Where(x => x.SourceAccountType == TransactionAccountTypes.Wallet)
            .Select(x => x.SourceAccountId).ToList());

        var accountIds = trans
            .Where(x => x.TargetAccountType == TransactionAccountTypes.Account)
            .Select(x => x.TargetAccountId).ToList();
        accountIds.AddRange(trans
            .Where(x => x.SourceAccountType == TransactionAccountTypes.Account)
            .Select(x => x.SourceAccountId).ToList());

        var walletBalances = await _tenantDbContext.Wallets.Where(x => walletIds.Contains(x.Id))
            .Select(x => Tuple.Create(x.Id, x.Balance, 0L)).ToListAsync();
        var accountBalances = await _tenantDbContext.TradeAccountStatuses.Where(x => accountIds.Contains(x.Id))
            .Select(x => Tuple.Create(x.Id, ((long)Math.Round(x.Balance * 100, 0)).ToScaledFromCents(), x.IdNavigation.AccountNumber))
            .ToListAsync();

        foreach (var item in trans)
        {
            if (item.SourceAccountType == TransactionAccountTypes.Account)
            {
                var balance = accountBalances
                    .FirstOrDefault(x => x.Item1 == item.SourceAccountId);
                item.SourceAccountNumber = balance?.Item3 ?? 0;
                item.SourceAccountBalanceInCents = balance?.Item2 ?? 0;
            }
            else if (item.SourceAccountType == TransactionAccountTypes.Wallet)
            {
                item.SourceAccountBalanceInCents = walletBalances
                    .FirstOrDefault(x => x.Item1 == item.SourceAccountId)?.Item2 ?? 0;
            }

            if (item.TargetAccountType == TransactionAccountTypes.Account)
            {
                var balance = accountBalances
                    .FirstOrDefault(x => x.Item1 == item.TargetAccountId);
                item.TargetAccountNumber = balance?.Item3 ?? 0;
                item.TargetAccountBalanceInCents = balance?.Item2 ?? 0;
            }
            else if (item.TargetAccountType == TransactionAccountTypes.Wallet)
            {
                item.TargetAccountBalanceInCents = walletBalances
                    .FirstOrDefault(x => x.Item1 == item.TargetAccountId)?.Item2 ?? 0;
            }
        }
    }

    private async Task FulfillHasComment(List<TransactionViewModel> items)
    {
        var sourceAccountId = items
            .Where(x => x.SourceAccountType == TransactionAccountTypes.Account)
            .Select(x => x.SourceAccountId)
            .Distinct()
            .ToList();

        var targetAccountId = items
            .Where(x => x.TargetAccountType == TransactionAccountTypes.Account)
            .Select(x => x.TargetAccountId)
            .Distinct()
            .ToList();

        var accountIds = sourceAccountId.Union(targetAccountId).Distinct().ToList();
        var accountComments = await _tenantDbContext.AccountComments
            .Where(x => accountIds.Contains(x.AccountId))
            .GroupBy(x => x.AccountId)
            .Select(x => x.Key)
            .ToListAsync();

        foreach (var item in items)
        {
            if (item.SourceAccountType == TransactionAccountTypes.Account)
                item.SourceAccountHasComment = accountComments.Contains(item.SourceAccountId);

            if (item.TargetAccountType == TransactionAccountTypes.Account)
                item.TargetAccountHasComment = accountComments.Contains(item.TargetAccountId);
        }
    }
}

public enum TransactionCheckStatus
{
    WalletNotFound = -1,
    BalanceNotEnough = -2,
    TradeAccountNotFound = -3,
    CurrencyNotMatch = -4,
    FundTypeNotMatch = -5,
    Passed = 1,
}