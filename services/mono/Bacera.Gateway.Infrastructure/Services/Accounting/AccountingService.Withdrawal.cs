using Bacera.Gateway.Services.Common;
using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public sealed partial class AccountingService
{
    public async Task<Withdrawal> WithdrawalCreateAsync(long paymentServiceId, long partyId, FundTypes fundType,
        CurrencyTypes currencyId,
        long amount, long operatorPartyId, Supplement.WithdrawalSupplement? supplement = null,
        long? sourceTradeAccountId = null, long? sourceWalletId = null)
    {
        var wallet = await WalletGetOrCreateAsync(partyId, currencyId, fundType);

        // won't check balance when transfer to target trade account
        if (wallet.Balance < amount.ToScaledFromCents() && sourceTradeAccountId == null) return new Withdrawal();

        var paymentServiceCurrencyId = await _tenantDbContext.PaymentServices
            .Where(x => x.Id == paymentServiceId)
            .Select(x => x.CurrencyId)
            .FirstOrDefaultAsync();

        var exchangeRate = 1m;
        if (currencyId != (CurrencyTypes)paymentServiceCurrencyId)
        {
            var rate = await GetExchangeRateAsync(currencyId, (CurrencyTypes)paymentServiceCurrencyId);
            // if (rate != null) exchangeRate = rate.BuyingRate;
            if (rate != null) exchangeRate = rate.BuyingRate * (1 - rate.AdjustRate / 100);
            exchangeRate = Math.Floor(exchangeRate * 1000) / 1000;
        }

        var withdraw = Withdrawal.Build(partyId, fundType, amount, currencyId);
        withdraw.SourceAccountId = sourceTradeAccountId;
        withdraw.SourceWalletId = sourceWalletId;
        withdraw.ExchangeRate = exchangeRate;
        withdraw.IdNavigation.AddActivity(operatorPartyId, ActionTypes.WithdrawalCreate);
        withdraw.Payment = Payment.Build(partyId, LedgerSideTypes.Debit, paymentServiceId, amount, currencyId);

        await TryAddAccountingTransitionLogForAccount("WithdrawalCreated", 0,
            (int)StateTypes.WithdrawalCreated, operatorPartyId, withdraw.SourceAccountId ?? 0);


        await _tenantDbContext.Withdrawals.AddAsync(withdraw);
        await _tenantDbContext.SaveChangesAsync();

        // FIX: Load the PaymentMethod navigation property after saving, otherwise controller ToClientResponseModel() will raise error
        await _tenantDbContext.Entry(withdraw.Payment)
            .Reference(p => p.PaymentMethod)
            .LoadAsync();

        _logger.LogInformation("Withdrawal: {Id} created by {PartyId}", withdraw.Id, operatorPartyId);
        if (supplement == null) return withdraw;

        supplement.ExchangeRate = exchangeRate;
        supplement.TargetCurrencyId = (CurrencyTypes)paymentServiceCurrencyId;
        var withdrawalSupplement = new Supplement
        {
            RowId = withdraw.Id,
            Type = (int)SupplementTypes.Withdraw,
            Data = JsonConvert.SerializeObject(supplement),
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };
        await _tenantDbContext.Supplements.AddAsync(withdrawalSupplement);
        await _tenantDbContext.SaveChangesAsync();

        return withdraw;
    }

    public async Task<bool> WithdrawalUpdateAsync(long id, Withdrawal.UpdateSpec spec, long operatorPartyId = 1)
    {
        var model = await _tenantDbContext.Withdrawals
            .Include(x => x.Payment)
            .Include(x => x.IdNavigation)
            .FirstOrDefaultAsync(x => x.Id.Equals(id));
        if (model == null
            || model.IdNavigation.StateId != (int)StateTypes.WithdrawalCreated
            || model.Payment.Status != (short)PaymentStatusTypes.Pending) return false;

        model.Amount = spec.Amount;

        model.Payment.Amount = spec.Amount;
        model.Payment.PaymentServiceId = (int)spec.PaymentServiceId;

        _tenantDbContext.Withdrawals.Update(model);
        await _tenantDbContext.SaveChangesAsync();

        _logger.LogInformation("Withdrawal: {Id} updated by {PartyId}", id, operatorPartyId);
        return true;
    }

    public async Task<bool> WithdrawalTryCancelAsync(long id, long operatorPartyId)
    {
        var item = await _tenantDbContext.Withdrawals
            .Include(x => x.IdNavigation)
            .Include(x => x.Payment) // TODO: Multiple payments for parent Id if needed
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item == null || item.IdNavigation.StateId != (int)StateTypes.WithdrawalCreated) return false;
        var legacyStateId = item.IdNavigation.StateId;
        var result = await TransitAsync(item, ActionTypes.WithdrawalCancel, operatorPartyId);

        if (result.Item1)
        {
            await TryAddAccountingTransitionLogForAccount("WithdrawalCanceled", legacyStateId,
                result.Item2.StateId, operatorPartyId, item.SourceAccountId ?? 0);

            _logger.LogInformation("Withdrawal: {Id} canceled by {PartyId}", id, operatorPartyId);
        }
        else _logger.LogWarning("Withdrawal: {Id} cancel failed by {PartyId}", id, operatorPartyId);

        return true;
    }

    public async Task<bool> WithdrawalTryCompletePaymentAsync(long withdrawalId, long operatorPartyId)
    {
        var withdrawal = await _tenantDbContext.Withdrawals
            .Include(x => x.IdNavigation)
            .Include(x => x.Payment)
            .FirstOrDefaultAsync(x => x.Id == withdrawalId);

        if (withdrawal == null) return false;
        if (withdrawal.Payment.Status != (short)PaymentStatusTypes.Completed ||
            withdrawal.Payment.Amount != withdrawal.Amount)
            return false;
        var legacyStateId = withdrawal.IdNavigation.StateId;

        var result = await TransitAsync(withdrawal, ActionTypes.WithdrawalExecutePayment, operatorPartyId);

        if (!result.Item1) return false;

        await TryAddAccountingTransitionLogForAccount("WithdrawalPaymentCompleted", legacyStateId,
            result.Item2.StateId, operatorPartyId, withdrawal.SourceAccountId ?? 0);

        _logger.LogInformation("Withdrawal: {Id} Payment completed by {PartyId}", withdrawal?.Id, operatorPartyId);
        return true;
    }

    public async Task<bool> WithdrawalCompleteAsync(Withdrawal model, long operatorPartyId,
        string referenceNumber = "")
    {
        var legacyStateId = model.IdNavigation.StateId;
        var result = await TransitAsync(model, ActionTypes.WithdrawalComplete, operatorPartyId);
        if (!result.Item1)
        {
            _logger.LogWarning("Withdrawal: {Id} complete failed by {PartyId}", model.Id, operatorPartyId);
            return false;
        }

        _logger.LogInformation("Withdrawal: {Id} complete by {PartyId}", model.Id, operatorPartyId);
        if (string.IsNullOrEmpty(referenceNumber)) return true;

        var transaction = await _tenantDbContext.Withdrawals
            .SingleAsync(x => x.Id == model.Id);
        transaction.ReferenceNumber = referenceNumber;

        await TryAddAccountingTransitionLogForAccount("WithdrawalCompleted", legacyStateId,
            result.Item2.StateId, operatorPartyId, model.SourceAccountId ?? 0);

        _tenantDbContext.Update(transaction);
        await _tenantDbContext.SaveChangesAsync();

        return true;
    }

    public async Task<Withdrawal> WithdrawalGetAsync(long id)
    {
        var model = await _tenantDbContext.Withdrawals
            .Include(x => x.IdNavigation)
            .FirstOrDefaultAsync(x => x.Id.Equals(id));
        return model ?? new Withdrawal();
    }

    public async Task<bool> WithdrawalRejectByTenantAsync(Withdrawal item, long operatorPartyId)
    {
        if (item.Id == 0) return false;

        var legacyStateId = item.IdNavigation.StateId;
        var result = await TransitAsync(item, ActionTypes.WithdrawalTenantReject, operatorPartyId);
        if (!result.Item1)
        {
            _logger.LogWarning("Withdrawal: {Id} tenant reject failed by {PartyId}", item.Id, operatorPartyId);
            return false;
        }

        await TryAddAccountingTransitionLogForAccount("WithdrawalRejected", legacyStateId,
            result.Item2.StateId, operatorPartyId, item.SourceAccountId ?? 0);

        _logger.LogInformation("Withdrawal: {Id} tenant rejected by {PartyId}", item.Id, operatorPartyId);
        return true;
    }

    public async Task<bool> WithdrawalRefundByTenantAsync(Withdrawal item, long operatorPartyId)
    {
        if (item.Id == 0) return false;
        var legacyStateId = item.IdNavigation.StateId;

        var result = await TransitAsync(item, ActionTypes.WithdrawalTenantRefund, operatorPartyId);
        if (!result.Item1)
        {
            _logger.LogWarning("Withdrawal: {Id} tenant refund failed by {PartyId}", item.Id, operatorPartyId);
            return false;
        }

        await TryAddAccountingTransitionLogForAccount("WithdrawalRefunded", legacyStateId,
            result.Item2.StateId, operatorPartyId, item.SourceAccountId ?? 0);

        _logger.LogInformation("Withdrawal: {Id} tenant refund by {PartyId}", item.Id, operatorPartyId);
        return true;
    }

    public async Task<Withdrawal.ClientResponseModel> WithdrawalGetForPartyAsync(long id, long partyId)
    {
        var model = await _tenantDbContext.Withdrawals
            .Where(x => x.PartyId.Equals(partyId))
            .ToClientResponseModel()
            .FirstOrDefaultAsync(x => x.Id.Equals(id));
        return model ?? new Withdrawal.ClientResponseModel();
    }

    public async Task<Result<List<WithdrawalViewModel>, Withdrawal.Criteria>> WithdrawalQueryForTenantAsync(
        Withdrawal.Criteria criteria, bool hideEmail = false)
    {
        var query = _tenantDbContext.Withdrawals.FilterBy(criteria);


        if (criteria.WalletId != null)
        {
            var wallet = await _tenantDbContext.Wallets.SingleOrDefaultAsync(x => x.Id == criteria.WalletId);
            if (wallet == null) return Result<List<WithdrawalViewModel>, Withdrawal.Criteria>.Of([], criteria);

            query = query.Where(x => x.PartyId == wallet.PartyId);
            query = query.Where(x => x.CurrencyId == wallet.CurrencyId);
            query = query.Where(x => x.FundType == wallet.FundType);
            query = query.Where(x => x.SourceAccountId == null);
        }

        var items = await query
            .ToTenantViewModel(hideEmail)
            .OrderByDescending(x => x.CreatedOn)
            .Skip((criteria.Page - 1) * criteria.Size)
            .Take(criteria.Size)
            .ToListAsync();

        criteria.Total = await query.CountAsync();

        // await FulfillUserForViewModel(items);
        // await FulfillHasComment(items);
        await FulfillOperatorNameForWithdrawal(items);
        return Result<List<WithdrawalViewModel>, Withdrawal.Criteria>.Of(items, criteria);
    }

    private async Task FulfillOperatorNameForWithdrawal(List<WithdrawalViewModel> items)
    {
        var matterIds = items.Select(x => x.Id).ToList();
        var activities = await _tenantDbContext.Activities
            .Where(x => matterIds.Contains(x.MatterId))
            .ToListAsync();
        var partyIds = activities.Select(x => x.PartyId).ToList();

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

    public async Task<Result<List<Withdrawal.ClientResponseModel>, Withdrawal.Criteria>> WithdrawalQueryForClientAsync(
        long partyId,
        Withdrawal.Criteria criteria)
    {
        criteria.PartyId = partyId;
        var items = await _tenantDbContext.Withdrawals
            .Include(x => x.IdNavigation)
            .PagedFilterBy(criteria)
            .ToClientResponseModel()
            .ToListAsync();
        return Result<List<Withdrawal.ClientResponseModel>, Withdrawal.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<WithdrawalViewModelForAgent>, Withdrawal.Criteria>> WithdrawalQueryForParentAsync(
        Withdrawal.Criteria criteria)
    {
        if (criteria.ParentAccountUid == null)
            return Result<List<WithdrawalViewModelForAgent>, Withdrawal.Criteria>.Of([], criteria);

        var query = _tenantDbContext.Withdrawals
            .Include(x => x.IdNavigation)
            .OrderByDescending(x => x.IdNavigation.StatedOn)
            .AsQueryable();

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
                else criteria.ParentAccountUid = null;
            }
        }

        if (criteria.Email != null)
            query = query.Where(x => x.Party.Email == criteria.Email);

        if (criteria.CurrencyId != null)
            query = query.Where(x => x.CurrencyId == (int)criteria.CurrencyId);

        if (criteria.FundType != null)
            query = query.Where(x => x.FundType == (int)criteria.FundType);

        if (criteria.From != null)
            query = query.Where(x => x.IdNavigation.StatedOn >= criteria.From.Value);

        if (criteria.To != null)
            query = query.Where(x => x.IdNavigation.StatedOn <= criteria.To.Value);

        if (criteria.StateId != null)
            query = query.Where(x => x.IdNavigation.StateId == (int)criteria.StateId);

        if (criteria.StateIds is { Count: > 0 })
            query = query.Where(x => criteria.StateIds.Contains(x.IdNavigation.StateId));

        if (criteria.PaymentCurrencyId != null)
            query = query.Where(x => x.Payment.CurrencyId == (int)criteria.PaymentCurrencyId);


        if (criteria.Role is AccountRoleTypes.Agent or AccountRoleTypes.Sales && criteria.ParentAccountUid != null)
        {
            query = query.Where(x => x.SourceAccount == null);
            var path = await _tenantDbContext.Accounts
                .Where(x => x.Uid == criteria.ParentAccountUid)
                .Select(x => x.ReferPath)
                .SingleAsync();
            var childPartyIds = await _tenantDbContext.Accounts
                .Where(x => x.ReferPath.StartsWith(path))
                .Where(x => x.Role == (short)AccountRoleTypes.Agent || x.Role == (short)AccountRoleTypes.Sales)
                .Select(x => x.PartyId)
                .Distinct()
                .ToListAsync();
            query = query.Where(x => childPartyIds.Contains(x.PartyId));
        }
        else
        {
            query = query.Where(x => x.SourceAccount != null);
            query = query.Where(x =>
                criteria.ParentAccountUid == null ||
                x.SourceAccount!.ReferPath.Contains(criteria.ParentAccountUid.Value.ToString()));
            query = query.Where(x => criteria.AccountUid == null || x.SourceAccount!.Uid == criteria.AccountUid);
            query = query.Where(x => criteria.AccountNumber == null || x.SourceAccount!.AccountNumber == criteria.AccountNumber);
        }

        var items = await query.ToParentViewModel()
            .Skip((criteria.Page - 1) * criteria.Size)
            .Take(criteria.Size)
            .ToListAsync();

        criteria.Total = await query.CountAsync();

        // Calculate total amount in a common currency (USD)
        var withdrawalsForSum = await query
            .Select(x => new { x.Amount, x.CurrencyId })
            .ToListAsync();

        var foreignCurrencies = withdrawalsForSum
            .Select(w => (CurrencyTypes)w.CurrencyId)
            .Where(c => c != CurrencyTypes.USD && c != CurrencyTypes.USC);
        var exchangeRates = await GetExchangeRatesForCurrenciesAsync(foreignCurrencies, CurrencyTypes.USD);

        decimal totalAmountInUSD = 0;
        foreach (var withdrawal in withdrawalsForSum)
        {
            var currency = (CurrencyTypes)withdrawal.CurrencyId;
            decimal amountInUSD;

            if (currency == CurrencyTypes.USD)
            {
                amountInUSD = withdrawal.Amount / 1_000_000m;
            }
            else if (currency == CurrencyTypes.USC)
            {
                var uscUnits = withdrawal.Amount / 1_000_000m;
                amountInUSD = uscUnits / 100m;
            }
            else
            {
                if (!exchangeRates.TryGetValue(currency, out var exchangeRate) || exchangeRate.SellingRate <= 0)
                    continue;

                var amountInCurrencyUnits = withdrawal.Amount / 1_000_000m;
                amountInUSD = amountInCurrencyUnits * exchangeRate.SellingRate;
            }

            totalAmountInUSD += amountInUSD;
        }

        // Convert back to scaled format for consistency with your system
        criteria.TotalAmount = (long)(totalAmountInUSD * 1_000_000m);

        // await FulfillUserForViewModel(items);
        return Result<List<WithdrawalViewModelForAgent>, Withdrawal.Criteria>.Of(items, criteria);
    }
}