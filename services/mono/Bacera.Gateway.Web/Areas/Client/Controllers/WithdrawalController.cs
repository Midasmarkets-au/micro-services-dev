using OpenIddict.Validation.AspNetCore;
using Amazon.Util.Internal.PlatformServices;
using Bacera.Gateway.Context;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.EventHandlers;
using Bacera.Gateway.Web.Request;
using Bacera.Gateway.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

using M = Withdrawal;
using MSG = ResultMessage.Withdrawal;

[Tags("Client/Withdrawal")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class WithdrawalController(
    IMediator mediator,
    TenantDbContext tenantDbContext,
    AccountingService accountingService,
    MyDbContextPool pool,
    ITradingApiService tradingApiSvc)
    : ClientBaseController
{
    /// <summary>
    /// Withdrawal Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ClientResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<M.ClientResponseModel>, M.Criteria>>> Index(
        [FromQuery] M.Criteria? criteria)
    {
        var pid = GetPartyId();
        criteria ??= new M.Criteria();
        return Ok(await accountingService.WithdrawalQueryForClientAsync(pid, criteria));
    }

    /// <summary>
    /// Get Withdrawal by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), 200)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M>> Get(long id)
    {
        var result = await accountingService.WithdrawalGetForPartyAsync(id, GetPartyId());
        return result.Id == 0 ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Cancel Withdrawal
    /// </summary>
    /// <param name="hashId"></param>
    /// <returns></returns>
    [HttpPut("{hashId}/cancel")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<M.ClientResponseModel>> Cancel(string hashId)
    {
        var pid = GetPartyId();
        var id = M.HashDecode(hashId);
        var withdrawal = await tenantDbContext.Withdrawals.SingleOrDefaultAsync(x => x.Id == id && x.PartyId == pid);

        if (withdrawal == null)
            return NotFound();

        var canTransit = await accountingService.CanTransitToStateAsync(withdrawal, ActionTypes.WithdrawalCancel,
            StateTypes.WithdrawalCanceled, pid);
        if (!canTransit)
            return BadRequest(Result.Error(ResultMessage.Common.ActionNotAllow));

        var result = await accountingService.WithdrawalTryCancelAsync(id, pid);
        if (!result)
            return BadRequest(Result.Error(MSG.CancelFailed));

        await mediator.Publish(new WithdrawalCanceledEvent(withdrawal));
        return NoContent();
    }

    // /// <summary>
    // /// Create Withdrawal
    // /// </summary>
    // /// <remarks>
    // /// 1. Check if the payment service is available for current user
    // /// 2. Create Withdrawal
    // /// 3. Create Payment if Request provided correctly
    // /// </remarks>
    // /// <param name="spec"></param>
    // /// <returns>Use PaymentId direct to payment</returns>
    // [HttpPost]
    // [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<IActionResult> Create([FromBody] WithdrawalCreateRequestModel spec)
    // {
    //     // model validation
    //     var specValidator = new WithdrawalCreateRequestModelValidator();
    //     var specValidationResult = await specValidator.ValidateAsync(spec);
    //
    //     if (!specValidationResult.IsValid)
    //         return BadRequest(Result.Error(MSG.InvalidParameters, specValidationResult));
    //
    //     var pid = GetPartyId();
    //     var wallet = await accountingService.WalletGetOrCreateAsync(pid, spec.CurrencyId, spec.FundType);
    //     if (wallet.Balance < spec.Amount)
    //         return BadRequest(Result.Error(ResultMessage.Transaction.BalanceNotEnough));
    //
    //     var paymentService =
    //         await GetPaymentServiceForWithdrawal(pid, spec.PaymentServiceId, spec.FundType, spec.CurrencyId);
    //     if (paymentService == null)
    //         return BadRequest(Result.Error(ResultMessage.Deposit.TheSpecifiedServiceDoesNotHavePermissionForAccount));
    //
    //     if (paymentService.Platform == (short)PaymentPlatformTypes.USDT)
    //     {
    //         try
    //         {
    //             var partyId = GetPartyId();
    //             string walletAddress = spec.Request["walletAddress"];
    //             var addressValid = await tenantDbContext.PaymentInfos
    //                 .Where(x => x.PartyId == partyId)
    //                 .Where(x => x.PaymentPlatform == (int)PaymentPlatformTypes.USDT)
    //                 .Where(x => x.Info.Contains(walletAddress))
    //                 .AnyAsync();
    //             if (!addressValid) return BadRequest(Result.Error("Invalid wallet address"));
    //         }
    //         catch
    //         {
    //             return BadRequest(Result.Error("Validate wallet address failed"));
    //         }
    //     }
    //
    //     var supplement = Supplement.WithdrawalSupplement.Build(pid, spec);
    //
    //     var withdrawal = await accountingService.WithdrawalCreateAsync(spec.PaymentServiceId, pid,
    //         spec.FundType, spec.CurrencyId, spec.Amount, pid, supplement, sourceWalletId: wallet.Id);
    //     if (withdrawal.IsEmpty())
    //         return BadRequest(Result.Error(MSG.InvalidParameters));
    //
    //     await mediator.Publish(new WithdrawalCreatedEvent(withdrawal));
    //
    //     return Ok(withdrawal.ToClientResponseModel());
    // }

    // [HttpPost("from-account/{accountUid:long}")]
    // [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<ActionResult<M.ClientResponseModel>> CreateForAccount(
    //     [FromBody] WithdrawalFromAccountCreateRequestModel spec, long accountUid)
    // {
    //     // model validation
    //     var specValidator = new WithdrawalFromAccountCreateRequestModelValidator();
    //     var specValidationResult = await specValidator.ValidateAsync(spec);
    //
    //     if (!specValidationResult.IsValid) return BadRequest(Result.Error(MSG.InvalidParameters, specValidationResult));
    //
    //     var partyId = GetPartyId();
    //     var account = await tenantDbContext.Accounts
    //         .Where(x => x.PartyId == partyId && x.Uid == spec.AccountUid)
    //         .Where(x => x.TradeAccount != null && x.TradeAccountStatus != null)
    //         .Select(x => new { x.Id, x.ServiceId, x.AccountNumber, x.FundType, x.CurrencyId, })
    //         .SingleOrDefaultAsync();
    //     if (account == null) return BadRequest(Result.Error(MSG.SourceAccountNotFound));
    //
    //     var tradeAccountStatus = await tenantDbContext.TradeAccountStatuses
    //         .Where(x => x.Id == account.Id)
    //         .SingleOrDefaultAsync();
    //     if (tradeAccountStatus == null) return BadRequest(Result.Error(MSG.SourceAccountNotFound));
    //     // var info = await tradingApiSvc.GetAccountBalanceAndLeverage()
    //
    //     if (pool.GetPlatformByServiceId(account.ServiceId) == PlatformTypes.MetaTrader5)
    //     {
    //         try
    //         {
    //             var info = await tradingApiSvc.GetAccountInfoAsync(account.ServiceId, account.AccountNumber);
    //             info.ApplyToTradeAccountStatus(tradeAccountStatus);
    //             tenantDbContext.TradeAccountStatuses.Update(tradeAccountStatus);
    //             await tenantDbContext.SaveChangesAsync();
    //
    //             if (info.Equity - info.Credit < spec.Amount / 100m)
    //             {
    //                 BcrLog.Slack(
    //                     $"MT5 Balance Error, Equity: {tradeAccountStatus.Equity}, Credit: {tradeAccountStatus.Credit}, Amount: {spec.Amount / 100m}, Account: {account.AccountNumber}");
    //                 return BadRequest(Result.Error(ResultMessage.Transaction.BalanceNotEnough));
    //             }
    //         }
    //         catch
    //         {
    //             return BadRequest(Result.Error("MT Server Error"));
    //         }
    //     }
    //     else if ((decimal)tradeAccountStatus.Equity - (decimal)tradeAccountStatus.Credit < spec.Amount / 100m)
    //     {
    //         BcrLog.Slack(
    //             $"Balance Error, Equity: {tradeAccountStatus.Equity}, Credit: {tradeAccountStatus.Credit}, Amount: {spec.Amount / 100m}, Account: {account.AccountNumber}");
    //         return BadRequest(Result.Error(ResultMessage.Transaction.BalanceNotEnough));
    //     }
    //
    //     // check User payment service access
    //     var paymentService = await GetPaymentServiceForWithdrawal(partyId, spec.PaymentServiceId,
    //         (FundTypes)account.FundType, (CurrencyTypes)account.CurrencyId);
    //     if (paymentService == null)
    //         return BadRequest(Result.Error(ResultMessage.Deposit.TheSpecifiedServiceDoesNotHavePermissionForAccount));
    //
    //     if (paymentService.Platform == (short)PaymentPlatformTypes.USDT)
    //     {
    //         try
    //         {
    //             string walletAddress = spec.Request["walletAddress"];
    //             var addressValid = await tenantDbContext.PaymentInfos
    //                 .Where(x => x.PartyId == partyId)
    //                 .Where(x => x.PaymentPlatform == (int)PaymentPlatformTypes.USDT)
    //                 .Where(x => x.Info.Contains(walletAddress))
    //                 .AnyAsync();
    //             if (!addressValid) return BadRequest(Result.Error("Invalid wallet address"));
    //         }
    //         catch
    //         {
    //             return BadRequest(Result.Error("Validate wallet address failed"));
    //         }
    //     }
    //
    //     var supplement = Supplement.WithdrawalSupplement.Build(partyId, spec);
    //     var withdrawal = await accountingService.WithdrawalCreateAsync(spec.PaymentServiceId, partyId,
    //         (FundTypes)account.FundType, (CurrencyTypes)account.CurrencyId, spec.Amount, partyId, supplement,
    //         sourceTradeAccountId: account.Id);
    //     if (withdrawal.IsEmpty())
    //         return BadRequest(Result.Error(MSG.InvalidParameters));
    //
    //     await mediator.Publish(new WithdrawalCreatedEvent(withdrawal));
    //
    //     return Ok(withdrawal.ToClientResponseModel());
    // }

    /// <summary>
    /// Update pending withdrawal
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<M.ClientResponseModel>> Update(long id, [FromBody] M.UpdateSpec spec)
    {
        var pid = GetPartyId();
        var item = await accountingService.WithdrawalGetForPartyAsync(id, pid);
        if (item.IsEmpty())
            return NotFound();

        if (item.StateId != (int)StateTypes.WithdrawalCreated)
            return BadRequest(Result.Error(MSG.CurrentStateCannotEdit));

        if (item.PaymentStateId != (int)PaymentStatusTypes.Pending)
            return BadRequest(Result.Error(MSG.CurrentPaymentStateCannotEdit));

        var result = await accountingService.WithdrawalUpdateAsync(id, spec, pid);
        return result ? NoContent() : BadRequest(Result.Error(MSG.CannotCancel));
    }


    /// <summary>
    /// Get Withdrawal exchange rate
    /// </summary>
    /// <param name="fromCurrencyId"></param>
    /// <param name="toCurrencyId"></param>
    /// <returns></returns>
    [HttpGet("{fromCurrencyId:int}/to/{toCurrencyId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<M.ClientResponseModel>> ExchangeRate(int fromCurrencyId, int toCurrencyId)
    {
        var (status, rate) = await GetExchangeRate((CurrencyTypes)fromCurrencyId, (CurrencyTypes)toCurrencyId);
        if (!status)
            return BadRequest(Result.Error(ResultMessage.Deposit.ExchangeRateNotExists));
        return Ok(new { rate });
    }

    private async Task<Tuple<bool, decimal>> GetExchangeRate(CurrencyTypes from, CurrencyTypes to)
    {
        if (from == to) return Tuple.Create(true, 1m);
        var exchangeRateEntity = await accountingService.GetExchangeRateAsync(from, to);
        return exchangeRateEntity == null
            ? Tuple.Create(false, 0m)
            : Tuple.Create(true, exchangeRateEntity.BuyingRate);
    }
}