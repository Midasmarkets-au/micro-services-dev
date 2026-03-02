using Bacera.Gateway.MyException;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services.Acct;

public partial class AcctService
{
    public Task<(bool, string)> DepositExecutePaymentAsync(long depositId, long operatorPartyId = 1, string note = "")
        => ProcessMatterAsync(depositId, async () =>
        {
            var item = await tenantCtx.Deposits
                .Include(x => x.IdNavigation)
                .Include(x => x.Payment)
                .ThenInclude(x => x.PaymentMethod)
                .SingleOrDefaultAsync(x => x.Id == depositId);
            // if (item == null) return (false, "Deposit not found");
            if (item == null) throw new ProcessMatterException("Deposit not found");
            if (item.IdNavigation.StateId is not (int)StateTypes.DepositCreated) throw new ProcessMatterException("Deposit not in created state");

            if (item.Payment.Status != (int)PaymentStatusTypes.Pending || item.Payment.PaymentMethod.Platform != (int)PaymentPlatformTypes.Wire)
                return;

            item.Payment.Status = (int)PaymentStatusTypes.Executing;
            item.Payment.UpdatedOn = DateTime.UtcNow;
            // note = $"{note}, PaymentStatus: {Enum.GetName((PaymentStatusTypes)item.Payment.Status)}";
            // TransitRaw(item, StateTypes.DepositPaymentCompleted, operatorPartyId, note);
        });
    
    public Task<(bool, string)> DepositCompleteAsync(long depositId, long operatorPartyId = 1, string note = "")
        => DepositToTradeAccountAsync(depositId, StateTypes.DepositCompleted, operatorPartyId, note);

    public Task<(bool, string)> DepositCallbackCompleteAsync(long depositId, long operatorPartyId = 1, string note = "")
        => DepositToTradeAccountAsync(depositId, StateTypes.DepositCallbackCompleted, operatorPartyId, note);

    public Task<(bool, string)> DepositCompletePaymentAsync(long depositId, long operatorPartyId = 1, string note = "")
        => ProcessMatterAsync(depositId, async () =>
        {
            var item = await tenantCtx.Deposits
                .Include(x => x.IdNavigation)
                .Include(x => x.Payment)
                .SingleOrDefaultAsync(x => x.Id == depositId);
            // if (item == null) return (false, "Deposit not found");
            if (item == null) throw new ProcessMatterException("Deposit not found");

            if (item.IdNavigation.StateId is not (int)StateTypes.DepositCreated)
                throw new ProcessMatterException("Deposit not in created state");

            if (item.Payment.Status != (int)PaymentStatusTypes.Executing)
                throw new ProcessMatterException("Payment not executing");

            item.Payment.Status = (int)PaymentStatusTypes.Completed;
            item.Payment.UpdatedOn = DateTime.UtcNow;

            note = $"{note}, PaymentStatus: {Enum.GetName((PaymentStatusTypes)item.Payment.Status)}";
            TransitRaw(item, StateTypes.DepositPaymentCompleted, operatorPartyId, note);
        });

    public Task<(bool, string)> DepositCompleteFromCallbackAsync(long depositId, long operatorPartyId = 1, string note = "")
        => ProcessMatterAsync(depositId, async () =>
        {
            var item = await tenantCtx.Deposits
                .Include(x => x.IdNavigation)
                .Include(x => x.Payment)
                .SingleOrDefaultAsync(x => x.Id == depositId);
            if (item == null) throw new ProcessMatterException("Deposit not found");

            if (item.IdNavigation.StateId is not (int)StateTypes.DepositCallbackCompleted)
                throw new ProcessMatterException("Deposit not in callback completed state");

            note = $"{note}, PaymentStatus: {Enum.GetName((PaymentStatusTypes)item.Payment.Status)}";
            TransitRaw(item, StateTypes.DepositCompleted, operatorPartyId, note);
        });

