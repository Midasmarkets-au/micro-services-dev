using Bacera.Gateway.Core.Types;
using Bacera.Gateway.MyException;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Services.Acct;

public partial class AcctService
{
    public async Task<Transaction?> CreateTransactionFromWalletToTradeAccountAsync(long walletId, long accountId, long amount,
        long operatorPartyId = 1, string note = "")
    {
        await using var transaction = await tenantCtx.Database.BeginTransactionAsync();
        try
        {
            var wallet = await tenantCtx.Wallets
                .Where(x => x.Id == walletId)
                .Select(x => new { x.PartyId, x.FundType, x.CurrencyId })
                .SingleAsync();

            var item = Transaction.Build(
                wallet.PartyId,
                TransactionAccountTypes.Wallet,
                walletId,
                wallet.PartyId,
                TransactionAccountTypes.Account,
                accountId,
                LedgerSideTypes.Debit, amount,
                (FundTypes)wallet.FundType,
                (CurrencyTypes)wallet.CurrencyId
            );

            item.IdNavigation.AddActivity(StateTypes.Initialed, StateTypes.TransferAwaitingApproval, operatorPartyId, note);
            tenantCtx.Transactions.Add(item);
            await tenantCtx.SaveChangesAsync();

            var setting = await cfgSvc.GetAsync<ApplicationConfigure.AutoCompleteTransactionAmountValue>(nameof(Public), 0,
                ConfigKeys.AutoCompleteTransactionAmount);

            if (setting?.Enabled == true)
            {
                await TransferFromWalletToAccountAsync(item, operatorPartyId);
                TransitRaw(item, StateTypes.TransferCompleted, operatorPartyId, note);
                await tenantCtx.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return item;
        }
        catch (Exception e)
        {
            // BcrLog.Slack($"Error_CreateTransactionFromWalletToTradeAccountAsync_tid_{_tenantId}_wid_{walletId}_aid_{accountId}_{e.Message}");
            logger.LogError(e, "Error_CreateTransactionFromWalletToTradeAccountAsync_tid_{_tenantId}_wid_{walletId}_aid_{accountId}", _tenantId,
                walletId, accountId);
            await transaction.RollbackAsync();
            tenantCtx.ChangeTracker.Clear();
            return null;
        }
    }

    /// <summary>
    /// Create transaction from wallet to trade account with currency conversion support
    /// </summary>
    public async Task<Transaction?> CreateTransactionFromWalletToTradeAccountWithConversionAsync(
        long walletId, long accountId, long accountAmount, 
        CurrencyTypes walletCurrency, CurrencyTypes accountCurrency,
        long operatorPartyId = 1, string note = "")
    {
        await using var transaction = await tenantCtx.Database.BeginTransactionAsync();
        try
        {
            var wallet = await tenantCtx.Wallets
                .Where(x => x.Id == walletId)
                .Select(x => new { x.PartyId, x.FundType, x.CurrencyId })
                .SingleAsync();

            // Calculate wallet amount if currencies differ
            long walletAmount = accountAmount;
            decimal exchangeRate = 1.0m;
            
            if (walletCurrency != accountCurrency)
            {
                exchangeRate = await GetExchangeRateAsync(walletCurrency, accountCurrency);
                if (exchangeRate <= 0)
                    throw new InvalidOperationException($"Exchange rate not available for {walletCurrency} to {accountCurrency}");
                
                // Do it in controller already
                // walletAmount = (long)Math.Ceiling(accountAmount / (double)exchangeRate);
            }

            // *** Apply exchange rate to transaction amount. Original it seems only store USD ***
            long transactionAmount = accountAmount;
            if (walletCurrency != accountCurrency)
            {
                transactionAmount = (long)(accountAmount * exchangeRate); // Apply exchange rate multiplier
            }

            var item = Transaction.Build(
                wallet.PartyId,
                TransactionAccountTypes.Wallet,
                walletId,
                wallet.PartyId,
                TransactionAccountTypes.Account,
                accountId,
                LedgerSideTypes.Debit, transactionAmount, // Use transactionAmount instead of accountAmount
                (FundTypes)wallet.FundType,
                accountCurrency // Use account currency as transaction currency
            );

            // Store conversion info in transaction note if currencies differ
            if (walletCurrency != accountCurrency)
            {
                note += $" [Conversion: {walletAmount} {walletCurrency} -> {accountAmount} {accountCurrency} @ {exchangeRate:F6}]";
            }

            item.IdNavigation.AddActivity(StateTypes.Initialed, StateTypes.TransferAwaitingApproval, operatorPartyId, note);
            tenantCtx.Transactions.Add(item);
            await tenantCtx.SaveChangesAsync();

            var setting = await cfgSvc.GetAsync<ApplicationConfigure.AutoCompleteTransactionAmountValue>(nameof(Public), 0,
                ConfigKeys.AutoCompleteTransactionAmount);

            if (setting?.Enabled == true)
            {
                await TransferFromWalletToAccountWithConversionAsync(item, walletAmount, accountAmount, transactionAmount, operatorPartyId);
                TransitRaw(item, StateTypes.TransferCompleted, operatorPartyId, note);
                await tenantCtx.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return item;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error_CreateTransactionFromWalletToTradeAccountWithConversionAsync_tid_{_tenantId}_wid_{walletId}_aid_{accountId}", 
                _tenantId, walletId, accountId);
            await transaction.RollbackAsync();
            tenantCtx.ChangeTracker.Clear();
            return null;
        }
    }

    /// <summary>
    /// Get exchange rate between currencies using the same logic as finance summary
    /// </summary>
    public async Task<decimal> GetExchangeRateAsync(CurrencyTypes fromCurrency, CurrencyTypes toCurrency)
    {
        if (fromCurrency == toCurrency) return 1.0m;

        // Check if this is a USC conversion (use programmatic rate)
        var (isUscConversion, uscRate) = CurrencyConversionHelper.GetUscConversionRate(fromCurrency, toCurrency);
        
        if (isUscConversion)
        {
            if (uscRate == -1.0m)
            {
                // Multi-step USC conversion
                var finalRate = await CurrencyConversionHelper.CalculateUscConversionRateDoubleAsync(
                    fromCurrency, 
                    toCurrency,
                    async (from, to) => await GetMtExchangeRateForTransaction(from, to));
                return (decimal)finalRate;
            }
            
            // Direct USC conversion
            return uscRate;
        }

        // Regular currency conversion using MT exchange rates
        var exchangeRate = await GetMtExchangeRateForTransaction(fromCurrency, toCurrency);
        if (exchangeRate <= 0)
        {
            logger.LogWarning("Exchange rate not found for {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            return 0; // Indicates no rate found
        }

        return (decimal)exchangeRate;
    }

    private async Task<double> GetMtExchangeRateForTransaction(CurrencyTypes from, CurrencyTypes to)
    {
        if (from == to) return 1.0;

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

        return 0; // No rate found
    }

    public async Task<Transaction?> CreateTransactionBetweenWalletAsync(long sourceWalletId, long targetWalletId,
        long amount, long operatorPartyId = 1, string note = "")
    {
        await using var transaction = await tenantCtx.Database.BeginTransactionAsync();
        try
        {
            var wallets = await tenantCtx.Wallets
                .Where(x => x.Id == sourceWalletId || x.Id == targetWalletId)
                .Select(x => new { x.Id, x.PartyId, x.FundType, x.CurrencyId })
                .ToListAsync();

            if (wallets.Count != 2) throw new ProcessMatterException("Source or target wallet not found");

            var sourceWallet = wallets.Single(x => x.Id == sourceWalletId);
            var targetWallet = wallets.Single(x => x.Id == targetWalletId);

            var item = Transaction.Build(
                sourceWallet.PartyId,
                TransactionAccountTypes.Wallet,
                sourceWalletId,
                targetWallet.PartyId,
                TransactionAccountTypes.Wallet,
                targetWalletId,
                LedgerSideTypes.Debit, amount,
                (FundTypes)sourceWallet.FundType,
                (CurrencyTypes)sourceWallet.CurrencyId
            );

            item.IdNavigation.AddActivity(StateTypes.Initialed, StateTypes.TransferAwaitingApproval, operatorPartyId,
                note);
            tenantCtx.Transactions.Add(item);
            await tenantCtx.SaveChangesAsync();

            var setting = await cfgSvc.GetAsync<ApplicationConfigure.AutoCompleteTransactionAmountValue>(nameof(Public),
                0, ConfigKeys.AutoCompleteTransactionAmount);

            // Only auto-complete if enabled AND amount is within threshold
            // For amounts > threshold, let TransferCreatedEventHandler handle manager approval
            if (setting?.Enabled == true && amount <= setting.Amount)
            {
                await TransferBetweenWalletAsync(item, operatorPartyId);
                TransitRaw(item, StateTypes.TransferCompleted, operatorPartyId, note);
                await tenantCtx.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return item;
        }
        catch (Exception e)
        {
            BcrLog.Slack(
                $"Error_CreateTransactionBetweenWalletAsync_tid_{_tenantId}_swid_{sourceWalletId}_twid_{targetWalletId}_{e.Message}");
            await transaction.RollbackAsync();
            tenantCtx.ChangeTracker.Clear();
            return null;
        }
    }

    // public async Task<(bool, string)> CreateTransactionBetweenTradeAccountAsync(long fromId, long toId, long amount,
    //     long operatorPartyId = 1, string note = "")
    // {
    //     var transaction = await tenantCtx.Database.BeginTransactionAsync();
    //     try
    //     {
    //         var accounts = await tenantCtx.Accounts
    //             .Where(x => x.Id == fromId || x.Id == toId)
    //             .Select(x => new { x.Id, x.ServiceId, x.AccountNumber, x.TradeAccountStatus!.Balance })
    //             .ToListAsync();
    //
    //         var item = Transaction.Build(
    //             wallet.PartyId,
    //             TransactionAccountTypes.Wallet,
    //             walletId,
    //             wallet.PartyId,
    //             TransactionAccountTypes.Account,
    //             accountId,
    //             LedgerSideTypes.Debit, amount,
    //             (FundTypes)wallet.FundType,
    //             (CurrencyTypes)wallet.CurrencyId
    //         );
    //
    //         item.IdNavigation.AddActivity(StateTypes.Initialed, StateTypes.TransferCreated, operatorPartyId, note);
    //         tenantCtx.Transactions.Add(item);
    //         await tenantCtx.SaveChangesAsync();
    //
    //         var setting = await cfgSvc.GetAsync<ApplicationConfigure.AutoCompleteTransactionAmountValue>(nameof(Public), 0,
    //             ConfigKeys.AutoCompleteTransactionAmount);
    //
    //         if (setting?.Enabled == true)
    //         {
    //             await TransferFromWalletToAccountAsync(item, operatorPartyId);
    //             TransitRaw(item, StateTypes.TransferCompleted, operatorPartyId, note);
    //         }
    //
    //         await transaction.CommitAsync();
    //         return (true, string.Empty);
    //     }
    //     catch (Exception e)
    //     {
    //         BcrLog.Slack($"Error_CreateTransactionFromWalletToTradeAccountAsync_tid_{_tenantId}_wid_{walletId}_aid_{accountId}_{e.Message}");
    //         await transaction.RollbackAsync();
    //         return (false, "Create transaction failed");
    //     }
    // }
    
    public Task<(bool, string)> CompleteTransactionAsync(long transactionId, long operatorPartyId = 1, string note = "") => ProcessMatterAsync(
        transactionId,
        async () =>
        {
            var item = await tenantCtx.Transactions
                .Include(x => x.IdNavigation)
                .SingleOrDefaultAsync(x => x.Id == transactionId);
            if (item == null) throw new ProcessMatterException("Transaction not found");
            if (item.IdNavigation.StateId != (int)StateTypes.TransferAwaitingApproval)
                throw new ProcessMatterException("Transaction not in awaiting approval state");

            switch (item)
            {
                case
                {
                    SourceAccountType: (int)TransactionAccountTypes.Wallet,
                    TargetAccountType: (int)TransactionAccountTypes.Account
                }:
                {
                    await TransferFromWalletToAccountAsync(item, operatorPartyId);
                    break;
                }
                case
                {
                    SourceAccountType: (int)TransactionAccountTypes.Account,
                    TargetAccountType: (int)TransactionAccountTypes.Wallet
                }:
                {
                    await TransferFromAccountToWalletAsync(item, operatorPartyId);
                    break;
                }
                case
                {
                    SourceAccountType: (int)TransactionAccountTypes.Account,
                    TargetAccountType: (int)TransactionAccountTypes.Account
                }:
                {
                    await TransferFromAccountToAccountAsync(item, operatorPartyId);
                    break;
                }
                case
                {
                    SourceAccountType: (int)TransactionAccountTypes.Wallet,
                    TargetAccountType: (int)TransactionAccountTypes.Wallet
                }:
                {
                    await TransferBetweenWalletAsync(item, operatorPartyId);
                    break;
                }
            }

            TransitRaw(item, StateTypes.TransferCompleted, operatorPartyId, note);
        });

    private async Task TransferFromAccountToAccountAsync(Transaction item, long operatorPartyId = 1)
    {
        var amountForSourceAccount = -1 * (decimal)item.Amount / 100;
        var amountForTargetAccount = (decimal)item.Amount / 100;
        var accounts = await tenantCtx.Accounts
            .Where(x => x.Status == 0)
            .Where(x => x.Id == item.SourceAccountId || x.Id == item.TargetAccountId)
            .Select(x => new { x.Id, x.ServiceId, x.AccountNumber, x.CurrencyId, x.TradeAccountStatus!.Balance })
            .ToListAsync();
        if (accounts.Count != 2) throw new ProcessMatterException("Account Status not valid");
        var sourceAccount = accounts.Single(x => x.Id == item.SourceAccountId);
        var targetAccount = accounts.Single(x => x.Id == item.TargetAccountId);

        // if (sourceAccount.ServiceId != targetAccount.ServiceId) throw new ProcessMatterException("Transfer between different services not allowed");
        var exchangeRatedTargetAmount = amountForTargetAccount;
        if (sourceAccount.CurrencyId != targetAccount.CurrencyId)
        {
            var exchangeRate = await GetExchangeRateAsync((CurrencyTypes)sourceAccount.CurrencyId, (CurrencyTypes)targetAccount.CurrencyId);
            if (exchangeRate <= 0)
                throw new InvalidOperationException($"Exchange rate not available for {sourceAccount.CurrencyId} to {targetAccount.CurrencyId}");
            exchangeRatedTargetAmount = (long)(amountForTargetAccount * exchangeRate);
        }

        var (res1, sourceTicket) = await apiService.ChangeBalance(sourceAccount.ServiceId, sourceAccount.AccountNumber,
            amountForSourceAccount, $"Withdrawal TF to {targetAccount.AccountNumber}");
        if (!res1) throw new ProcessMatterException("Withdrawal TF to account failed");

        var (res2, targetTicket) = await apiService.ChangeBalance(targetAccount.ServiceId, targetAccount.AccountNumber,
            exchangeRatedTargetAmount, $"Deposit TF from {sourceAccount.AccountNumber}");
        if (!res2) throw new ProcessMatterException("Deposit TF from account failed");

        item.ReferenceNumber = $"{sourceTicket},{targetTicket}";

        var (srcRes, _, srcBalance) = await TradeAccountUpdateBalanceAndLeverageAsync(item.SourceAccountId);
        if (!srcRes) return;

        var srcAccountLog = new AccountLog
        {
            AccountId = item.SourceAccountId,
            OperatorPartyId = operatorPartyId,
            Action = $"TransferToAccount:{targetAccount.AccountNumber}",
            Before = $"Balance: {sourceAccount.Balance}",
            After = $"Balance: {srcBalance}",
            CreatedOn = DateTime.UtcNow,
        };
        tenantCtx.AccountLogs.Add(srcAccountLog);

        var (tgtRes, _, tgtBalance) = await TradeAccountUpdateBalanceAndLeverageAsync(item.TargetAccountId);
        if (!tgtRes) return;

        var tgtAccountLog = new AccountLog
        {
            AccountId = item.TargetAccountId,
            OperatorPartyId = operatorPartyId,
            Action = $"TransferFromAccount:{sourceAccount.AccountNumber}",
            Before = $"Balance: {targetAccount.Balance}",
            After = $"Balance: {tgtBalance}",
            CreatedOn = DateTime.UtcNow,
        };
        tenantCtx.AccountLogs.Add(tgtAccountLog);
    }

    private async Task TransferFromAccountToWalletAsync(Transaction item, long operatorPartyId = 1)
    {
        var account = await tenantCtx.Accounts
            .Where(x => x.Id == item.TargetAccountId && x.Status == 0)
            .Select(x => new { x.Id, x.ServiceId, x.AccountNumber, x.TradeAccountStatus!.Balance })
            .SingleOrDefaultAsync();
        if (account == null) throw new ProcessMatterException("Account Status not valid");

        var amountForAccount = -1 * (decimal)item.Amount / 100;
        var amountInCentsForWallet = item.Amount; // removed * 10000 coz item.Amount is from transaction table
        var (result, ticket) = await apiService.ChangeBalance(account.ServiceId, account.AccountNumber, amountForAccount,
            $"Withdrawal TF to Wallet");
        if (!result) throw new ProcessMatterException("Withdrawal TF to wallet failed");

        await WalletChangeBalanceRawAsync(item.TargetAccountId, item.Id, amountInCentsForWallet);
        item.ReferenceNumber = ticket;
        var (res, _, balance) = await TradeAccountUpdateBalanceAndLeverageAsync(item.TargetAccountId);
        if (!res) return;

        var accountLog = new AccountLog
        {
            AccountId = account.Id,
            OperatorPartyId = operatorPartyId,
            Action = $"TransferToWallet:{item.TargetAccountId}",
            Before = $"Balance: {account.Balance}",
            After = $"Balance: {balance}",
            CreatedOn = DateTime.UtcNow,
        };
        tenantCtx.AccountLogs.Add(accountLog);
    }

    private async Task TransferFromWalletToAccountAsync(Transaction item, long operatorPartyId = 1)
    {
        var account = await tenantCtx.Accounts
            .Where(x => x.Id == item.TargetAccountId)
            .Select(x => new { x.Id, x.ServiceId, x.AccountNumber, x.Status, x.TradeAccountStatus!.Balance })
            .SingleOrDefaultAsync();
        if (account == null) throw new ProcessMatterException("Account not found");
        if (account.Status != 0) throw new ProcessMatterException("Account Status not valid");

        var amountForAccount = (decimal)item.Amount / 100; // item.Amount is from db; Divide by 10000 logic is moved to central TradingApiService.ChangeBalance()
        var amountOutCentsFromWallet = -1 * item.Amount; // Fixed: Use direct amount without extra multiplication
        await WalletChangeBalanceRawAsync(item.SourceAccountId, item.Id, amountOutCentsFromWallet);

        var (result, ticket) = await apiService.ChangeBalance(account.ServiceId, account.AccountNumber, amountForAccount,
            $"Deposit TF from Wallet");
        if (!result) throw new ProcessMatterException("Deposit TF from wallet failed");

        item.ReferenceNumber = ticket;
        tenantCtx.Entry(item).Property(y => y.ReferenceNumber).IsModified = true;

        double updatedBalance;
        try
        {
            var (res, _, balance) = await TradeAccountUpdateBalanceAndLeverageAsync(item.TargetAccountId);
            updatedBalance = balance;
            if (!res) return;
        }
        catch (Exception e)
        {
            updatedBalance = account.Balance - (item.Amount / 100).ToCentsFromScaled(); // Fixed: Proper scaling
            BcrLog.Slack($"TradeAccountUpdateBalanceAndLeverageAsync_tid_{_tenantId}_aid_{item.TargetAccountId}_error_{e.Message}");
        }

        var accountLog = new AccountLog
        {
            AccountId = account.Id,
            OperatorPartyId = operatorPartyId,
            Action = $"TransferFromWallet:{item.SourceAccountId}",
            Before = $"Balance: {account.Balance}",
            After = $"Balance: {updatedBalance}",
            CreatedOn = DateTime.UtcNow,
        };
        tenantCtx.AccountLogs.Add(accountLog);
    }

    /// <summary>
    /// Transfer from wallet to account with currency conversion support
    /// </summary>
    private async Task TransferFromWalletToAccountWithConversionAsync(Transaction item, long walletAmount, long accountAmount, long transactionAmount, long operatorPartyId = 1)
    {
        var account = await tenantCtx.Accounts
            .Where(x => x.Id == item.TargetAccountId)
            .Select(x => new { x.Id, x.ServiceId, x.AccountNumber, x.Status, x.TradeAccountStatus!.Balance })
            .SingleOrDefaultAsync();
        if (account == null) throw new ProcessMatterException("Account not found");
        if (account.Status != 0) throw new ProcessMatterException("Account Status not valid");

        var amountForAccount = (decimal)transactionAmount / 100; // Moving divided by 10000 inside apiService.ChangeBalance()

        // Use wallet amount for wallet balance change - wallet amount should be in original currency scale
        var amountOutCentsFromWallet = -1 * walletAmount;
        await WalletChangeBalanceRawAsync(item.SourceAccountId, item.Id, amountOutCentsFromWallet);

        var (result, ticket) = await apiService.ChangeBalance(account.ServiceId, account.AccountNumber, amountForAccount,
            $"Deposit TF from Wallet (C)");
        if (!result) throw new ProcessMatterException("Deposit TF from wallet failed");

        item.ReferenceNumber = ticket;
        tenantCtx.Entry(item).Property(y => y.ReferenceNumber).IsModified = true;

        double updatedBalance;
        try
        {
            var (res, _, balance) = await TradeAccountUpdateBalanceAndLeverageAsync(item.TargetAccountId);
            updatedBalance = balance;
            if (!res) return;
        }
        catch (Exception e)
        {
            updatedBalance = account.Balance - (accountAmount / 100).ToCentsFromScaled();
            BcrLog.Slack($"TradeAccountUpdateBalanceAndLeverageAsync_tid_{_tenantId}_aid_{item.TargetAccountId}_error_{e.Message}");
        }

        var accountLog = new AccountLog
        {
            AccountId = account.Id,
            OperatorPartyId = operatorPartyId,
            Action = $"TransferFromWallet:{item.SourceAccountId} (Conversion: {walletAmount} -> {accountAmount})",
            Before = $"Balance: {account.Balance}",
            After = $"Balance: {updatedBalance}",
            CreatedOn = DateTime.UtcNow,
        };
        tenantCtx.AccountLogs.Add(accountLog);
    }

    private async Task TransferBetweenWalletAsync(Transaction item, long operatorPartyId = 1)
    {
        await WalletChangeBalanceRawAsync(item.SourceAccountId, item.Id, -1 * item.Amount);
        await WalletChangeBalanceRawAsync(item.TargetAccountId, item.Id, item.Amount);
    }

    public async Task<List<Transaction.ClientPageModel>> QueryForTransactionClientAsync(Transaction.ClientCriteria criteria)
    {
        // *** remove Currency filter coz wallet now support multiple currencies *** //
        criteria.CurrencyId = null;
        // filter out transfering Rewards to downline account's wallet
        var items = await tenantCtx.Transactions.Where(x => x.TargetAccountType != (int)TransactionAccountTypes.Wallet).PagedFilterBy(criteria).ToClientPageModel().ToListAsync();
        var accountIds = items
            .Where(x => x.SourceType == TransactionAccountTypes.Account)
            .Select(x => x.SourceId)
            .Union(items.Where(y => y.TargetType == TransactionAccountTypes.Account).Select(x => x.TargetId))
            .Distinct()
            .ToList();

        var accounts = await tenantCtx.Accounts
            .Where(x => accountIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.AccountNumber);

        foreach (var item in items)
        {
            if (item.SourceType == TransactionAccountTypes.Account)
            {
                item.SourceAccountNumber = accounts.GetValueOrDefault(item.SourceId, 0);
            }

            if (item.TargetType == TransactionAccountTypes.Account)
            {
                item.TargetAccountNumber = accounts.GetValueOrDefault(item.TargetId, 0);
            }
        }

        return items;
    }
}