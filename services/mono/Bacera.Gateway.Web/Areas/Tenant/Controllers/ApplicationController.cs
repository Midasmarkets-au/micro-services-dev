using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Web.EventHandlers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Application;
using MSG = ResultMessage.Application;

[Tags("Tenant/Application")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class ApplicationController(
    IMediator mediator,
    AuthDbContext authDbContext,
    UserManager<User> userManager,
    TradingService tradingService,
    TenantDbContext tenantDbContext,
    AccountingService accountingService,
    AccountManageService accManageSvc,
    IApplicationTokenService tokenService,
    ApplicationService applicationService)
    : TenantBaseController
{
    private readonly AuthDbContext _authDbContext = authDbContext;
    private readonly AccountingService _accountingSvc = accountingService;

    /// <summary>
    /// Application pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var hideEmail = ShouldHideEmail();
        var result = await applicationService.TenantQueryAsync(criteria, hideEmail);
        return Ok(result);
    }

    /// <summary>
    /// Update Application
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(long id, [FromBody] ApplicationSupplement request)
    {
        var item = await applicationService.GetAsync(id);
        if (item.IsEmpty())
            return NotFound();
        if (item.CanUpdate() == false)
            return BadRequest(ToErrorResult(MSG.ApplicationStateCannotUpdate));
        return Ok(await applicationService.UpdateSupplementAsync(id, request));
    }

    /// <summary>
    /// Create Application for user
    /// </summary>
    /// <param name="partyId"></param>
    /// <param name="supplement"></param>
    /// <returns></returns>
    [HttpPost("for-user/{partyId:long}/trade-account")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    public async Task<ActionResult<M>> Create(long partyId, [FromBody] ApplicationSupplement supplement)
    {
        var result = await applicationService.CreateApplication(partyId, ApplicationTypes.TradeAccount, supplement);
        if (result.Id == 0)
            return BadRequest(result);

        await mediator.Publish(new ApplicationCreatedEvent(result));
        return Ok(result);
    }

    /// <summary>
    /// Get Application
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var item = await applicationService.GetAsync(id);
        return item.IsEmpty() ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Approve Application
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Account))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(long id, [FromBody] ApplicationSupplement request)
    {
        var application = await applicationService.GetAsync(id);
        if (application.IsEmpty())
            return NotFound();
        if (application.CanApprove() == false)
            return BadRequest(ToErrorResult(MSG.ApplicationStateCannotApprove));
        var operatorPid = GetPartyId();
        switch (application.Type)
        {
            case (short)ApplicationTypes.TradeAccount:
                return await ApproveAccountApplicationAsync(application, request);
            // return await LegacyApproveAccountApplicationAsync(application, request);
            case (short)ApplicationTypes.TradeAccountChangeLeverage:
                return await ApproveChangeLeverageAsync(application, operatorPid);
            case (short)ApplicationTypes.TradeAccountChangePassword:
                return await ApproveChangePasswordAsync(application);
            case (short)ApplicationTypes.WholesaleAccount:
                return await ApproveWholeSaleAccountAsync(application, operatorPid);
            case (short)ApplicationTypes.WholesaleReferral:
                return await ApproveWholeSaleReferralAsync(application, operatorPid);
            default:
                return BadRequest(Result.Error(MSG.ApplicationTypeInvalid));
        }
    }

    /// <summary>
    /// Reject Application
    /// </summary>
    /// <param name="id"></param>
    /// <param name="requestModel"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(long id, [FromBody] M.RejectRequestModel requestModel)
    {
        var item = await applicationService.GetAsync(id);
        if (item.IsEmpty())
            return NotFound();

        if (item.CanReject() == false)
            return BadRequest(ToErrorResult(MSG.ApplicationStateCannotReject));

        var result = await applicationService.RejectAsync(id, GetPartyId(), requestModel.Reason);
        if (!result)
            return BadRequest(ToErrorResult(MSG.RejectFailed));

        await mediator.Publish(new ApplicationRejectedEvent(item));
        return NoContent();
    }

    /// <summary>
    /// Reverse a Rejected Application
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/reverse-reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReverseReject(long id)
        => await applicationService.ReverseRejectAsync(id, GetPartyId())
            ? NoContent()
            : BadRequest(ToErrorResult(MSG.ReverseRejectFailed));

    /// <summary>
    /// Complete Application
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(long id)
    {
        var item = await applicationService.GetAsync(id);
        if (item.IsEmpty()) return NotFound();

        if (item.CanComplete() == false) return BadRequest(ToErrorResult(MSG.ApplicationStateCannotComplete));

        var result = await applicationService.CompleteAsync(id, GetPartyId());
        return result ? NoContent() : BadRequest(ToErrorResult(MSG.CompleteFailed));
    }

    private async Task<IActionResult> ApproveChangePasswordAsync(M item)
    {
        var supplement = JsonConvert
            .DeserializeObject<TradeAccountApplicationSupplement.ChangePasswordDTO>(item.Supplement ?? "{}");

        if (supplement == null || false == supplement.IsValidated())
            return BadRequest(Result.Error(MSG.ChangePasswordDataError));

        var tradeAccounts = await tradingService.TradeAccountQueryAsync(new TradeAccount.Criteria
            { Uid = supplement.AccountUid, PartyId = item.PartyId });
        if (false == tradeAccounts.Data.Any())
            return BadRequest(Result.Error(ResultMessage.Account.AccountNotExists));
        var account = tradeAccounts.Data.First();
        var user = await userManager.Users.SingleOrDefaultAsync(x =>
            x.PartyId == item.PartyId && x.TenantId == GetTenantId());
        if (user == null)
            return BadRequest(Result.Error(ResultMessage.Common.UserNotFound));

        // var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

        var tokenRequest = ApplicationToken.Build(user.PartyId, TokenTypes.TradeAccountChangePasswordToken,
            supplement.AccountUid);
        var token = await tokenService.GenerateTokenAsync(tokenRequest, TimeSpan.FromHours(24));
        if (token == null)
            return BadRequest(Result.Error(ResultMessage.Common.TokenGenerateFail));

        var platform = await tenantDbContext.TradeServices.Where(x => x.Id == account.ServiceId)
            .Select(x => x.Platform).FirstOrDefaultAsync();
        var viewModel = new ResetTradeAccountPasswordViewModel(GetTenantId(), user.Email!, user.PartyId,
            supplement.AccountUid,
            account.AccountNumber, user.GuessUserName(),
            supplement.CallbackUrl, token.Token, (PlatformTypes)platform);

        var result =
            await applicationService.ApproveAsync(item.Id, GetPartyId(), (ApplicationTypes)item.Type,
                account.Id);
        if (result == false)
            return BadRequest(ToErrorResult(MSG.ApproveFailed));

        await mediator.Publish(new TradeAccountPasswordChangeRequestedEvent(viewModel));
        await mediator.Publish(new ApplicationApprovedEvent(item));
        return NoContent();
    }
    
    private async Task<IActionResult> ApproveWholeSaleAccountAsync(M item, long operatorPid)
    {
        TradeAccountApplicationSupplement.WholesaleDTO supplement;
        try
        {
            supplement = JsonConvert
                             .DeserializeObject<
                                 TradeAccountApplicationSupplement.WholesaleDTO>(item.Supplement ?? "{}") ??
                         new TradeAccountApplicationSupplement.WholesaleDTO();
        }
        catch
        {
            supplement = new TradeAccountApplicationSupplement.WholesaleDTO();
        }

        if (!supplement.IsValidated())
            return BadRequest(Result.Error(MSG.ChangeAccountTypeError));

        // update all accounts to Wholesale under this party
        var accounts = await tenantDbContext.Accounts
            .Where(x => x.PartyId == item.PartyId)
            .Where(x => x.Type != (short)AccountTypes.WholesaleAlpha)
            .ToListAsync();

        if (!accounts.Any())
            return BadRequest(Result.Error(ResultMessage.Account.AccountNotExists));

        foreach (var account in accounts)
        {
            account.Type = (short)AccountTypes.WholesaleAlpha;
            account.UpdatedOn = DateTime.UtcNow;
        }

        tenantDbContext.Accounts.UpdateRange(accounts);
        await tenantDbContext.SaveChangesWithAuditAsync(operatorPid);

        var user = await userManager.Users.SingleAsync(x => x.PartyId == item.PartyId && x.TenantId == GetTenantId());
        if (!await userManager.IsInRoleAsync(user, UserRoleTypesString.Wholesale))
            await userManager.AddToRoleAsync(user, UserRoleTypesString.Wholesale);

        var result =
            await applicationService.ApproveAsync(item.Id, GetPartyId(), ApplicationTypes.WholesaleAccount,
                accounts.First().Id);

        if (result == false)
            return BadRequest(ToErrorResult(MSG.ApproveFailed));

        await mediator.Publish(new ApplicationApprovedEvent(item));
        return NoContent();
    }
    
    private async Task<IActionResult> ApproveWholeSaleReferralAsync(M item, long operatorPid)
    {
        var application = await applicationService.GetAsync(item.Id);
        if (application.IsEmpty())
            return NotFound();

        var result =
            await applicationService.ApproveAsync(item.Id, GetPartyId(), (ApplicationTypes)item.Type);
        if (result == false)
            return BadRequest(ToErrorResult(MSG.ApproveFailed));

        await mediator.Publish(new ApplicationApprovedEvent(item));
        return NoContent();
    }

    private async Task<IActionResult> ApproveChangeLeverageAsync(M item, long operatorPartyId = 1)
    {
        var supplement = JsonConvert
            .DeserializeObject<TradeAccountApplicationSupplement.ChangeLeverageDTO>(item.Supplement ?? "{}");
        if (supplement == null || false == supplement.IsValidated())
            return BadRequest(Result.Error(ResultMessage.Common.InvalidInput));

        var tradeAccounts = await tradingService.TradeAccountQueryAsync(new TradeAccount.Criteria
            { Uid = supplement.AccountUid, PartyId = item.PartyId });
        if (false == tradeAccounts.Data.Any()) return NotFound();
        var account = tradeAccounts.Data.First();

        var accountStatus = await tradingService.GetTradeAccountBalanceAndLeverageFromServer(account.Id);

        // update leverage and balance if changed
        if (accountStatus.Item1
            && account.TradeAccountStatus != null
            && (accountStatus.Item2 != account.TradeAccountStatus.Leverage
                || accountStatus.Item3 != account.TradeAccountStatus.Balance))
        {
            var tradeAccountStatus = await tenantDbContext.TradeAccountStatuses
                .Where(x => x.Id == account.Id).SingleAsync();
            tradeAccountStatus.Leverage = accountStatus.Item2;
            tradeAccountStatus.Balance = accountStatus.Item3;
            tradeAccountStatus.UpdatedOn = DateTime.UtcNow;
            tenantDbContext.TradeAccountStatuses.Update(tradeAccountStatus);
            await tenantDbContext.SaveChangesWithAuditAsync(operatorPartyId);
        }

        if (accountStatus.Item1 && accountStatus.Item2 == supplement.Leverage)
            return BadRequest(Result.Error(MSG.HasSameLeverage));

        var response =
            await tradingService.TradeAccountChangeLeverage(account.Id, supplement.Leverage, operatorPartyId);
        if (response == false) return BadRequest(MSG.TradeServiceErrorPleaseRetry);

        var result =
            await applicationService.ApproveAsync(item.Id, GetPartyId(), (ApplicationTypes)item.Type,
                account.Id);
        if (result == false)
            return BadRequest(ToErrorResult(MSG.ApproveFailed));

        await mediator.Publish(new TradeAccountLeverageChangedEvent(item.PartyId, account.AccountNumber,
            supplement.Leverage,
            accountStatus.Item2));

        await mediator.Publish(new ApplicationApprovedEvent(item));
        return NoContent();
    }

    private async Task<IActionResult> ApproveAccountApplicationAsync(M application, ApplicationSupplement request)
    {
        var supplement = JsonConvert
            .DeserializeObject<ApplicationSupplement>(application.Supplement!)!
            .Merge(request);

        var validator = new ApplicationSupplementValidator();
        var validatorResult = await validator.ValidateAsync(supplement);
        if (validatorResult.IsValid == false)
            return BadRequest(Result.Error(MSG.ApproveFailed, validatorResult.Errors));

        var (result, msg) = supplement.Role switch
        {
            // AccountRoleTypes.Client => await tradingService.CreateClientAccountAsync(application.PartyId
            //     , supplement.ParentAccountId
            //     , supplement.CurrencyIdType
            //     , supplement.FundType, supplement.AccountType
            //     , referCode: supplement.ReferCode?.Trim().ToUpper()
            //     , operatorPartyId: GetPartyId()
            //     , tenantId: GetTenantId()),

            AccountRoleTypes.Client => await accManageSvc.CreateClientAsync(application.PartyId
                , supplement.ParentAccountId
                , supplement.CurrencyIdType
                , supplement.FundType, supplement.AccountType
                , referCode: supplement.ReferCode?.Trim().ToUpper()
                , tradeServiceId: supplement.ServiceId
                , operatorPartyId: GetPartyId()
                , tenantId: GetTenantId()
                , accountNumber: request.AccountNumber),

            AccountRoleTypes.Agent => await tradingService.CreateAgentAccountAsync(application.PartyId
                , supplement.AgentSelfGroup ?? string.Empty
                , supplement.ParentAccountId
                , supplement.CurrencyIdType
                , supplement.FundType, supplement.AccountType
                , referCode: supplement.ReferCode?.Trim().ToUpper()
                , operatorPartyId: GetPartyId()
                , tenantId: GetTenantId()),

            AccountRoleTypes.Sales => await tradingService.CreateSalesAccountAsync(application.PartyId
                , supplement.SalesDividedGroup ?? string.Empty
                , supplement.SalesSelfGroup ?? string.Empty
                , supplement.ParentAccountId
                , supplement.CurrencyIdType
                , tenantId: GetTenantId()),

            AccountRoleTypes.Rep => await tradingService.CreateRepAccountAsync(application.PartyId
                , supplement.SalesSelfGroup ?? string.Empty
                , supplement.SalesDividedGroup ?? string.Empty
                , tenantId: GetTenantId()),

            _ => (false, "__ROLE_NOT_SUPPORTED__")
        };

        if (!result) return BadRequest(ToErrorResult(msg));


        var accountId = long.Parse(msg);
        await applicationService.ApproveAsync(application.Id, GetPartyId(), (ApplicationTypes)application.Type,
            accountId);
        await mediator.Publish(new AccountCreatedEvent(accountId));
        // await _tradingService.BuildAndAssignAccountGroup(accountId);
        return Ok(await tradingService.AccountGetAsync(accountId));
    }
}