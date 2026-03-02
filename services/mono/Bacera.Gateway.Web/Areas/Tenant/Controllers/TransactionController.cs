using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Acct;
using Bacera.Gateway.ViewModels.Tenant;
using Bacera.Gateway.Web.EventHandlers;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Transaction;
using MSG = ResultMessage.Transaction;

[Tags("Tenant/Transaction")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class TransactionController(
    AccountingService accountingService,
    TradingService tradingService,
    IMediator mediator,
    AcctService acctService,
    ConfigService configSvc,
    TenantDbContext tenantDbContext,
    ILogger<TransactionController> logger)
    : TenantBaseController
{
    /// <summary>
    /// Transaction pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<TransactionViewModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var hideEmail = ShouldHideEmail();
        var result = await accountingService.TransactionQueryForTenantAsync(criteria, hideEmail);
        return Ok(result);
    }

    [HttpGet("auto-complete-setting")]
    public async Task<IActionResult> GetAutoCompleteTransactionSetting()
    {
        var setting = await configSvc.GetAsync<ApplicationConfigure.AutoCompleteTransactionAmountValue>(
            nameof(Public)
            , 0
            , ConfigKeys.AutoCompleteTransactionAmount);

        return Ok(setting);
    }

    [HttpPut("auto-complete-setting")]
    public async Task<IActionResult> UpdateAutoCompleteTransactionSetting([FromBody] ApplicationConfigure.AutoCompleteTransactionAmountValue spec)
    {
        await configSvc.SetAsync(nameof(Public), 0, ConfigKeys.AutoCompleteTransactionAmount, spec);
        return Ok(spec);
    }
    
    /// <summary>
    /// Get Transaction
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(long id)
    {
        var result = await accountingService.TransactionGetAsync(id);
        if (result.Id == 0)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Transfer from Wallet to Trade Account
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("to-trade-account")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferFromWalletToTradeAccount([FromBody] M.RequestModel spec)
    {
        var pid = GetPartyId();
        var (result, transaction) =
            await accountingService.TransactionCreateFromWalletToTradeAccountAsync(spec.PartyId, spec.WalletId,
                spec.TradeAccountId, spec.Amount, pid);
        return result switch
        {
            TransactionCheckStatus.Passed => Ok(transaction),
            TransactionCheckStatus.WalletNotFound => BadRequest(Result.Error(MSG.WalletInvalided)),
            TransactionCheckStatus.TradeAccountNotFound => BadRequest(Result.Error(MSG.TradeAccountInvalided)),
            TransactionCheckStatus.CurrencyNotMatch => BadRequest(Result.Error(MSG.CurrencyNotMatched)),
            TransactionCheckStatus.FundTypeNotMatch => BadRequest(Result.Error(MSG.FundTypeNotMatched)),
            _ => BadRequest(Result.Error(MSG.TransferFailed)),
        };
    }

    /// <summary>
    /// Transfer from Wallet to Trade Account
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("to-wallet")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferFromTradeAccountToWallet([FromBody] M.RequestModel spec)
    {
        var pid = GetPartyId();
        var (result, transaction) =
            await accountingService.TransactionCreateFromTradeAccountToWalletAsync(spec.PartyId, spec.TradeAccountId,
                spec.WalletId, spec.Amount, pid);

        return result switch
        {
            TransactionCheckStatus.Passed => Ok(transaction),
            TransactionCheckStatus.WalletNotFound => BadRequest(Result.Error(MSG.WalletInvalided)),
            TransactionCheckStatus.TradeAccountNotFound => BadRequest(Result.Error(MSG.TradeAccountInvalided)),
            TransactionCheckStatus.CurrencyNotMatch => BadRequest(Result.Error(MSG.CurrencyNotMatched)),
            TransactionCheckStatus.FundTypeNotMatch => BadRequest(Result.Error(MSG.FundTypeNotMatched)),
            _ => BadRequest(Result.Error(MSG.TransferFailed)),
        };
    }

    /// <summary>
    /// Cancel Transfer by Tenant Admin
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(long id)
    {
        var item = await accountingService.TransactionGetAsync(id);
        if (item.IsEmpty())
        {
            return NotFound();
        }

        var result = await accountingService.TransactionTryCancelAsync(id, GetPartyId());
        if (!result)
            return BadRequest(Result.Error(MSG.CancelFailed));

        await mediator.Publish(new TransferCanceledEvent(item));
        return NoContent();
    }

    /// <summary>
    /// Reject Transfer by Tenant Admin
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(long id)
    {
        var item = await accountingService.TransactionGetAsync(id);
        if (item.IsEmpty())
            return NotFound();

        var canTransit = await accountingService.CanTransitToStateAsync(item, ActionTypes.TransferReject,
            StateTypes.TransferRejected, GetPartyId());
        if (!canTransit)
            return BadRequest(Result.Error(ResultMessage.Common.ActionNotAllow));

        var result = await accountingService.TransactionRejectByTenantAsync(item, GetPartyId());
        if (!result)
            return BadRequest(Result.Error(MSG.RejectFailed));

        await mediator.Publish(new TransferRejectedEvent(item));
        return NoContent();
    }

    /// <summary>
    /// Approve and Complete Transfer by Tenant Admin
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(long id)
    {
        var (result, msg) = await acctService.CompleteTransactionAsync(id, GetPartyId());
        return result ? NoContent() : BadRequest(Result.Error(msg));
    }
}