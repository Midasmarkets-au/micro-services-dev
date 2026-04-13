
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Services.Withdraw;
using Bacera.Gateway.Vendor.Pay247;
using Bacera.Gateway.ViewModels.Tenant;
using Bacera.Gateway.Web.EventHandlers;
using Grpc.Core;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Withdrawal;
using MSG = ResultMessage.Withdrawal;

[Tags("Tenant/Withdrawal")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class WithdrawalController(
    ITenantGetter tenantGetter,
    AccountingService accountingService,
    IMediator mediator,
    TradingService tradingService,
    TenantDbContext tenantCtx,
    AcctService acctSvc,
    ConfigService cfgSvc,
    WithdrawalService withdrawalSvc,
    ILogger<WithdrawalController> logger)
    : TenantBaseController
{
    private readonly long _tenantId = tenantGetter.GetTenantId();

    /// <summary>
    /// Withdrawal Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<WithdrawalViewModel, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var hideEmail = ShouldHideEmail();
        var result = await accountingService.WithdrawalQueryForTenantAsync(criteria, hideEmail);
        return Ok(result);
    }

    /// <summary>
    /// Get Withdrawal
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var result = await accountingService.WithdrawalGetAsync(id);
        return result.IsEmpty()
            ? NotFound()
            : Ok(result);
    }

    /// <summary>
    /// Create Withdrawal
    /// </summary>
    /// <remarks>
    /// 1. Check if the payment service is available for current user
    /// 2. Create Withdrawal
    /// 3. Create Payment if Request provided correctly
    /// </remarks>
    /// <param name="spec"></param>
    /// <returns>Use PaymentId direct to payment</returns>
    [HttpPost]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<M.ClientResponseModel>> Create([FromBody] Withdrawal.CreateByTenantSpec spec)
    {
        // model validation
        var specValidator = new WithdrawalCreateByTenantSpecValidator();
        var specValidationResult = await specValidator.ValidateAsync(spec);

        if (!specValidationResult.IsValid)
            return BadRequest(Result.Error(MSG.InvalidParameters, specValidationResult));

        var paymentService = await GetPaymentServiceForWithdrawalByTenant(spec.PaymentServiceId);
        if (paymentService == null)
            return BadRequest(Result.Error(ResultMessage.Deposit.PaymentPlatformNotFound));

        var wallet = await accountingService.WalletGetOrCreateAsync(spec.PartyId, spec.CurrencyId, spec.FundType);
        if (wallet.Balance < spec.Amount.ToScaledFromCents())
            return BadRequest(Result.Error(ResultMessage.Transaction.BalanceNotEnough));

        var supplement = Supplement.WithdrawalSupplement.Build(spec.PartyId, spec);
        var withdrawal = await accountingService.WithdrawalCreateAsync(spec.PaymentServiceId, spec.PartyId,
            spec.FundType, spec.CurrencyId, spec.Amount, GetPartyId(), supplement, sourceWalletId: wallet.Id);
        if (withdrawal.IsEmpty())
            return BadRequest(Result.Error(MSG.InvalidParameters));

        await mediator.Publish(new WithdrawalCreatedEvent(withdrawal));

        return Ok(withdrawal.ToClientResponseModel());
    }

    [HttpPost("from-account/{accountUid:long}")]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<M.ClientResponseModel>> CreateForAccount(long accountUid,
        [FromBody] Withdrawal.CreateFromAccountByTenantSpec spec)
    {
        if (accountUid != spec.AccountUid)
            return BadRequest(Result.Error(MSG.InvalidParameters));

        var account = await tenantCtx.Accounts
            .Include(x => x.TradeAccountStatus)
            .Where(x => x.Uid == accountUid)
            .Where(x => x.TradeAccount != null)
            .SingleOrDefaultAsync();

        if (account == null || account.TradeAccountStatus == null)
            return BadRequest(Result.Error(MSG.SourceAccountNotFound));

        // check User payment service access
        var paymentService = await GetPaymentServiceForWithdrawalByTenant(spec.PaymentServiceId);
        if (paymentService == null)
            return BadRequest(Result.Error(ResultMessage.Deposit.TheSpecifiedServiceDoesNotHavePermissionForAccount));

        try
        {
            // var (balanceResult, _, balance) =
            //     await _tradingSvc.GetTradeAccountBalanceAndLeverageFromServer(account.Id);
            // if (!balanceResult)
            //     return BadRequest(Result.Error(MSG.CannotGetBalance));
            var balance = account.TradeAccountStatus?.Equity ?? 0;
            if (balance < (spec.Amount / 100d))
            {
                logger.LogError("BalanceCheck: {accountNumber} {balance} < {amount}", account.AccountNumber, balance,
                    spec.Amount / 100d);
                return BadRequest(Result.Error(ResultMessage.Transaction.BalanceNotEnough));
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Cannot get balance for account {AccountId} from trade service", account.Id);
            return BadRequest(Result.Error(MSG.CannotGetBalance));
        }
        // try
        // {
        //     var (balanceResult, _, balance) = await _tradingSvc.GetTradeAccountBalanceAndLeverageFromServer(account.Id);
        //     if (!balanceResult)
        //         return BadRequest(Result.Error(MSG.CannotGetBalance));

        //     if (balance < (spec.Amount / 100d))
        //         return BadRequest(Result.Error(ResultMessage.Transaction.BalanceNotEnough));
        // }
        // catch (Exception e)
        // {
        //     _logger.LogError(e, "Cannot get balance for account {AccountId} from trade service", account.Id);
        //     return BadRequest(Result.Error(MSG.CannotGetBalance));
        // }

        var supplement = Supplement.WithdrawalSupplement.Build(spec.PartyId, spec);
        var withdrawal = await accountingService.WithdrawalCreateAsync(spec.PaymentServiceId, spec.PartyId,
            (FundTypes)account.FundType, (CurrencyTypes)account.CurrencyId, spec.Amount.ToScaledFromCents(), GetPartyId(), supplement,
            sourceTradeAccountId: account.Id);
        if (withdrawal.IsEmpty())
            return BadRequest(Result.Error(MSG.InvalidParameters));

        await mediator.Publish(new WithdrawalCreatedEvent(withdrawal));

        return Ok(withdrawal.ToClientResponseModel());
    }

    /// <summary>
    /// Cancel Withdrawal by Tenant Admin
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(long id)
    {
        var (result, message) = await acctSvc.CancelWithdrawalAsync(id, GetPartyId());
        return result ? NoContent() : BadRequest(Result.Error(message));
    }

    /// <summary>
    /// Approve and Complete Withdrawal by Tenant Admin
    /// Action: Approve (1420)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(long id, [FromBody] M.ApproveSpec spec)
    {
        var (result, message) = await acctSvc.ApproveWithdrawalAsync(id, GetPartyId(), spec.Comment);
        if (!result) return BadRequest(Result.Error(message));
        return NoContent();
    }

    /// <summary>
    /// Reject Withdrawal by Tenant Admin
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(long id, [FromBody] Withdrawal.RejectSpec spec)
    {
        if (id != spec.Id)
            return BadRequest(Result.Error(MSG.InvalidParameters));

        var item = await accountingService.WithdrawalGetAsync(id);
        if (item.IsEmpty())
            return NotFound();
        var legacyStateId = item.IdNavigation.StateId;

        var canTransit = await accountingService.CanTransitToStateAsync(item, ActionTypes.WithdrawalTenantReject,
            StateTypes.WithdrawalTenantRejected, GetPartyId());
        if (!canTransit)
            return BadRequest(Result.Error(ResultMessage.Common.ActionNotAllow));

        var result = await accountingService.WithdrawalRejectByTenantAsync(item, GetPartyId());
        if (!result)
            return BadRequest(Result.Error(MSG.RejectFailed));

        var comment = new Comment
        {
            Content = spec.Reason,
            Type = (short)CommentTypes.Withdrawal,
            RowId = id,
            PartyId = GetPartyId()
        };
        tenantCtx.Comments.Add(comment);
        await tenantCtx.SaveChangesAsync();


        await mediator.Publish(new WithdrawalRejectedEvent(item));
        return NoContent();
    }

    /// <summary>
    /// Try to complete payment for Withdrawal by Tenant Admin
    /// Action: WithdrawalExecutePayment (1430)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/complete-payment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompletePayment(long id)
    {
        var item = await accountingService.WithdrawalGetAsync(id);
        if (item.IsEmpty())
            return NotFound();

        var canTransit = await accountingService.CanTransitToStateAsync(item, ActionTypes.WithdrawalExecutePayment,
            StateTypes.WithdrawalPaymentCompleted, GetPartyId());
        if (!canTransit)
            return BadRequest(Result.Error(ResultMessage.Common.ActionNotAllow));

        var result = await accountingService.WithdrawalTryCompletePaymentAsync(id, GetPartyId());
        if (!result) return BadRequest(Result.Error(MSG.CompleteFailed));

        return await Complete(id);
    }

    /// <summary>
    /// Complete Withdrawal by Tenant Admin
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(long id)
    {
        var item = await accountingService.WithdrawalGetAsync(id);
        if (item.IsEmpty())
            return NotFound();

        var canTransit = await accountingService.CanTransitToStateAsync(item, ActionTypes.WithdrawalComplete,
            StateTypes.WithdrawalCompleted, GetPartyId());
        if (!canTransit)
            return BadRequest(Result.Error(ResultMessage.Common.ActionNotAllow));

        var result = await accountingService.WithdrawalCompleteAsync(item, GetPartyId());
        if (!result)
            return BadRequest(Result.Error(MSG.CompleteFailed));

        await mediator.Publish(new WithdrawalCompletedEvent(item));

        return NoContent();
    }

    /// <summary>
    /// Restore Withdrawal
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("cancel/{id:long}/restore")]
    [HttpPut("reject/{id:long}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    public async Task<IActionResult> CancelAndRejectRestore(long id)
    {
        var (res, msg) = await acctSvc.WithdrawalRestoreToInitialAsync(id, GetPartyId(), "Restore by tenant");
        return !res ? BadRequest(Result.Error(msg)) : NoContent();
    }

    /// <summary>
    /// Get withdrawal info for withdrawal
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetInfo(long id)
    {
        var supplement = await tenantCtx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.Withdraw)
            .Where(x => x.RowId == id)
            .FirstOrDefaultAsync();
        var info = JsonConvert.DeserializeObject(supplement?.Data ?? "{}");
        return Ok(info);
    }

    /// <summary>
    /// Refund Withdrawal by Tenant Admin
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/refund")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Refund(long id)
    {
        var item = await accountingService.WithdrawalGetAsync(id);
        if (item.IsEmpty())
            return NotFound();

        var canTransit = await accountingService.CanTransitToStateAsync(item, ActionTypes.WithdrawalTenantRefund,
            StateTypes.WithdrawalCreated, GetPartyId());
        if (!canTransit)
            return BadRequest(Result.Error(ResultMessage.Common.ActionNotAllow));

        var comment = "Withdraw Refund - PID: " + item.PaymentId;
        var amount = Math.Abs(Decimal.Divide(item.Amount, 100));

        if (item.SourceAccountId != null) // refund trade account
        {
            string ticket;
            try
            {
                (var success, ticket) =
                    await tradingService.TradeAccountChangeBalanceAndUpdateStatus((long)item.SourceAccountId, amount,
                        comment);
                if (!success)
                    return BadRequest(Result.Error(ResultMessage.Account.TradeServerError));
            }
            catch
            {
                return BadRequest(Result.Error(ResultMessage.Account.TradeServerError));
            }

            //Audit
            var data = JsonConvert.SerializeObject(new
            {
                AccountId = item.SourceAccountId,
                AdjustType = "Balance",
                Amount = amount,
                Comment = comment,
                Ticket = ticket,
            });
            var audit = Audit.Build(AuditTypes.TradeAccountBalance, AuditActionTypes.Update, GetPartyId(),
                (long)item.SourceAccountId,
                data, System.Net.Dns.GetHostName());
            await tenantCtx.Audits.AddAsync(audit);
            await tenantCtx.SaveChangesAsync();
        }
        else // refund wallet
        {
            var wallet = await accountingService.WalletGetOrCreateAsync(item.PartyId,
                (CurrencyTypes)item.CurrencyId,
                (FundTypes)item.FundType);

            var spec = new Refund.CreateSpec
            {
                TargetId = wallet.Id,
                TargetType = RefundTargetTypes.Wallet,
                Amount = item.Amount,
                Comment = comment,
            };

            var refund = await accountingService.RefundCreateAsync(spec, GetPartyId());
            if (refund.IsEmpty()) return BadRequest(ToErrorResult(ResultMessage.Refund.CreateFailed));

            var result = await accountingService.RefundCompleteAsync(refund.Id, GetPartyId());
            if (result == false) return BadRequest(ToErrorResult(MSG.CompleteFailed));

            //Audit
            var data = JsonConvert.SerializeObject(new
            {
                WalletId = wallet.Id,
                Amount = amount,
                Comment = comment,
            });
            var audit = Audit.Build(AuditTypes.Wallet, AuditActionTypes.Update, GetPartyId(), wallet.Id,
                data, System.Net.Dns.GetHostName());
            await tenantCtx.Audits.AddAsync(audit);
            await tenantCtx.SaveChangesAsync();
        }

        var statusUpdate = await accountingService.WithdrawalRefundByTenantAsync(item, GetPartyId());
        if (!statusUpdate)
            return BadRequest(Result.Error(MSG.RefundFailed));

        return Ok();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("ofa-df")]
    public async Task<IActionResult> CreateOfaDf([FromBody] M.IdSpec spec)
    {
        // if (_tenantId != 10000) return BadRequest(Result.Error(ResultMessage.Common.ActionNotAllow));
        var result = await withdrawalSvc.CreateOfaDfByWithdrawalIdAsync(spec.Id);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    private async Task<PaymentService?> GetPaymentServiceForWithdrawalByTenant(long paymentServiceId) =>
        await tenantCtx.PaymentServices
            .Where(x => x.CanWithdraw == 1)
            .Where(x => x.Id == paymentServiceId)
            .Where(x => x.IsActivated == 1)
            .SingleOrDefaultAsync();

    /// <summary>
    /// Get ExLink payout bank list for a specific currency
    /// </summary>
    /// <param name="currency">Currency code (e.g., VND, THB)</param>
    /// <returns>List of supported banks</returns>
    [HttpGet("exlink/banks")]
    [ProducesResponseType(typeof(Vendor.ExLinkCashier.ExLinkCashier.BankListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExLinkBankList(
        [FromQuery] string currency)
    {
        if (string.IsNullOrEmpty(currency))
            return BadRequest(Result.Error("Currency is required"));

        // Get payment method configuration
        var paymentMethod = await tenantCtx.PaymentMethods
            .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
            .Where(x => x.DeletedOn == null && x.IsDeleted == false)
            .Where(x => x.MethodType == PaymentMethodTypes.Withdrawal)
            .FirstOrDefaultAsync();

        if (paymentMethod == null)
            return BadRequest(Result.Error("Payment method not found"));

        try
        {
            var options = Vendor.ExLinkCashier.ExLinkCashierOptions.FromJson(paymentMethod.Configuration);
            var httpClient = new HttpClient();

            var result = await Vendor.ExLinkCashier.ExLinkCashier.QueryPayoutBankListAsync(
                options.Uid,
                options.SecretKey,
                currency,
                options.PaymentType,
                httpClient,
                logger
            );

            if (result == null)
                return BadRequest(Result.Error("Failed to retrieve bank list from ExLink"));

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query ExLink bank list for currency {Currency}", currency);
            return BadRequest(Result.Error($"Failed to query bank list: {ex.Message}"));
        }
    }

    /// <summary>
    /// Query ExLink withdrawal order status
    /// </summary>
    /// <param name="paymentNumber">Merchant order number</param>
    /// <returns>Withdrawal order status details</returns>
    [HttpGet("exlink/payouts/{paymentNumber}/status")]
    [ProducesResponseType(typeof(Vendor.ExLinkCashier.ExLinkCashier.WithdrawalStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExLinkPayoutOrderStatusForWithdrawal(string paymentNumber)
    {
        if (string.IsNullOrEmpty(paymentNumber))
            return BadRequest(Result.Error("Merchant order number is required"));

        // Get payment method configuration
        var paymentMethod = await tenantCtx.PaymentMethods
            .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
            .Where(x => x.DeletedOn == null && x.IsDeleted == false)
            .Where(x => x.MethodType == PaymentMethodTypes.Withdrawal)
            .FirstOrDefaultAsync();

        if (paymentMethod == null)
            return BadRequest(Result.Error("Payment method not found"));

        try
        {
            var options = Vendor.ExLinkCashier.ExLinkCashierOptions.FromJson(paymentMethod.Configuration);
            var httpClient = new HttpClient();

            var result = await Vendor.ExLinkCashier.ExLinkCashier.QueryWithdrawalStatusAsync(
                options.Uid,
                options.SecretKey,
                paymentNumber,
                httpClient,
                logger
            );

            if (result == null)
                return BadRequest(Result.Error("Failed to retrieve withdrawal status from ExLink"));

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query ExLink withdrawal status for order {MerchantOrderNo}", paymentNumber);
            return BadRequest(Result.Error($"Failed to query withdrawal status: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get ExLink payout payment types for a specific currency
    /// </summary>
    /// <param name="currency">Currency code (e.g., VND, THB)</param>
    /// <returns>List of available payment types for withdrawals</returns>
    [HttpGet("exlink/payment-types")]
    [ProducesResponseType(typeof(Vendor.ExLinkCashier.ExLinkCashier.PaymentTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExLinkPayoutPaymentTypes([FromQuery] string currency)
    {
        if (string.IsNullOrEmpty(currency))
            return BadRequest(Result.Error("Currency is required"));

        // Get payment method configuration
        var paymentMethod = await tenantCtx.PaymentMethods
            .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
            .Where(x => x.DeletedOn == null && x.IsDeleted == false)
            .Where(x => x.MethodType == PaymentMethodTypes.Withdrawal)
            .FirstOrDefaultAsync();

        if (paymentMethod == null)
            return BadRequest(Result.Error("Payment method not found"));

        try
        {
            var options = Vendor.ExLinkCashier.ExLinkCashierOptions.FromJson(paymentMethod.Configuration);
            var httpClient = new HttpClient();

            var result = await Vendor.ExLinkCashier.ExLinkCashier.QueryPayoutPaymentTypesAsync(
                options.Uid,
                options.SecretKey,
                currency,
                httpClient,
                logger
            );

            if (result == null)
                return BadRequest(Result.Error("Failed to retrieve payment types from ExLink"));

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query ExLink payout payment types for currency {Currency}", currency);
            return BadRequest(Result.Error($"Failed to query payment types: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get ExLink account balance
    /// </summary>
    /// <returns>Account balance for all currencies</returns>
    [HttpGet("exlink/balance")]
    [ProducesResponseType(typeof(Vendor.ExLinkCashier.ExLinkCashier.AccountBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExLinkAccountBalance()
    {
        // Get payment method configuration
        var paymentMethod = await tenantCtx.PaymentMethods
            .Where(x => x.Platform == (int)PaymentPlatformTypes.ExLinkGlobal)
            .Where(x => x.DeletedOn == null && x.IsDeleted == false)
            .Where(x => x.MethodType == PaymentMethodTypes.Withdrawal)
            .FirstOrDefaultAsync();

        if (paymentMethod == null)
            return BadRequest(Result.Error("Payment method not found"));

        try
        {
            var options = Vendor.ExLinkCashier.ExLinkCashierOptions.FromJson(paymentMethod.Configuration);
            var httpClient = new HttpClient();

            var result = await Vendor.ExLinkCashier.ExLinkCashier.QueryAccountBalanceAsync(
                options.Uid,
                options.SecretKey,
                httpClient,
                logger
            );

            if (result == null)
                return BadRequest(Result.Error("Failed to retrieve account balance from ExLink"));

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query ExLink account balance");
            return BadRequest(Result.Error($"Failed to query account balance: {ex.Message}"));
        }
    }
}