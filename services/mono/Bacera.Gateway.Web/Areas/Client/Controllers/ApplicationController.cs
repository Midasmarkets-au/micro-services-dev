
using Bacera.Gateway.Auth;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Web.EventHandlers;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

using M = Application;
using MSG = ResultMessage.Application;

[Tags("Client/Application")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class ApplicationController(
    IMediator mediator,
    UserService userService,
    UserManager<User> userManager,
    AuthDbContext authDbContext,
    TradingService tradingService,
    TenantDbContext tenantDbContext,
    IApplicationTokenService tokenSvc,
    AccountManageService accManageSvc,
    ApplicationService applicationService)
    : ClientBaseController
{
    private readonly UserService _userSvc = userService;

    /// <summary>
    /// Application Pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<M.ResponseModel>, M.Criteria>>> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.PartyId = GetPartyId();

        var hideEmail = ShouldHideEmail();
        var result = await applicationService.QueryAsync(criteria, hideEmail);
        return Ok(result);
    }

    /// <summary>
    /// Create Application
    /// </summary>
    /// <param name="supplement"></param>
    /// <returns></returns>
    [HttpPost("trade-account")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    public async Task<ActionResult<M>> Create([FromBody] ApplicationSupplement supplement)
    {
        var partyId = GetPartyId();

        if (!string.IsNullOrEmpty(supplement.ReferCode) && supplement.ReferCode.Contains("code="))
        {
            return BadRequest(Result.Error("Invalid refer code"));
        }

        if (string.IsNullOrEmpty(supplement.ReferCode) && User.IsInRole(UserRoleTypesString.IB))
        {
            var referralCode = await tenantDbContext.ReferralCodes
                .Where(x => x.PartyId == partyId)
                .Where(x => x.ServiceType == (int)ReferralServiceTypes.Client)
                .OrderBy(x => x.Id)
                .Select(x => x.Code)
                .FirstOrDefaultAsync();
            supplement.ReferCode = referralCode;
        }

        var existing = await tenantDbContext.Applications
            .Where(x => x.Type == (int)ApplicationTypes.TradeAccount)
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Status == (int)ApplicationStatusTypes.AwaitingApproval || x.Status == (int)ApplicationStatusTypes.Approved)
            .AnyAsync();

        if (existing) return BadRequest(Result.Error("You have a pending application"));

        var result = await applicationService.CreateApplication(partyId, ApplicationTypes.TradeAccount, supplement);
        if (result.Id == 0)
        {
            // If Id is 0, validation failed. RejectedReason contains the error message
            var errorMessage = string.IsNullOrWhiteSpace(result.RejectedReason) 
                ? "Failed to create application" 
                : result.RejectedReason;
            return BadRequest(Result.Error(errorMessage));
        }

        await mediator.Publish(new ApplicationCreatedEvent(result));
        return Ok(result);
    }

    /// <summary>
    /// Create Whole Sale Account Application
    /// </summary>
    /// <param name="supplement"></param>
    /// <returns></returns>
    [HttpPost("whole-sale-account")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    public async Task<ActionResult<M>> CreateWholeSaleAccount(
        [FromBody] TradeAccountApplicationSupplement.WholesaleDTO supplement)
    {
        var exist = await tradingService.AccountGetByUidAsync(supplement.AccountUid);

        if (exist.IsEmpty() || exist.PartyId != GetPartyId())
            return BadRequest(Result.Error(ResultMessage.Account.AccountNotExists));

        //if (exist.Status != (int)AccountStatusTypes.Activate)
        //    return BadRequest(Result.Error(ResultMessage.Account.AccountInactivated));

        if (exist.Type == (int)AccountTypes.WholesaleAlpha)
            return BadRequest(Result.Error(ResultMessage.Account.AccountTypeAlreadyAssigned));

        // if (await _userSvc.PartyHasTagAsync(GetPartyId(), "WholesaleApplicationBanned"))
        //     return BadRequest(Result.Error(ResultMessage.Account.AccountTypeAlreadyAssigned));

        var result = await applicationService
            .CreateApplication(GetPartyId(), ApplicationTypes.WholesaleAccount, supplement);
        if (result.Id == 0)
            return BadRequest(result);

        await mediator.Publish(new ApplicationCreatedEvent(result));

        return Ok(result);
    }

    /// <summary>
    /// Create Application for Change Trade Account Leverage
    /// </summary>
    /// <param name="supplement"></param>
    /// <returns></returns>
    [HttpPost("change-leverage")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<M>> ChangeTradeAccountLeverage(
        [FromBody] TradeAccountApplicationSupplement.ChangeLeverageDTO supplement)
    {
        var validator = new TradeAccountApplicationSupplement.ChangeLeverageDTOValidator();
        var validationResult = await validator.ValidateAsync(supplement);
        if (validationResult.IsValid == false)
        {
            return BadRequest(Result.Error(MSG.InvalidParameter, validationResult.Errors.Select(x => x.ErrorMessage).ToList()));
        }

        var partyId = GetPartyId();
        var count = await tenantDbContext.Applications
            .Where(x => x.PartyId == partyId && x.Type == (int)ApplicationTypes.TradeAccountChangeLeverage)
            .Where(x => x.Status == (int)ApplicationStatusTypes.AwaitingApproval)
            .CountAsync();

        if (count > 2) return BadRequest(Result.Error("Two maximum pending applications to change leverage"));

        var accountId = await tradingService.TradeAccountLookupByUidAsync(uid: supplement.AccountUid);
        if (accountId == 0 || false == await tradingService.AccountExistsForPartyAsync(accountId, GetPartyId()))
            return NotFound();

        var availableLeverages = await accManageSvc.GetAccountAvailableLeverages(accountId);
        if (availableLeverages.Contains(supplement.Leverage) == false)
            return BadRequest(Result.Error("Invalid leverage"));

        var result = await applicationService.CreateApplication(GetPartyId(), ApplicationTypes.TradeAccountChangeLeverage, supplement,
            referenceId: supplement.AccountUid);

        if (result.Id == 0) return BadRequest(result);

        await mediator.Publish(new ApplicationCreatedEvent(result));

        return Ok(result);
    }

    /// <summary>
    /// Create Application for Change Trade Account Password
    /// </summary>
    /// <param name="supplement"></param>
    /// <returns></returns>
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<M>> ChangeTradeAccountPassword(
        [FromBody] TradeAccountApplicationSupplement.ChangePasswordDTO supplement)
    {
        var validator = new TradeAccountApplicationSupplement.ChangePasswordDTOValidator();
        var validationResult = await validator.ValidateAsync(supplement);
        if (validationResult.IsValid == false)
        {
            return BadRequest(Result.Error(MSG.InvalidParameter, validationResult.Errors.Select(x => x.ErrorMessage).ToList()));
        }

        var account = await tenantDbContext.Accounts.FirstOrDefaultAsync(x => x.Uid == supplement.AccountUid);

        if (account is not { HasTradeAccount: true }) return NotFound();

        var application = await applicationService.CreateApplication(GetPartyId()
            , ApplicationTypes.TradeAccountChangePassword
            , supplement
            , referenceId: supplement.AccountUid);

        if (application.Id == 0) return BadRequest(application);

        await mediator.Publish(new ApplicationCreatedEvent(application));

        var user = await userManager.Users.SingleOrDefaultAsync(x => x.TenantId == GetTenantId() && x.PartyId == GetPartyId());
        if (user == null) return BadRequest(Result.Error(ResultMessage.Common.UserNotFound));

        var tokenRequest = ApplicationToken.Build(user.PartyId, TokenTypes.TradeAccountChangePasswordToken, supplement.AccountUid);

        var token = await tokenSvc.GenerateTokenAsync(tokenRequest, TimeSpan.FromHours(24));
        if (token == null) return BadRequest(Result.Error(ResultMessage.Common.TokenGenerateFail));

        var platform = await tenantDbContext.TradeServices.Where(x => x.Id == account.ServiceId)
            .Select(x => x.Platform)
            .FirstOrDefaultAsync();

        var viewModel = new ResetTradeAccountPasswordViewModel(GetTenantId(), user.Email!, user.PartyId,
            supplement.AccountUid,
            account.AccountNumber, user.GuessUserName(),
            supplement.CallbackUrl, token.Token, (PlatformTypes)platform);

        var result = await applicationService.ApproveAsync(application.Id, GetPartyId(), (ApplicationTypes)application.Type, account.Id);

        if (result == false) return BadRequest(ToErrorResult(MSG.ApproveFailed));

        await mediator.Publish(new TradeAccountPasswordChangeRequestedEvent(viewModel));
        await mediator.Publish(new ApplicationApprovedEvent(application));
        return Ok(application);
    }
    
    /// <summary>
    /// Create Whole Sale Referral Application
    /// </summary>
    /// <param name="supplement"></param>
    /// <returns></returns>
    [HttpPost("wholesale-referral")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = UserRoleTypesString.Wholesale)]
    public async Task<ActionResult<M>> CreateWholeSaleReferral(
        [FromBody] TradeAccountApplicationSupplement.WholesaleReferralDTO supplement)
    {
        var tenantId = GetTenantId();
        var target = await authDbContext.Users
            .Where(x => x.TenantId == tenantId && x.Email == supplement.Email)
            .Select(x => new
            {
                x.PartyId,
                Roles = x.UserRoles.Where(y => y.ApplicationRole.Name == UserRoleTypesString.Wholesale)
                    .Select(y => y.ApplicationRole.Name)
            })
            .FirstOrDefaultAsync();

        if (target == null) return BadRequest(Result.Error(ResultMessage.User.UserNotFound));
        if (target.Roles.Contains(UserRoleTypesString.Wholesale))
            return BadRequest(Result.Error(ResultMessage.Account.AccountIsWholesale));

        var accountPid = await tenantDbContext.Accounts
            .Where(x => x.AccountNumber == supplement.AccountNumber && x.PartyId == target.PartyId)
            .Select(x => x.PartyId)
            .FirstOrDefaultAsync();

        if (accountPid == 0) return BadRequest(Result.Error(ResultMessage.Account.AccountNotExists));

        var result = await applicationService.CreateApplication(GetPartyId(), ApplicationTypes.WholesaleReferral,
            supplement, referenceId: accountPid);
        
        if (result.Id == 0)
            return BadRequest(result);

        await mediator.Publish(new ApplicationCreatedEvent(result));
        return Ok(result);
    }
}