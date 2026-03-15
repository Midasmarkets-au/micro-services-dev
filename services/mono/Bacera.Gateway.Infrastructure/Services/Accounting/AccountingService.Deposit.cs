using System.Diagnostics;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public sealed partial class AccountingService
{
    public async Task<Deposit.ClientResponseModel> DepositGetForPartyAsync(long id, long partyId)
    {
        var model = await _tenantDbContext.Deposits
            .Include(x => x.IdNavigation)
            .Include(x => x.Payment)
            .Where(x => x.PartyId.Equals(partyId))
            .ToClientResponseModels()
            .FirstOrDefaultAsync(x => x.Id.Equals(id));
        return model ?? new Deposit.ClientResponseModel();
    }

    private async Task TryAddAccountingTransitionLogForAccount(string action, int from, int to,
        long operatorPartyId, long accountId)
    {
        // check if from and to in StateTypes:
        if (accountId == 0) return;
        try
        {
            if (!Enum.IsDefined(typeof(StateTypes), from) || !Enum.IsDefined(typeof(StateTypes), to))
            {
                _logger.LogWarning("Invalid state type: {From} or {To}", from, to);
                return;
            }

            _tenantDbContext.AccountLogs.Add(Account.BuildLog(accountId, operatorPartyId, action
                , Enum.GetName((StateTypes)from) ?? from.ToString()
                , Enum.GetName((StateTypes)to) ?? to.ToString()));

            await _tenantDbContext.SaveChangesAsync();
        }
        catch
        {
            // ignored
        }
    }


    public async Task<Deposit> DepositGetAsync(long id)
    {
        var model = await _tenantDbContext.Deposits
            .Include(x => x.IdNavigation)
            .Include(x => x.Payment)
            .FirstOrDefaultAsync(x => x.Id.Equals(id));
        return model ?? new Deposit();
    }

    public async Task<bool> DepositUpdateAsync(long id, Deposit.UpdateSpec spec, long operatorPartyId = 1)
    {
        var model = await _tenantDbContext.Deposits
            .Include(x => x.Payment)
            .Include(x => x.IdNavigation)
            .FirstOrDefaultAsync(x => x.Id.Equals(id));
        if (model == null
            || model.IdNavigation.StateId != (int)StateTypes.DepositCreated
            || model.Payment.Status != (short)PaymentStatusTypes.Pending) return false;

        model.Amount = spec.Amount;
        model.CurrencyId = (int)spec.CurrencyId;

        model.Payment.Amount = spec.Amount;
        model.Payment.CurrencyId = (int)spec.CurrencyId;

        _tenantDbContext.Deposits.Update(model);
        await _tenantDbContext.SaveChangesAsync();

        _logger.LogInformation("Deposit: {Id} updated by {PartyId}", id, operatorPartyId);
        return true;
    }

    public async Task<Result<List<DepositViewModel>, Deposit.Criteria>> DepositQueryForTenantAsync(
        Deposit.Criteria criteria, bool hideEmail = false)
    {
        var query = _tenantDbContext.Deposits
            .Include(x => x.IdNavigation)
            .Include(x => x.Payment)
            .FilterBy(criteria)
            .ToTenantViewModel(hideEmail)
            .OrderByDescending(x => x.CreatedOn);
        var items = await query
            .Skip((criteria.Page - 1) * criteria.Size)
            .Take(criteria.Size)
            .ToListAsync();
        criteria.Total = await query.CountAsync();

        // await FulfillUserForViewModel(items);
        await FulfillDepositHasSupplement(items);
        // await FulfillHasComment(items);
        await FulfillOperatorNameForDeposit(items);
        await FulfillDepositHasWalletAddress(items);
        return Result<List<DepositViewModel>, Deposit.Criteria>.Of(items, criteria);
    }

    private async Task FulfillOperatorNameForDeposit(List<DepositViewModel> items)
    {
        var matterIds = items.Select(x => x.Id).ToList();
        var activities = await _tenantDbContext.Activities
            .Where(x => matterIds.Contains(x.MatterId))
            .ToListAsync();
        var partyIds = activities.Select(x => x.PartyId).Distinct().ToList();

        var users = await _authDbContext.Users
            .Where(x => partyIds.Contains(x.PartyId) && x.TenantId == _tenantId)
            .ToListAsync();

        foreach (var item in items)
        {
            var latestActivity = activities
                .Where(x => x.MatterId == item.Id)
                .MaxBy(x => x.PerformedOn);
            if (latestActivity == null) continue;
            var user = users.FirstOrDefault(x => x.PartyId == latestActivity.PartyId);
            if (user == null) continue;
            item.OperatorName = user.GuessUserNativeName();
        }
    }

    public async Task<Result<List<Deposit.ClientResponseModel>, Deposit.Criteria>> DepositQueryForClientAsync(
        long partyId,
        Deposit.Criteria criteria)
    {
        var query = _tenantDbContext.Deposits
            .FilterBy(criteria)
            .ToClientResponseModels()
            .OrderByDescending(x => x.CreatedOn);
        var items = await query
            .Skip((criteria.Page - 1) * criteria.Size)
            .Take(criteria.Size)
            .ToListAsync();
        criteria.Total = await query.CountAsync();
        return Result<List<Deposit.ClientResponseModel>, Deposit.Criteria>.Of(items, criteria);
    }


    public async Task<Result<List<DepositViewModelForAgent>, Deposit.Criteria>> DepositQueryForParentAsync(
        Deposit.Criteria criteria)
    {
        // assert criteria.ParentAccountUid != null;
        if (criteria.ParentAccountUid == null)
            return Result<List<DepositViewModelForAgent>, Deposit.Criteria>.Of([], criteria);

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
                    .Where(x => x.Group.ToLower() == criteria.Target.ToLower() &&
                                x.Role != (int)AccountRoleTypes.Client)
                    .Where(x => x.ReferPath.Contains(criteria.ParentAccountUid!.Value.ToString()))
                    .Select(x => x.Uid)
                    .FirstOrDefaultAsync();
                if (uid == 0)
                {
                    uid = await _tenantDbContext.AccountExtraRelations
                        .Where(x => x.ParentAccount.Uid == criteria.ParentAccountUid)
                        .Where(x => x.ChildAccount.Group.ToLower() == criteria.Target.ToLower())
                        .Select(x => x.ChildAccount.Uid)
                        .FirstOrDefaultAsync();
                }

                if (uid != 0) criteria.ParentAccountUid = uid;
                else criteria.ParentAccountUid = -1;
            }
        }

        var query = _tenantDbContext.Deposits
            .FilterBy(criteria)
            .ToParentViewModel()
            .OrderByDescending(x => x.CreatedOn);
        var items = await query
            .Skip((criteria.Page - 1) * criteria.Size)
            .Take(criteria.Size)
            .ToListAsync();
        criteria.Total = await query.CountAsync();

        // Calculate total amount in a common currency (USD)
        var depositsForSum = await query
            .Select(x => new { x.Amount, x.CurrencyId })
            .ToListAsync();

        var foreignCurrencies = depositsForSum
            .Select(d => (CurrencyTypes)d.CurrencyId)
            .Where(c => c != CurrencyTypes.USD && c != CurrencyTypes.USC);
        var exchangeRates = await GetExchangeRatesForCurrenciesAsync(foreignCurrencies, CurrencyTypes.USD);

        decimal totalAmountInUSD = 0;
        foreach (var deposit in depositsForSum)
        {
            var currency = (CurrencyTypes)deposit.CurrencyId;
            decimal amountInUSD;

            if (currency == CurrencyTypes.USD)
            {
                amountInUSD = deposit.Amount / 1_000_000m;
            }
            else if (currency == CurrencyTypes.USC)
            {
                var uscUnits = deposit.Amount / 1_000_000m;
                amountInUSD = uscUnits / 100m;
            }
            else
            {
                if (!exchangeRates.TryGetValue(currency, out var exchangeRate) || exchangeRate.SellingRate <= 0)
                    continue;

                var amountInCurrencyUnits = deposit.Amount / 1_000_000m;
                amountInUSD = amountInCurrencyUnits * exchangeRate.SellingRate;
            }

            totalAmountInUSD += amountInUSD;
        }

        // Convert back to scaled format for consistency with your system
        criteria.TotalAmount = (long)(totalAmountInUSD * 1_000_000m);

        // await FulfillUserForViewModel(items);
        return Result<List<DepositViewModelForAgent>, Deposit.Criteria>.Of(items, criteria);
    }

    private async Task FulfillDepositHasSupplement(List<DepositViewModel> items)
    {
        var ids = items.Select(x => x.Id).ToList();
        var supplements = await _tenantDbContext.Supplements
            .Where(x => x.Type == (int)SupplementTypes.DepositReceipt)
            .Where(x => ids.Contains(x.RowId))
            .Select(x => x.RowId)
            .ToListAsync();
        foreach (var item in items)
        {
            item.HasReceipt = supplements.Contains(item.Id);
        }
    }

    private async Task FulfillDepositHasWalletAddress(List<DepositViewModel> items)
    {
        var ids = items.Select(x => x.Id).ToList();
        var supplements = await _tenantDbContext.Supplements
            .Where(x => x.Type == (int)SupplementTypes.Deposit && ids.Contains(x.RowId))
            .Select(x => new { x.RowId, x.Data })
            .ToListAsync();
         // new wallet address 
        var referenceNumbers = items.Select(x => x.Payment.ReferenceNumber).Where(x => !string.IsNullOrEmpty(x)).ToList();
        var cryptoTransactions = await _tenantDbContext.CryptoTransactions
            .Where(x => referenceNumbers.Contains(x.TransactionHash))
            .ToListAsync();
        foreach (var item in items)
        {
            var cryptoTransaction = cryptoTransactions.FirstOrDefault(x => x.PaymentId == item.Payment.Id);
            var record = supplements.FirstOrDefault(x => x.RowId == item.Id);
            if (record != null)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<dynamic>(record.Data);
                    item.WalletAddress = obj?.Request?.walletAddress ?? string.Empty;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            if( item.WalletAddress == string.Empty && cryptoTransaction != null){
                item.WalletAddress = cryptoTransaction.FromAddress;
            }
        }

    }

    // private async Task FulfillHasComment(List<DepositViewModel> items)
    // {
    //     var accountIds = items
    //         .Select(x => x.TargetTradeAccountId)
    //         .Where(x => x > 0)
    //         .Select(x => x!.Value)
    //         .Distinct()
    //         .ToList();
    //
    //     var userIds = items
    //         .Select(x => x.User.PartyId)
    //         .Where(x => x > 0)
    //         .Distinct()
    //         .ToList();
    //
    //     var commentKeys = await _tenantDbContext.Comments
    //         .Where(x => (accountIds.Contains(x.RowId) && x.Type == (int)CommentTypes.Account)
    //                     || (userIds.Contains(x.RowId) && x.Type == (int)CommentTypes.User))
    //         .GroupBy(x => new { x.RowId, x.Type })
    //         .Select(g => new { g.Key.RowId, g.Key.Type })
    //         .ToListAsync();
    //
    //     foreach (var item in items)
    //     {
    //         item.User.HasComment = commentKeys.Any(x => x.RowId == item.User.PartyId && x.Type == (int)CommentTypes.User);
    //         item.TargetTradeAccount.HasComment = commentKeys.Any(x =>
    //             x.RowId == item.TargetTradeAccountId && x.Type == (int)CommentTypes.Account);
    //     }
    // }
}