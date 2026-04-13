
using Bacera.Gateway.Context;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.EventHandlers;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = TradeAccount;
using MSG = ResultMessage.Account;

[Area("Tenant")]
[Tags("Tenant/Trade Account")]
[Route("api/" + VersionTypes.V1 + "/[Area]/trade-account")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class TradeAccountController(
    IMediator mediator,
    ITenantGetter tenantGetter,
    TenantDbContext tenantCtx,
    TradingService tradingService,
    ISendMailService sendMailService,
    AccountingService accountingService,
    AccountManageService accManageSvc,
    MyDbContextPool pool,
    IBackgroundJobClient backgroundJobClient)
    : TenantBaseController
{
    /// <summary>
    /// Trade Account Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.IncludeClosed();
        return Ok(await tradingService.TradeAccountQueryAsync(criteria));
    }

    /// <summary>
    /// Get trade account
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TradeAccount(long uid)
    {
        var item = await tradingService.TradeAccountGetByUidAsync(uid);
        return item.IsEmpty() ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Set rebate rule template Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="rebateRuleTemplateId"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/template/{rebateRuleTemplateId:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TradeAccount(long id, long rebateRuleTemplateId)
    {
        var tradeAccount = await tenantCtx.TradeAccounts
            .SingleOrDefaultAsync(x => x.Id == id);

        if (tradeAccount == null)
            return NotFound();

        tradeAccount.RebateBaseSchemaId = rebateRuleTemplateId;
        tenantCtx.TradeAccounts.Update(tradeAccount);

        await tenantCtx.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Get trade account read only code
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/read-only-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TradeAccountReadOnlyCode(long id)
    {
        var item = await tenantCtx.TradeAccountStatuses
            .Where(x => x.Id == id)
            .Select(x => new { x.Id, x.ReadOnlyCode })
            .SingleOrDefaultAsync();
        return item == null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Create trade account
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TradeAccount([FromBody] TradeAccountCreateRequestModel spec)
    {
        var validator = new TradeAccountCreateRequestModelValidator();
        var validatorResult = await validator.ValidateAsync(spec);

        if (validatorResult.IsValid == false)
            return BadRequest(Result.Error(MSG.AccountCreateFailed, validatorResult.Errors));
        if (!pool.IsServiceExisted(spec.ServiceId)) return BadRequest(Result.Error(MSG.ServiceNotFound));

        try
        {
            var info = await accManageSvc.CreateTradeAccountAsync(spec.AccountId
                , spec.ServiceId
                , spec.Leverage
                , spec.Group
                , (CurrencyTypes)spec.CurrencyId);

            if (info.IsEmpty()) return BadRequest(Result.Error(MSG.TradeAccountCreateFailed + "_1_"));

            await accManageSvc.CreateDefaultWalletForAccount(info.AccountId);
            await accManageSvc.AddAccountTagsAsync(info.AccountId, spec.ExtraTags.ToArray());
            await mediator.Publish(new TradeAccountCreatedEvent(info.AccountId
                , info.Password
                , info.PasswordInvestor
                , info.PasswordPhone));
            var tradeAccount = await tenantCtx.TradeAccounts.SingleAsync(x => x.Id == info.AccountId);
            return Ok(tradeAccount);
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Error(MSG.TradeAccountCreateFailed, ex.Message));
        }
    }

    /// <summary>
    /// Read Only Code Notice email preview
    /// </summary>
    /// <param name="id"></param>
    /// <param name="to"></param>
    /// <param name="bcc"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    [HttpPost("{id:long}/read-only-code/notice/preview")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReadOnlyCodeNoticePreview(long id, [FromQuery] string to,
        [FromQuery] List<string> bcc, [FromQuery] string language = "en-us")
    {
        if (!Utils.IsValidEmail(to) || bcc.Any(x => !Utils.IsValidEmail(x)))
            return BadRequest(Result.Error(ResultMessage.Register.InvalidEmail));

        var account = await tenantCtx.TradeAccounts
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.AccountNumber,
                x.IdNavigation.Name,
                x.IdNavigation.PartyId,
                x.CreatedOn,
                ReadOnlyCode = x.TradeAccountStatus != null ? x.TradeAccountStatus.ReadOnlyCode : string.Empty
            }).SingleOrDefaultAsync();

        if (account == null)
            return NotFound();

        if (string.IsNullOrEmpty(account.ReadOnlyCode))
            return BadRequest(Result.Error(MSG.ReadyOnlyCodeNotExists));

        var vm = new ReadOnlyCodeNoticeViewModel
        {
            Email = to,
            BccEmails = bcc,
            NativeName = account.Name,
            AccountNumber = account.AccountNumber,
            ReadOnlyCode = account.ReadOnlyCode,
            Date = account.CreatedOn.ToString("yyyy-MM-dd")
        };
        var html = await sendMailService.GenerateEmailWithTemplateAsync(vm, language);
        return Content(html);
    }

    /// <summary>
    /// Send out Read Only Code 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="to"></param>
    /// <param name="bcc"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    [HttpPost("{id:long}/read-only-code/notice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReadOnlyCodeNotice(long id, [FromQuery] string to, [FromQuery] List<string> bcc,
        [FromQuery] string language = "en-us")
    {
        if (!Utils.IsValidEmail(to) || bcc.Any(x => !Utils.IsValidEmail(x)))
            return BadRequest(Result.Error(ResultMessage.Register.InvalidEmail));

        var account = await tenantCtx.TradeAccounts
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.AccountNumber,
                x.IdNavigation.Name,
                x.IdNavigation.PartyId,
                x.CreatedOn,
                ReadOnlyCode = x.TradeAccountStatus != null ? x.TradeAccountStatus.ReadOnlyCode : string.Empty
            }).SingleOrDefaultAsync();

        if (account == null)
            return NotFound();

        if (string.IsNullOrEmpty(account.ReadOnlyCode))
            return BadRequest(Result.Error(MSG.ReadyOnlyCodeNotExists));

        var vm = new ReadOnlyCodeNoticeViewModel
        {
            Email = to,
            BccEmails = bcc,
            NativeName = account.Name,
            AccountNumber = account.AccountNumber,
            ReadOnlyCode = account.ReadOnlyCode,
            Date = account.CreatedOn.ToString("yyyy-MM-dd"),
        };
        backgroundJobClient.Enqueue<IGeneralJob>(
            x => x.ReadOnlyCodeNoticeAsync(tenantGetter.GetTenantId(), vm, language));

        var html = await sendMailService.GenerateEmailWithTemplateAsync(vm, language);
        var history = CommunicateHistory.Build(account.PartyId, account.Id, CommunicateTypes.ReadOnlyCodeNotice, html,
            GetPartyId());
        await tenantCtx.CommunicateHistories.AddAsync(history);
        await tenantCtx.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Update trade account leverage at database and MT4
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="leverage"></param>
    /// <returns></returns>
    [HttpPut("{accountId:long}/leverage/{leverage:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateLeverage(long accountId, int leverage)
    {
        if (leverage is < 1 or > 1000)
            return BadRequest(Result.Error(MSG.InvalidLeverage));

        if (!await tenantCtx.TradeAccounts.AnyAsync(x => x.Id == accountId))
            return NotFound();

        var result = await tradingService.TradeAccountChangeLeverage(accountId, leverage, GetPartyId());

        return result ? NoContent() : BadRequest(Result.Error(MSG.TradeServerError));
    }

    // /// <summary>
    // /// Update trade account balance at database and MT4
    // /// </summary>
    // /// <param name="accountId"></param>
    // /// <param name="spec"></param>
    // /// <returns></returns>
    // [HttpPut("{accountId:long}/balance")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<ActionResult<PaymentService.AccessResponseModel>> UpdateBalance(long accountId,
    //     [FromBody] TradeAccount.ChangeBalanceSpec spec)
    // {
    //     if (spec.AccountId != accountId)
    //         return BadRequest(Result.Error(ResultMessage.Common.InvalidInput));
    //
    //     if (spec.Amount is 0 or < -50000m or > 50000m)
    //         return BadRequest(Result.Error(MSG.InvalidAmount));
    //
    //     if (!await _tenantDbContext.TradeAccounts.AnyAsync(x => x.Id == accountId))
    //         return BadRequest(Result.Error(MSG.AccountNotExists));
    //
    //     var (result, ticket) =
    //         await _tradingSvc.TradeAccountChangeBalanceAndUpdateStatusWithAuditAsync(accountId, spec, GetPartyId());
    //
    //     if (!result)
    //         return BadRequest(Result.Error(ticket));
    //
    //     return Ok(new { Ticket = ticket });
    // }

    // /// <summary>
    // /// Update trade account credit MT4
    // /// </summary>
    // /// <param name="accountId"></param>
    // /// <param name="spec"></param>
    // /// <returns></returns>
    // [HttpPut("{accountId:long}/credit")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<ActionResult<object>> UpdateCredit(long accountId,
    //     [FromBody] TradeAccount.ChangeBalanceSpec spec)
    // {
    //     if (spec.AccountId != accountId)
    //         return BadRequest(Result.Error(ResultMessage.Common.InvalidInput));
    //
    //     if (spec.Amount is 0 or < -50000m or > 50000m)
    //         return BadRequest(Result.Error(MSG.InvalidAmount));
    //
    //     if (!await _tenantDbContext.TradeAccounts.AnyAsync(x => x.Id == accountId))
    //         return BadRequest(Result.Error(MSG.AccountNotExists));
    //
    //     spec.Comment = "Credit " + GetCreditComment(spec.Amount, spec.Comment);
    //     string ticket;
    //     try
    //     {
    //         (var success, ticket) =
    //             await _tradingSvc.TradeAccountChangeCreditAsync(accountId, spec.Amount, spec.Comment);
    //         spec.Ticket = ticket;
    //         if (!success)
    //             return BadRequest(Result.Error(MSG.TradeServerError));
    //     }
    //     catch
    //     {
    //         return BadRequest(Result.Error(MSG.TradeServerError));
    //     }
    //
    //     spec.AdjustType = "Credit";
    //     var data = JsonConvert.SerializeObject(spec);
    //     var audit = Audit.Build(AuditTypes.TradeAccountBalance, AuditActionTypes.Update, GetPartyId(), accountId,
    //         data, System.Net.Dns.GetHostName());
    //     await _tenantDbContext.Audits.AddAsync(audit);
    //     await _tenantDbContext.SaveChangesAsync();
    //
    //     return Ok(new { Ticket = ticket });
    // }

    /// <summary>
    /// Check trade account balance
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    [HttpPut("{accountId:long}/check-balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> CheckBalance(long accountId)
    {
        if (!await tenantCtx.TradeAccounts.AnyAsync(x => x.Id == accountId))
            return BadRequest(Result.Error(MSG.AccountNotExists));

        try
        {
            var (success, balance) = await tradingService.TradeAccountCheckBalance(accountId);
            if (!success)
                return BadRequest(Result.Error(MSG.TradeServerError));
            return Ok(new { Balance = balance });
        }
        catch
        {
            return BadRequest(Result.Error(MSG.TradeServerError));
        }
    }

    private static string GetCreditComment(decimal amount, string comment)
        => (amount > 0 ? "in" : "out") + " " + comment.Trim();

    private async Task CreateDefaultWalletForAccount(long accountId, CurrencyTypes currencyId)
    {
        var account = await tradingService.AccountGetAsync(accountId);
        await accountingService.WalletGetOrCreateAsync(account.PartyId, currencyId, (FundTypes)account.FundType);
    }

    private async Task<TradeAccountStatus> GetStatus(long accountId) =>
        await tenantCtx.TradeAccountStatuses
            .Where(x => x.Id == accountId)
            .Select(x => new TradeAccountStatus
            {
                Balance = x.Balance,
            }).SingleOrDefaultAsync() ?? new TradeAccountStatus();

    private static string GetChange(TradeAccountStatus originalValues, TradeAccountStatus currentValues, string action,
        string ticket)
    {
        return JsonConvert.SerializeObject(new
        {
            originalValues = new { originalValues.Balance, Ticket = "", Action = "" },
            currentValues = new { currentValues.Balance, Ticket = ticket, Action = action }
        });
    }
}