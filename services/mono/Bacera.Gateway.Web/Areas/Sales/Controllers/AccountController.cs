using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.ViewModels.Parent;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

using M = Account;

[Tags("Sales/Account")]
public class AccountController(
    TradingService tradingService,
    TenantDbContext tenantDbContext,
    AccountManageService accManageSvc,
    IBackgroundJobClient client,
    UserService userSvc,
    ConfigurationService configurationService)
    : SalesBaseController
{
    /// <summary>
    /// Account pagination for Sales
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Index(long salesUid, [FromQuery] M.SalesCriteria? criteria)
    {
        criteria ??= new M.SalesCriteria();
        var salesId = await accManageSvc.GetAccountIdByUidAsync(salesUid);
        var items = await accManageSvc.QueryAccountForSalesAsync(salesId, criteria);
        var result = Result<List<AccountForSalesViewModel>, M.SalesCriteria>.Of(items, criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get account by uid for Sales
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}")]
    [ProducesResponseType(typeof(Result<M.ClientResponseModel, M.Criteria>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long salesUid, long uid)
    {
        var clientAccount = await tradingService.AccountLookupForParentAsync(salesUid, uid);
        if (clientAccount.IsEmpty()) return NotFound();

        return Ok(await tradingService.AccountGetForPartyAsync(clientAccount.Uid, clientAccount.PartyId));
    }

    [HttpGet("{childAccountUid:long}/level-account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLevelAccountsInBetween(long salesUid, long childAccountUid)
    {
        var items = await accManageSvc.GetParentLevelAccountAsync(salesUid, childAccountUid);
        return Ok(items);
    }

    [HttpGet("{childAccountUid:long}/stat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetParentAccountStat(long salesUid, long childAccountUid,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var childAccountId = await tenantDbContext.Accounts
            .Where(x => x.Uid == childAccountUid && x.ReferPath.Contains(salesUid.ToString()))
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
        if (childAccountId == 0) return NotFound("__ACCOUNT_NOT_FOUND__");

        var result = await tradingService.GetParentAccountStatAsync(childAccountId, from, to);
        return Ok(result);
    }

    [HttpGet("child/stat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> NetAmountOfChildStatByUid(long salesUid,
        [FromQuery] long uid,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool viewClient = false)
    {
        var path = await tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .Select(x => x.ReferPath)
            .SingleOrDefaultAsync();
        if (path == null || !path.Contains(salesUid.ToString())) return Ok(new List<M.ChildNetStatAmountResponseModel>());

        var result = await tradingService.GetDirectChildNetAmountForAccountByUid(uid, from, to, viewClient);
        var childSumUp = new Account.ChildNetStatAmountResponseModel
        {
            Id = 0,
            Uid = 0,
            Group = "Child-Sum-Up",
            Role = 0,
            DepositAmounts = result.Where(x => x.Uid != 0)
                .SelectMany(x => x.DepositAmounts)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.Sum(kvp => kvp.Value)),

            WithdrawalAmounts = result.Where(x => x.Uid != 0)
                .SelectMany(x => x.WithdrawalAmounts)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.Sum(kvp => kvp.Value)),

            RebateAmounts = result.Where(x => x.Uid != 0)
                .SelectMany(x=> x.RebateAmounts)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.Sum(kvp => kvp.Value)),
        };
        
        return Ok(childSumUp);
    }

    [HttpGet("child/stat/rebate/symbol-grouped")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateSymbolGroupedStat(long salesUid,
        [FromQuery] long uid,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var path = await tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .Select(x => x.ReferPath)
            .SingleOrDefaultAsync();
        if (path == null || !path.Contains(salesUid.ToString())) return Ok(new List<M.ChildNetStatAmountResponseModel>());
        
        var result = await tradingService.GetChildAccountRebateSymbolGroupedStatByUid(uid, from, to);
        return Ok(result);
    }

    [HttpGet("child/stat/trade/symbol-grouped")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateTradeGroupedStat(long salesUid,
        [FromQuery] long uid,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var path = await tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .Select(x => x.ReferPath)
            .SingleOrDefaultAsync();
        if (path == null || !path.Contains(salesUid.ToString())) return Ok(new { });
        var result = await tradingService.GetChildAccountTradeSymbolGroupedStatByUid(uid, from, to);
        return Ok(result);
    }

    /// <summary>
    /// Get IB Account Configuration (Default Level Setting)
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="agentUid"></param>
    /// <returns></returns>
    [HttpGet("configuration/{agentUid:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAgentConfiguration(long salesUid, long agentUid)
    {
        var agentAccount = await tenantDbContext.Accounts
            .Where(x => x.Uid == agentUid)
            .SingleOrDefaultAsync();
        if (agentAccount == null) return NotFound();

        var configurations = await tenantDbContext.Configurations
            .Where(x => x.Category == nameof(Account))
            .Where(x => x.RowId == agentAccount.Id)
            .ToListAsync();

        return Ok(configurations);
    }

    /// <summary>
    /// Get Agent Default Level Setting
    /// </summary>
    /// <param name="agentUid"></param>
    /// <returns></returns>
    [HttpGet("{agentUid:long}/default-level-setting")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(Dictionary<int, List<RebateAgentRule.DefaultLevelSetting>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefaultLevelSetting(long agentUid)
    {
        var account = await tenantDbContext.Accounts
            .Where(x => x.Uid == agentUid)
            .FirstOrDefaultAsync();

        if (account == null)
            return NotFound();

        var item = await tenantDbContext.Configurations
            .Where(x => x.Category == nameof(Account))
            .Where(x => x.RowId == account.Id)
            .Where(x => x.Key == "DefaultRebateLevelSetting")
            .ToTenantViewModel()
            .FirstOrDefaultAsync();

        var obj = item?.Value;
        if (obj != null) return Ok(obj);

        var result = await configurationService.GetDefaultRebateLevelSettingAsync(account.SiteId);
        return Ok(result);
    }

    /// <summary>
    /// Get Account Config - Available Account Type
    /// </summary>
    /// <param name="salesUid"></param>
    /// <returns></returns>
    [HttpGet("configuration/account-type-available")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountTypeAvailable(long salesUid)
    {
        var account = await tenantDbContext.Accounts
            .Where(x => x.Uid == salesUid)
            .FirstOrDefaultAsync();

        if (account == null)
            return NotFound();

        var item = await tenantDbContext.Configurations
            .Where(x => x.Category == nameof(Account))
            .Where(x => x.RowId == account.Id)
            .Where(x => x.Key == "AccountTypeAvailable")
            .ToTenantViewModel()
            .FirstOrDefaultAsync();

        var obj = item?.Value;
        if (obj != null) return Ok(obj);

        var result = await configurationService.GetAccountTypeAvailableAsync(account.SiteId);
        return Ok(result);
    }

    /// <summary>
    /// Get account referral path by uid for Sales
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("referralPath/{uid:long}")]
    [ProducesResponseType(typeof(Result<M.ClientResponseModel, M.Criteria>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReferralPath(long salesUid, long uid)
    {
        var referPath = await tenantDbContext.Accounts
            .Where(x => x.ReferPath.Contains(salesUid.ToString()))
            .Where(x => x.Uid == uid)
            .Select(x => x.ReferPath)
            .FirstOrDefaultAsync();
        if (referPath == null) return NotFound("__ACCOUNT_NOT_FOUND__");

        var referPathUids = referPath.Split('.', StringSplitOptions.RemoveEmptyEntries)
            .Select(long.Parse)
            .ToList();

        var results = await tenantDbContext.Accounts
            .Where(x => referPathUids.Contains(x.Uid))
            .Where(x => x.Role == (short)AccountRoleTypes.Agent)
            .OrderBy(x => x.Level)
            .Select(x => x.Uid)
            .ToListAsync();

        return Ok(results);
    }
    
    [HttpGet("{uid:long}/view-email-code")]
    public async Task<IActionResult> ViewEmailCode(long salesUid, long uid)
    {
        var isAccountUnder = await tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .Where(x => x.ReferPath.Contains(salesUid.ToString()))
            .AnyAsync();
        if (!isAccountUnder) return NotFound("ACCOUNT_NOT_FOUND_UNDER");

        var account = await tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .Select(x => new { x.Party.Email })
            .SingleAsync();

        var partyId = GetPartyId();
        var exist = await tenantDbContext.AuthCodes
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Event == $"{AuthCode.EventLabel.ParentViewEmail}:{account.Email}")
            .Where(x => x.ExpireOn > DateTime.UtcNow)
            .AnyAsync();
        if (exist) return BadRequest("CODE_ALREADY_SENT");

        var user = await userSvc.GetPartyAsync(partyId);
        var tenantId = GetTenantId();
        client.Enqueue<IGeneralJob>(x => x.GenerateAuthCodeAndSendEmailAsync(
            tenantId, user.EmailRaw, $"{AuthCode.EventLabel.ParentViewEmail}:{account.Email}"));

        return Ok();
    }

    [HttpGet("{uid:long}/view-email/{code}")]
    public async Task<IActionResult> ViewEmail(long salesUid, long uid, string code)
    {
        var isAccountUnder = await tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .Where(x => x.ReferPath.Contains(salesUid.ToString()))
            .AnyAsync();
        if (!isAccountUnder) return NotFound("ACCOUNT_NOT_FOUND_UNDER");

        var account = await tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .Select(x => new { x.Party.Email })
            .SingleAsync();

        var partyId = GetPartyId();

        var item = await tenantDbContext.AuthCodes
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Event == $"{AuthCode.EventLabel.ParentViewEmail}:{account.Email}")
            .Where(x => x.ExpireOn > DateTime.UtcNow)
            .Where(x => x.Code == code)
            .FirstOrDefaultAsync();
        if (item == null) return NotFound("CODE_NOT_FOUND");

        return Ok(account.Email);
    }
}