    public Task<(bool, string)> DepositCancelAsync(long depositId, long operatorPartyId = 1, string note = "")
        => ProcessMatterAsync(depositId, async () =>
        {
            var item = await tenantCtx.Deposits
                .Include(x => x.IdNavigation)
                .Include(x => x.Payment)
                .SingleOrDefaultAsync(x => x.Id == depositId);
            if (item == null) throw new ProcessMatterException("Deposit not found");

            if (item.IdNavigation.StateId is (int)StateTypes.DepositCompleted or (int)StateTypes.DepositCallbackCompleted)
                throw new ProcessMatterException("Deposit already completed");

            note = $"{note}, PaymentStatus: {Enum.GetName((PaymentStatusTypes)item.Payment.Status)}";
            TransitRaw(item, StateTypes.DepositCanceled, operatorPartyId, note);
        });

    public Task<(bool, string)> DepositRestoreToInitialAsync(long depositId, long operatorPartyId = 1, string note = "")
        => ProcessMatterAsync(depositId, async () =>
        {
            var item = await tenantCtx.Deposits
                .Include(x => x.IdNavigation)
                .Include(x => x.Payment)
                .SingleOrDefaultAsync(x => x.Id == depositId);
            if (item == null) throw new ProcessMatterException("Deposit not found");

            note = $"{note}, PaymentStatus: {Enum.GetName((PaymentStatusTypes)item.Payment.Status)}";
            TransitRaw(item, StateTypes.DepositCreated, operatorPartyId, note);
            item.Payment.Status = (int)PaymentStatusTypes.Executing;
            item.Payment.UpdatedOn = DateTime.UtcNow;
        });

    public Task<(bool, string)> DepositCallbackTimeoutAsync(long depositId, long operatorPartyId = 1, string note = "")
        => ProcessMatterAsync(depositId, async () =>
        {
            var item = await tenantCtx.Deposits
                .Include(x => x.IdNavigation)
                .Include(x => x.Payment)
                .SingleOrDefaultAsync(x => x.Id == depositId);
            if (item == null) throw new ProcessMatterException("Deposit not found");

            note = $"{note}, PaymentStatus: {Enum.GetName((PaymentStatusTypes)item.Payment.Status)}";
            TransitRaw(item, StateTypes.DepositCallbackTimeOut, operatorPartyId, note);
        });

    private Task<(bool, string)> DepositToTradeAccountAsync(long depositId, StateTypes finalState, long operatorPartyId, string note)
        => ProcessMatterAsync(depositId, async () =>
        {
            var item = await tenantCtx.Deposits
                .Include(x => x.IdNavigation)
                .Include(x => x.Payment)
                .SingleOrDefaultAsync(x => x.Id == depositId);
            if (item == null) throw new ProcessMatterException("Deposit not found");

            if (item.IdNavigation.StateId is (int)StateTypes.DepositCompleted or (int)StateTypes.DepositCallbackCompleted)
                throw new ProcessMatterException("Deposit already completed");

            if (item.Payment.Status != (int)PaymentStatusTypes.Completed)
                throw new ProcessMatterException("Payment not completed");

            var account = await tenantCtx.Accounts
                .Where(x => x.Id == item.TargetAccountId)
                .Select(x => new { x.Id, x.PartyId, x.ServiceId, x.AccountNumber, x.ActiveOn })
                .SingleOrDefaultAsync();

            if (account == null) throw new ProcessMatterException("Account not found");

            if (!pool.IsServiceExisted(account.ServiceId))
                throw new ProcessMatterException("Service not found");

            if (!string.IsNullOrWhiteSpace(item.ReferenceNumber))
            {
                var isDepositAlreadyAdded = await CheckIfDepositAlreadyAddedAsync(account.ServiceId, account.AccountNumber, item.ReferenceNumber);
                if (isDepositAlreadyAdded) throw new ProcessMatterException("Deposit already added");
            }

            TransitRaw(item, finalState, operatorPartyId, note);

            var amountForAccount = (decimal)item.Amount / 100;
            var comment = await GetMtCommentForDeposit(account.Id, item.PaymentId);

            var (balanceChanged, refNumber) = await apiService.ChangeBalance(account.ServiceId, account.AccountNumber, amountForAccount, comment);
            if (!balanceChanged) throw new ProcessMatterException("Failed to change balance");

            item.ReferenceNumber = refNumber;
        });

