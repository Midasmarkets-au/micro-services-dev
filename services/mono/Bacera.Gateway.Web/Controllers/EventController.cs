using Bacera.Gateway.Auth;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Bacera.Gateway.Web.Controllers;

public class EventController(
    ILogger<EventController> logger,
    UserManager<User> userManager,
    TradingService tradingService,
    TenantDbContext tenantDbContext,
    ITenantGetter tenancySvc,
    AccountManageService accManageSvc,
    ConfigurationService configurationService)
    : BaseController
{
    [HttpPost("user/register")]
    public async Task<IActionResult> CreateEventUser(EventUserCreateSpec spec, string token)
    {
        var ip = GetRemoteIpAddress().Trim();
        token = token.Trim();
        var ipList = await configurationService.GetIpSettingAsync();
        if (!ipList.Any())
        {
            logger.LogWarning("IP Setting is empty");
            return BadRequest(Result.Error("__IP_SETTING_EMPTY__"));
        }

        if (ipList.All(x => x.Ip != ip))
        {
            logger.LogWarning("IP {Ip} is not allowed", ip);
            return BadRequest(Result.Error("__IP_AUTH_FAIL__"));
        }

        if (ipList.Where(x => x.Ip == ip)
            .Where(x => !string.IsNullOrEmpty(x.Token))
            .Where(x => x.Type == IpSettingTypes.AllowedServerApi)
            .All(x => x.Token != token))
        {
            logger.LogWarning("IP {Ip} is not allowed with token {Token}", ip, token);
            return BadRequest(Result.Error("__TOKEN__AUTH_FAIL__"));
        }

        var email = spec.Email.ToLower().Trim();
        // check user exists
        var userExists = await CheckUserExists(email);
        if (userExists)
            return BadRequest(Result.Error(ResultMessage.Register.EmailExists));

        var tradeSvc = await tenantDbContext.TradeServices
            .FirstOrDefaultAsync(x => x.Platform == (int)PlatformTypes.MetaTrader5);

        if (tradeSvc == null)
            return BadRequest(Result.Error("__TRADE_SERVICE_NOT_FOUND__"));
        TradeServiceOptions tradeSvcOptions;
        try
        {
            var options = JsonConvert.DeserializeObject<TradeServiceOptions>(tradeSvc.Configuration);
            if (options == null)
                return BadRequest(Result.Error("__TRADE_SERVICE_OPTIONS_NOT_FOUND__"));
            tradeSvcOptions = options;
        }
        catch (Exception)
        {
            return BadRequest(Result.Error("__TRADE_SERVICE_OPTIONS_NOT_FOUND__"));
        }

        if (tradeSvcOptions.Groups != null && !tradeSvcOptions.Groups.Contains(spec.TradeAccountGroup))
            return BadRequest(Result.Error("__TRADE_ACCOUNT_GROUP_NOT_FOUND__"));

        // check rebate rule
        if (false == await tenantDbContext.RebateDirectSchemas.AnyAsync(x => x.Id == spec.RebateDirectSchemaId))
        {
            return BadRequest(Result.Error("__REBATE_DIRECT_SCHEMA_NOT_FOUND__"));
        }

        // check agent account
        var agentAccount = await tenantDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == spec.AgentAccountId);
        if (agentAccount == null)
            return BadRequest(Result.Error("__AGENT_ACCOUNT_NOT_FOUND__"));
        if (agentAccount.Role != (int)AccountRoleTypes.Agent)
            return BadRequest(Result.Error("__ACCOUNT_IS_NOT_AGENT__"));

        if (agentAccount.SalesAccountId == null)
            return BadRequest(Result.Error("__AGENT_HAS_NO_ASSIGNED_SALES__"));

        //// Generate User Id
        var party = Party.Create(email);
        party.SiteId = 0;
        await tenantDbContext.PartyRoles.AddAsync(new PartyRole { Party = party, RoleId = (int)UserRoleTypes.Client });
        await tenantDbContext.SaveChangesAsync();

        var tenantId = tenancySvc.GetTenantId();
        
        var uid = await Utils.GenerateUniqueIdAsync(async uid =>
            await userManager.Users.Where(x => x.TenantId == tenantId).AnyAsync(x => x.Uid == uid)
        );
        var user = Auth.User.Create(email)
            .SetUid(uid)
            .Party(party.Id).Tenant(tenantId)
            .SetName(spec.FirstName, spec.LastName)
            .SetPhoneNumber(spec.Phone)
            .SetCcc(spec.Ccc)
            .Ip(GetRemoteIpAddress());

        user.EmailConfirmed = true;
        user.NativeName = spec.NativeName.Trim();

        var result = await userManager.CreateAsync(user, spec.Password);

        if (!result.Succeeded)
            return BadRequest(Result.Error(result.Errors.FirstOrDefault()?.Description ?? "__CREATE_USER_ERROR__"));

        await userManager.AddClaimsAsync(user, new List<Claim>
        {
            new(UserClaimTypes.PartyId, party.Id.ToString()),
            new(UserClaimTypes.TenantId, tenancySvc.GetTenantId().ToString()),
        });

        await userManager.AddToRoleAsync(user, UserRoleTypesString.Client);

        logger.LogInformation("User {Uid} registered via Event", user.Uid);
        //
        // var (createResult, msg) = await tradingService.CreateClientAccountAsync(
        //     user.PartyId
        //     , agentAccount.Id
        //     , CurrencyTypes.USD
        //     , FundTypes.Wire
        //     , AccountTypes.Standard
        //     , null
        //     , false
        //     , ""
        //     , 0
        //     , tenancySvc.GetTenantId());

        var (createResult, msg) = await accManageSvc.CreateClientAsync(
            user.PartyId
            , agentAccount.Id
            , CurrencyTypes.USD
            , FundTypes.Wire
            , AccountTypes.Standard
            , null
            , false
            , ""
            , 0
            , tenancySvc.GetTenantId()
            , tradeSvc.Id);
        if (!createResult)
            return BadRequest(Result.Error(msg));

        var accountId = long.Parse(msg);
        var account = await tenantDbContext.Accounts.SingleAsync(x => x.Id == accountId);

        // var tradeAccount = await tradingService.CreateTradeAccountForAccountAsync(account.Id, tradeSvc.Id,
        //     spec.Password, passwordInvestor, passwordPhone, 400, spec.TradeAccountGroup, CurrencyTypes.USD,
        //     spec.Comment);
        var info = await accManageSvc.CreateTradeAccountAsync(accountId, account.ServiceId, 400, spec.TradeAccountGroup, CurrencyTypes.USD,
            spec.Comment);

        if (info.IsEmpty()) return BadRequest(Result.Error("__CREATE_TRADE_ACCOUNT_ERROR__"));
        try
        {
            var initialBalance = 50000L.ToScaledFromCents(); // have to multiply 10000 coz apiService.ChangeBalance will divide by 10000
            await tradingService.TradeAccountChangeBalanceAndUpdateStatus(account.Id, initialBalance, spec.Comment.Trim());
        }
        catch
        {
            logger.LogInformation("Change balance error Account Id {@AccountId} ", account.Id);
        }

        var rebateDirectRule = new RebateDirectRule
        {
            TargetAccountId = agentAccount.Id,
            SourceTradeAccountId = account.Id,
            RebateDirectSchemaId = spec.RebateDirectSchemaId,
            CreatedBy = 1,
        };
        await tenantDbContext.RebateDirectRules.AddAsync(rebateDirectRule);

        var rebateClientRule =
            await tenantDbContext.RebateClientRules.FirstOrDefaultAsync(x => x.ClientAccountId == account.Id)
            ?? new RebateClientRule
            {
                RebateDirectSchemaId = spec.RebateDirectSchemaId,
                ClientAccountId = account.Id,
                DistributionType = (int)RebateDistributionTypes.Direct,
            };
        rebateClientRule.UpdatedOn = DateTime.UtcNow;
        rebateClientRule.RebateDirectSchemaId = spec.RebateDirectSchemaId;
        rebateClientRule.DistributionType = (int)RebateDistributionTypes.Direct;
        tenantDbContext.RebateClientRules.Update(rebateClientRule);
        await tenantDbContext.SaveChangesAsync();

        return Ok(new { account.AccountNumber });
    }

    private async Task<bool> CheckUserExists(string specEmail)
        => await userManager.FindByEmailAsync(specEmail) != null;
}

public class EventUserCreateSpec
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public long AgentAccountId { get; set; }
    public long RebateDirectSchemaId { get; set; }
    public string TradeAccountGroup { get; set; } = null!;
    public string NativeName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Ccc { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Comment { get; set; } = string.Empty;
}