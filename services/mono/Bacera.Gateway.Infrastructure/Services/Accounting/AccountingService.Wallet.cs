using Bacera.Gateway.ViewModels.Tenant;
using CsvHelper.TypeConversion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Linq;

namespace Bacera.Gateway;

public sealed partial class AccountingService
{
    /// <summary>
    /// Returns true if the wallet exists and is marked as primary; otherwise false.
    /// Returns false if the wallet does not exist.
    /// </summary>
    public Task<bool> IsPrimaryWallet(long walletId)
        => IsPrimaryWallet(walletId, System.Threading.CancellationToken.None);

    /// <summary>
    /// Returns true if the wallet exists and is marked as primary; otherwise false.
    /// Returns false if the wallet does not exist.
    /// </summary>
    public async Task<bool> IsPrimaryWallet(long walletId, System.Threading.CancellationToken cancellationToken)
    {
        if (walletId <= 0) return false;

        // Project directly to a boolean and avoid tracking
        return await _tenantDbContext.Wallets
            .AsNoTracking()
            .Where(w => w.Id == walletId)
            .Select(w => w.IsPrimary == 1)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<long> EnsureWalletExistsAsync(long partyId,
        CurrencyTypes currency = CurrencyTypes.USD,
        FundTypes type = FundTypes.Wire)
    {
        var walletId = await _tenantDbContext.Wallets
            .Where(x =>
                x.PartyId.Equals(partyId)
                && x.FundType.Equals((int)type)
                && x.CurrencyId.Equals((int)currency)
            )
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
        if (walletId != 0) return walletId;

        var isPrimary = await IsFirstWalletForPartyAsync(partyId);
        var wallet = Wallet.Build(partyId, currency, type, isPrimary);
        await _tenantDbContext.Wallets.AddAsync(wallet);
        try
        {
            await _tenantDbContext.SaveChangesAsync();
            return wallet.Id;
        }
        catch (InvalidOperationException)
        {
            _logger.LogError("Wallet of FundType {fundType} and Currency {currency} for Party {partyId} exists",
                type, currency, partyId);
            var existingWalletId = await _tenantDbContext.Wallets
                .Where(x =>
                    x.PartyId.Equals(partyId)
                    && x.FundType.Equals((int)type)
                    && x.CurrencyId.Equals((int)currency)
                )
                .Select(x => x.Id)
                .SingleOrDefaultAsync();
            return existingWalletId;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create wallet for party {partyId}", partyId);
            throw;
        }
    }

    public async Task<bool> WalletChangeBalanceAsync(long walletId, long matterId, long amount,
        long operatorPartyId = 1)
    {
        var transaction = await _tenantDbContext.Database.BeginTransactionAsync();
        long prevBalance;
        Wallet? wallet;
        try
        {
            wallet = await _tenantDbContext.Wallets
                .FromSqlRaw(
                    "SELECT * FROM acct.\"_Wallet\" WHERE \"Id\" = {0} FOR UPDATE"
                    , walletId
                )
                .SingleOrDefaultAsync();
            if (wallet == null) return false;
            prevBalance = wallet.Balance;
            wallet.Balance += amount;
            _tenantDbContext.Wallets.Update(wallet);
            await _tenantDbContext.SaveChangesWithAuditAsync(operatorPartyId);
            await transaction.CommitAsync();
        }
        // catch if not single error

        catch (Exception e)
        {
            // _logger.LogError("Wallet of Id {walletId} is not unique", walletId);
            _logger.LogError(e, "Wallet of Id {walletId} is not unique", walletId);
            await transaction.RollbackAsync();
            throw;
        }

        var trans = wallet.TransactionByMatter(matterId, prevBalance, amount);
        await _tenantDbContext.WalletTransactions.AddAsync(trans);
        await _tenantDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<Wallet> WalletChangeBalanceAsync(long partyId, FundTypes fundType,
        long matterId, long amount,
        CurrencyTypes currency, long operatorPartyId = 1)
    {
        // await using var ctx = _myDbContextPool.CreateTenantDbContextAsync(_tenantId);
        _tenantDbContext.ChangeTracker.Clear();
        var transaction = await _tenantDbContext.Database.BeginTransactionAsync();
        Wallet? wallet;
        long prevBalance;
        try
        {
            wallet = await _tenantDbContext.Wallets
                .FromSqlRaw(
                    "SELECT * FROM acct.\"_Wallet\" WHERE \"PartyId\" = {0} AND \"FundType\" = {1} AND \"CurrencyId\" = {2} FOR UPDATE"
                    , partyId
                    , (int)fundType
                    , (int)currency
                )
                .SingleOrDefaultAsync();
            if (wallet == null)
            {
                var isPrimary = await IsFirstWalletForPartyAsync(partyId);
                wallet = Wallet.Build(partyId, currency, fundType, isPrimary);
                wallet.Balance = amount;
                prevBalance = 0;
                _tenantDbContext.Wallets.Add(wallet);
                await _tenantDbContext.SaveChangesWithAuditAsync(operatorPartyId);
                await transaction.CommitAsync();
            }
            else
            {
                prevBalance = wallet.Balance;
                wallet.Balance += amount;
                _tenantDbContext.Wallets.Update(wallet);
                await _tenantDbContext.SaveChangesWithAuditAsync(operatorPartyId);
                await transaction.CommitAsync();
            }
        }
        // catch if not single error
        catch (InvalidOperationException)
        {
            _logger.LogError("Wallet of FundType {fundType} and Currency {currency} for Party {partyId} is not unique",
                fundType, currency, partyId);
            await transaction.RollbackAsync();
            throw;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        var trans = wallet.TransactionByMatter(matterId, prevBalance, amount);
        await _tenantDbContext.WalletTransactions.AddAsync(trans);
        await _tenantDbContext.SaveChangesAsync();
        _tenantDbContext.ChangeTracker.Clear();
        return wallet;
    }

    public async Task<Wallet> WalletGetOrCreateAsync(long partyId,
        CurrencyTypes currency = CurrencyTypes.USD,
        FundTypes type = FundTypes.Wire)
    {
        var wallet = await _tenantDbContext.Wallets
            .Where(x =>
                x.PartyId.Equals(partyId)
                && x.FundType.Equals((int)type)
                && x.CurrencyId.Equals((int)currency)
            )
            .FirstOrDefaultAsync();

        if (wallet != null)
        {
            // _logger.LogWarning("Wallet {id} already exists", wallet.Id);
            return wallet;
        }

        var isPrimary = await IsFirstWalletForPartyAsync(partyId);
        wallet = Wallet.Build(partyId, currency, type, isPrimary);
        await _tenantDbContext.Wallets.AddAsync(wallet);
        await _tenantDbContext.SaveChangesAsync();
        return wallet;
    }

    public async Task<Wallet> WalletGetOrCreateForClientAsync(long partyId,
        CurrencyTypes currency = CurrencyTypes.USD,
        FundTypes type = FundTypes.Wire)
    {
        // ALWAYS ensure USD primary wallet exists for clients
        var walletUSD = await _tenantDbContext.Wallets
            .Where(x =>
                x.PartyId.Equals(partyId)
                && x.FundType.Equals((int)type)
                && x.CurrencyId.Equals((int)CurrencyTypes.USD)
            )
            .FirstOrDefaultAsync();

        // Create USD primary wallet if it doesn't exist
        // -> Update trd."_Account".WalletId to USD primary wallet's ID
        if (walletUSD is null)
        {
            try
            {
                // Ensure only one primary wallet
                var existingPrimaryWallet = await _tenantDbContext.Wallets
                    .Where(x => x.PartyId.Equals(partyId) && x.IsPrimary == 1)
                    .FirstOrDefaultAsync();

                bool toCreatePrimaryWallet = existingPrimaryWallet is null;

                walletUSD = Wallet.Build(partyId, CurrencyTypes.USD, type, toCreatePrimaryWallet);
                await _tenantDbContext.Wallets.AddAsync(walletUSD);
                await _tenantDbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("acct_wallets_party_id_type_currency_id_unique") == true)
            {
                // Handle race condition: wallet was created by another request
                walletUSD = await _tenantDbContext.Wallets
                    .Where(x =>
                        x.PartyId.Equals(partyId)
                        && x.FundType.Equals((int)type)
                        && x.CurrencyId.Equals((int)CurrencyTypes.USD)
                    )
                    .FirstOrDefaultAsync();

                if (walletUSD is null)
                    throw; // Re-throw if wallet still doesn't exist
            }

            // Update client's WalletId to the just created USD Wallet's Id as the primary walletId
            var accountClient = await _tenantDbContext.Accounts
                .Where(x => 
                x.PartyId == partyId 
                && x.Role == (short)AccountRoleTypes.Client
                )
                .FirstOrDefaultAsync();

            if (accountClient != null)
            {
                accountClient.WalletId = walletUSD.Id;  // Assign the wallet ID, not the wallet entity
                _tenantDbContext.Accounts.Update(accountClient);
                await _tenantDbContext.SaveChangesAsync();
            }
        }

        if (currency == CurrencyTypes.USD) return walletUSD;

        // For non-USD currencies, get or create the specific wallet
        var walletNotUSD = await _tenantDbContext.Wallets
            .Where(x =>
                x.PartyId.Equals(partyId)
                && x.FundType.Equals((int)type)
                && x.CurrencyId.Equals((int)currency)
            )
            .FirstOrDefaultAsync();

        if (walletNotUSD is null)
        {
            try
            {
                walletNotUSD = Wallet.Build(partyId, currency, type, false);
                await _tenantDbContext.Wallets.AddAsync(walletNotUSD);
                await _tenantDbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("acct_wallets_party_id_type_currency_id_unique") == true)
            {
                // Handle race condition: wallet was created by another request
                walletNotUSD = await _tenantDbContext.Wallets
                    .Where(x =>
                        x.PartyId.Equals(partyId)
                        && x.FundType.Equals((int)type)
                        && x.CurrencyId.Equals((int)currency)
                    )
                    .FirstOrDefaultAsync();

                if (walletNotUSD is null)
                    throw; // Re-throw if wallet still doesn't exist
            }
        }

        return walletUSD;
    }

    public async Task<List<WalletStatisticViewModel>> WalletStatisticAsync(Wallet.Criteria criteria)
    {
        var items = await _tenantDbContext.Wallets
            .FilterBy(criteria)
            .GroupBy(x => new { x.CurrencyId, x.FundType })
            .Select(x => new WalletStatisticViewModel
            {
                CurrencyId = (CurrencyTypes)x.Key.CurrencyId,
                FundType = (FundTypes)x.Key.FundType,
                Balance = x.Sum(w => w.Balance)
            })
            .OrderByDescending(x => x.Balance)
            .ToListAsync();
        return items;
    }

    /// <summary>
    /// Retrieves all wallets with IsPrimary flag directly from the database column.
    /// No longer calculates primary wallets dynamically.
    /// </summary>
    /// <param name="criteria">Filter criteria for wallet query</param>
    /// <param name="hideEmail">Whether to hide email information in the response</param>
    /// <returns>Result containing list of wallets with IsPrimary flag from the database</returns>
    public async Task<Result<List<WalletViewModel>, Wallet.Criteria>> WalletQueryAsync(Wallet.Criteria criteria,
        bool hideEmail = false)
    {
        // Get all wallets that match the criteria with pagination
        var items = await _tenantDbContext.Wallets
            .PagedFilterBy(criteria)
            .ToTenantViewModel(hideEmail)
            .ToListAsync();

        // await FulfillUserWithHasComment(items);

        return Result<List<WalletViewModel>, Wallet.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<Wallet.ResponseModel>, Wallet.Criteria>> WalletQueryForClientAsync(long partyId,
        Wallet.Criteria criteria)
    {
        criteria.PartyId = partyId;
        var items = await _tenantDbContext.Wallets
            .PagedFilterBy(criteria)
            .ToResponseModels()
            .ToListAsync();
        return Result<List<Wallet.ResponseModel>, Wallet.Criteria>.Of(items, criteria);
    }

    public async Task<Wallet> WalletGetAsync(long id)
    {
        var item = await _tenantDbContext.Wallets
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
        return item ?? new Wallet();
    }

    public async Task<Wallet.ResponseModel> WalletGetForPartyAsync(long id, long partyId)
    {
        var item = await _tenantDbContext.Wallets
            .Where(x => x.Id == id)
            .Where(x => x.PartyId == partyId)
            .ToResponseModels()
            .FirstOrDefaultAsync();
        return item ?? new Wallet.ResponseModel();
    }

    public async Task<Result<List<WalletTransaction>, WalletTransaction.Criteria>> WalletTransactionQueryAsync(
        WalletTransaction.Criteria criteria)
    {
        var items = await _tenantDbContext.WalletTransactions
            .Include(x => x.Matter)
            .PagedFilterBy(criteria)
            .ToListAsync();
        return Result<List<WalletTransaction>, WalletTransaction.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<Wallet.WalletTransactionViewModel>, WalletTransaction.Criteria>>
        WalletTransactionQueryAsync(long partyId, WalletTransaction.Criteria criteria)
    {
        criteria.PartyId = partyId;
        var items = await _tenantDbContext.WalletTransactions
            .PagedFilterBy(criteria)
            .ToClientViewModel()
            .ToListAsync();

        var transactionIds = items.Where(x => x.MatterType == MatterTypes.InternalTransfer)
            .Select(x => x.MatterId)
            .ToList();

        var transactions = await _tenantDbContext.Transactions
            .Where(x => transactionIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.SourceAccountType,
                x.SourceAccountId,
                x.TargetAccountType,
                x.TargetAccountId
            })
            .ToListAsync();

        var accountIds = new HashSet<long>();
        foreach (var t in transactions)
        {
            if (t.SourceAccountType == (short)TransactionAccountTypes.Account)
                accountIds.Add(t.SourceAccountId);
            if (t.TargetAccountType == (short)TransactionAccountTypes.Account)
                accountIds.Add(t.TargetAccountId);
        }

        var accountNumbers = await _tenantDbContext.Accounts.AsNoTracking()
            .Where(x => accountIds.Contains(x.Id))
            .Select(x => new { x.Id, x.AccountNumber })
            .ToDictionaryAsync(x => x.Id, x => x.AccountNumber);

        var rebateMatterIds = items
            .Where(x => x.MatterType == MatterTypes.Rebate)
            .Select(x => x.MatterId)
            .ToList();

        var rebates = await _tenantDbContext.Rebates
            .Where(x => rebateMatterIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                Ticket = x.TradeRebate != null ? x.TradeRebate.Ticket : 0
            })
            .ToListAsync();

        var walletAdjustMatterIds = items
            .Where(x => x.MatterType == MatterTypes.WalletAdjust)
            .Select(x => x.MatterId)
            .ToList();

        var idToWalletAdjustSourceType = await _tenantDbContext.WalletAdjusts
            .Where(x => walletAdjustMatterIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => (long)x.SourceType);

        foreach (var item in items)
        {
            if (item.MatterType == MatterTypes.InternalTransfer)
            {
                var transaction = transactions.Single(x => x.Id == item.MatterId);
                item.Source = transaction.SourceAccountType == (short)TransactionAccountTypes.Wallet
                    ? 0
                    : accountNumbers[transaction.SourceAccountId];
                item.Target = transaction.TargetAccountType == (short)TransactionAccountTypes.Wallet
                    ? 0
                    : accountNumbers[transaction.TargetAccountId];
            }
            else if (item.MatterType == MatterTypes.Rebate)
            {
                item.Source = rebates.FirstOrDefault(x => x.Id == item.MatterId)?.Ticket ?? 0;
            }
            else if (item.MatterType == MatterTypes.WalletAdjust)
            {
                item.Source = idToWalletAdjustSourceType.GetValueOrDefault(item.MatterId, 0);
            }
        }


        return Result<List<Wallet.WalletTransactionViewModel>, WalletTransaction.Criteria>.Of(items, criteria);
    }

    /// <summary>
    /// Determines if a new wallet should be marked as primary for the given party.A wallet is primary if it's the first wallet for that PartyId.
    /// </summary>
    private async Task<bool> IsFirstWalletForPartyAsync(long partyId)
    {
        var hasNoWalletYet = !await _tenantDbContext.Wallets
            .AnyAsync(x => x.PartyId == partyId);
        return hasNoWalletYet;
    }
}