    private async Task<string> GetMtCommentForDeposit(long accountId, long paymentId)
    {
        var payment = await tenantCtx.Payments
            .Include(x => x.PaymentMethod)
            .Select(x => new { 
                x.Id, x.PaymentServiceId, 
                x.ReferenceNumber, 
                x.PaymentMethod.CommentCode,
                x.PaymentMethod.Platform  // Add this to determine crypto vs traditional
            })
            .SingleAsync(x => x.Id == paymentId);

        var accountHasDeposit = await tenantCtx.Deposits
            .Where(x => x.IdNavigation.StateId == (int)StateTypes.DepositCompleted ||
                        x.IdNavigation.StateId == (int)StateTypes.DepositCallbackCompleted)
            .Where(x => x.TargetAccountId == accountId)
            .Where(x => x.Id != paymentId)
            .AnyAsync();

        var prefix = accountHasDeposit ? "Deposit add" : "Deposit new";
        var refNumber = Utils.GetLastChars(payment.Id.ToString());

        // Determine if this is a crypto payment (UT) or traditional payment (MT)
        var isCrypto = payment.Platform == (int)PaymentPlatformTypes.Crypto;
                       //payment.Platform == (int)PaymentPlatformTypes.UsdtTrc20 ||
                       //payment.Platform == (int)PaymentPlatformTypes.UsdtErc20;

        var platformCode = isCrypto ? "UT" : "MT";  // UT = USDT/Crypto, MT = MetaTrader/Traditional

        // For USDT Crypto (PaymentServiceId = 10055), extract wallet number from ReferenceNumber
        // ¸Ä³É Deposit new UT10.XXXX »ò Deposit new MT10.XXXX, UT/MT=USDT
        string walletSuffix = string.Empty;
        if (payment.PaymentServiceId == 10055 && !string.IsNullOrEmpty(payment.ReferenceNumber))
        {
            // Extract number after "#" from ReferenceNumber (e.g., "USDT Hot Wallet #2" -> "2")
            var hashIndex = payment.ReferenceNumber.IndexOf('#');
            if (hashIndex >= 0 && hashIndex < payment.ReferenceNumber.Length - 1)
            {
                var walletNumber = payment.ReferenceNumber.Substring(hashIndex + 1).Trim();
                // Format as UT{number}/MT{number} (e.g., UT2/MT2)
                if (!string.IsNullOrEmpty(walletNumber))
                {
                    walletSuffix = $" {platformCode}{walletNumber}";
                    return $"{prefix} {walletSuffix}.{refNumber}";
                }
            }
        }

        if (!string.IsNullOrEmpty(payment.CommentCode)) 
            return $"{prefix} {payment.CommentCode}.{refNumber}";
        
        return prefix + " margin in" + walletSuffix;
    }

    private async Task<bool> CheckIfDepositAlreadyAddedAsync(int serviceId, long accountNumber, string referenceNumber)
    {
        var platform = pool.GetPlatformByServiceId(serviceId);
        switch (platform)
        {
            case PlatformTypes.MetaTrader4:
            {
                var reference = long.Parse(referenceNumber);
                await using var mt4Ctx = pool.CreateCentralMT4DbContextAsync(serviceId);
                var exist = await mt4Ctx.Mt4Trades
                    .Where(x => x.Login == accountNumber)
                    .Where(x => x.Cmd == 6 && x.Ticket == reference)
                    .AnyAsync();
                return exist;
            }
            case PlatformTypes.MetaTrader5:
            {
                var reference = ulong.Parse(referenceNumber);
                await using var mt5Ctx = pool.CreateCentralMT5DbContextAsync(serviceId);
                var exist = await mt5Ctx.Mt5Deals2025s
                    .Where(x => x.Login == (ulong)accountNumber)
                    .Where(x => x.Action == 2 && x.Deal == reference)
                    .AnyAsync();
                return exist;
            }
            case PlatformTypes.MetaTrader4Demo:
            case PlatformTypes.MetaTrader5Demo:
            case PlatformTypes.CTrader:
            default:
                return false;
        }
    }
}