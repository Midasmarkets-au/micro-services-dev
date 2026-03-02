using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M = Bacera.Gateway.Account;
using MSG = Bacera.Gateway.ResultMessage.Account;

namespace Bacera.Gateway.Web.Areas.Agent.Controllers;

[Tags("IB/Account")]
public class AccountController(
    TradingService tradingService,
    TenantDbContext tenantDbContext,
    IBackgroundJobClient client,
    ITenantGetter tenantGetter,
    UserService userSvc,
    ConfigurationService configurationService,
    IGeneralJob generalJob,
    AccountManageService accManSvc)
    : AgentBaseController
{
    private long TenantId => tenantGetter.GetTenantId();
    /// <summary>
    /// Account pagination for current IB
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ClientResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index(long agentUid, [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.ParentAccountUid = agentUid;

        var parentLevel = await tenantDbContext.Accounts
            .Where(x => x.Uid == agentUid)
            .Select(x => x.Level)
            .FirstOrDefaultAsync();
        if (criteria.RelativeLevel != null)
            criteria.Level = criteria.RelativeLevel + parentLevel;

        var hideUserEmail = criteria.ChildParentAccountUid != null;
        return Ok(await tradingService.AccountQueryForParentAsync(criteria, GetPartyId(), parentLevel, hideUserEmail));
    }

    /// <summary>
    /// Get account by uid for current IB
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}")]
    [ProducesResponseType(typeof(Result<M.ClientResponseModel, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(long agentUid, long uid)
    {
        var clientAccount = await tradingService.AccountLookupForParentAsync(agentUid, uid);
        if (clientAccount.IsEmpty()) return NotFound();

        return Ok(await tradingService.AccountGetForPartyAsync(clientAccount.Uid, clientAccount.PartyId));
    }

    /// <summary>
    /// Refresh Trade Account Status for current IB
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}/refresh")]
    [ProducesResponseType(typeof(M.ClientResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefreshTradeAccountStatus(long agentUid, long uid)
    {
        var clientAccount = await tradingService.AccountLookupForParentAsync(agentUid, uid);
        if (clientAccount.IsEmpty()) return NotFound();

        await Task.WhenAll(
            generalJob.TryUpdateTradeAccountStatus(TenantId, clientAccount.Id, true),
            accManSvc.UpdateAccountSearchText(clientAccount.Id)
        );

        var item = await tradingService.AccountGetForPartyAsync(clientAccount.Uid, clientAccount.PartyId);
        return item.IsEmpty() ? NotFound() : Ok(item);
    }


    [HttpGet("child/stat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> NetAmountOfChildStatByUid(long agentUid,
        [FromQuery] long uid,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool viewClient = false)
    {
        var path = await tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .Select(x => x.ReferPath)
            .SingleOrDefaultAsync();
        if (path == null || !path.Contains(agentUid.ToString())) return Ok(new List<M.ChildNetStatAmountResponseModel>());
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
    public async Task<IActionResult> RebateSymbolGroupedStat(long agentUid,
        [FromQuery] long uid,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var path = await tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .Select(x => x.ReferPath)
            .SingleOrDefaultAsync();
        if (path == null || !path.Contains(agentUid.ToString())) return Ok(new List<M.ChildNetStatAmountResponseModel>());

        var result = await tradingService.GetChildAccountRebateSymbolGroupedStatByUid(uid, from, to);
        return Ok(result);
    }

    // [HttpGet("child/stat/trade/symbol-grouped")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<IActionResult> RebateTradeGroupedStat(long agentUid,
    //     [FromQuery] long uid,
    //     [FromQuery] DateTime? from,
    //     [FromQuery] DateTime? to)
    // {
    //     var path = await _tenantDbContext.Accounts
    //         .Where(x => x.Uid == uid)
    //         .Select(x => x.ReferPath)
    //         .SingleOrDefaultAsync();
    //     if (path == null) return Ok(new { });
    //     if (!path.Contains(agentUid.ToString()) || !path.StartsWith(agentUid.ToString()))
    //         return Ok(new { });
    //     var result = await _tradingSvc.GetChildAccountTradeSymbolGroupedStatByUid(uid, from, to);
    //     return Ok(result);
    // }

    /// <summary>
    /// Get Agent Default Level Setting
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}/default-level-setting")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(Dictionary<int, List<RebateAgentRule.DefaultLevelSetting>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefaultLevelSetting(long uid)
    {
        var account = await tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
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

    [HttpGet("{uid:long}/view-email-code")]
    public async Task<IActionResult> ViewEmailCode(long agentUid, long uid)
    {
        var isAccountUnder = await tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .Where(x => x.ReferPath.Contains(agentUid.ToString()))
            .AnyAsync();
        if (!isAccountUnder) return NotFound("ACCOUNT_NOT_FOUND_UNDER");

        var account = await tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .Select(x => new { x.Party.Email, x.ReferPath })
            .SingleAsync();

        var parentUids = account.ReferPath.Split('.')
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(long.Parse)
            .ToList();

        if (parentUids.Count < 2 || parentUids[^2] != agentUid)
            return BadRequest("NOT_DIRECT_CHILD");

        var partyId = GetPartyId();
        var exist = await tenantDbContext.AuthCodes
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Event == $"{AuthCode.EventLabel.ParentViewEmail}:{account.Email}")
            .Where(x => x.ExpireOn > DateTime.UtcNow)
            .AnyAsync();
        if (exist) return BadRequest("CODE_ALREADY_SENT");

        var user = await userSvc.GetPartyAsync(partyId);

        client.Enqueue<IGeneralJob>(x => x.GenerateAuthCodeAndSendEmailAsync(
            TenantId, user.EmailRaw, $"{AuthCode.EventLabel.ParentViewEmail}:{account.Email}"));

        return Ok();
    }

    [HttpGet("{uid:long}/view-email/{code}")]
    public async Task<IActionResult> ViewEmail(long agentUid, long uid, string code)
    {
        var isAccountUnder = await tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .Where(x => x.ReferPath.Contains(agentUid.ToString()))
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