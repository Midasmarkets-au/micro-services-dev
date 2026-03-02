using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M = Bacera.Gateway.Account;

namespace Bacera.Gateway.Web.Areas.Rep.Controllers;

[Tags("Rep/Account")]
public class AccountController : RepBaseController
{
    private readonly TradingService _tradingSvc;
    private readonly TenantDbContext _tenantDbContext;

    public AccountController(TradingService tradingService, TenantDbContext tenantDbContext)
    {
        _tradingSvc = tradingService;
        _tenantDbContext = tenantDbContext;
    }

    /// <summary>
    /// Account pagination for current Rep
    /// </summary>
    /// <param name="repUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ClientResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index(long repUid, [FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.ParentAccountUid = repUid;
        var parentLevel = await _tenantDbContext.Accounts
            .Where(x => x.Uid == repUid)
            .Select(x => x.Level)
            .FirstOrDefaultAsync();
        return Ok(await _tradingSvc.AccountQueryForParentAsync(criteria, GetPartyId(), parentLevel, false));
    }

    /// <summary>
    /// Get account by uid for current Rep
    /// </summary>
    /// <param name="repUid"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("{uid:long}")]
    [ProducesResponseType(typeof(Result<M.ClientResponseModel, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(long repUid, long uid)
    {
        var clientAccount = await _tradingSvc.AccountLookupForParentAsync(repUid, uid);
        if (clientAccount.IsEmpty())
            return NotFound();

        return Ok(await _tradingSvc.AccountGetForPartyAsync(clientAccount.Uid, clientAccount.PartyId));
    }

    /// <summary>
    /// Get group name list
    /// </summary>
    /// <returns></returns>
    [HttpGet("group/name-list")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupNameList(long repUid, [FromQuery] AccountGroupTypes type,
        [FromQuery] string keywords = "")
    {
        var role = (AccountRoleTypes)((int)type * 100);
        var items = await _tradingSvc.GetAllGroupNamesUnderAccountByUid(repUid, role, keywords);
        return Ok(items);
    }


    [HttpGet("child/stat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> NetAmountOfChildStatByUid(long repUid,
        [FromQuery] long uid,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool viewClient = false)
    {
        var path = await _tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .Select(x => x.ReferPath)
            .SingleOrDefaultAsync();
        if (path == null) return Ok(new List<M.ChildNetStatAmountResponseModel>());
        if (!path.Contains(repUid.ToString()) || !path.Contains(uid.ToString()))
            return Ok(new List<M.ChildNetStatAmountResponseModel>());

        var result = await _tradingSvc.GetDirectChildNetAmountForAccountByUid(uid, from, to, viewClient);
        return Ok(result);
    }

    [HttpGet("child/stat/rebate/symbol-grouped")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateSymbolGroupedStat(long repUid,
        [FromQuery] long uid,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var path = await _tenantDbContext.Accounts
            .Where(x => x.Uid == uid)
            .Select(x => x.ReferPath)
            .SingleOrDefaultAsync();
        if (path == null) return Ok(new { });
        if (!path.Contains(repUid.ToString()) || !path.Contains(uid.ToString()))
            return Ok(new { });

        var result = await _tradingSvc.GetChildAccountRebateSymbolGroupedStatByUid(uid, from, to);
        return Ok(result);
    }

    // [HttpGet("child/stat/trade/symbol-grouped")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<IActionResult> RebateTradeGroupedStat(long repUid,
    //     [FromQuery] long uid,
    //     [FromQuery] DateTime? from,
    //     [FromQuery] DateTime? to)
    // {
    //     var path = await _tenantDbContext.Accounts
    //         .Where(x => x.Uid == uid)
    //         .Select(x => x.ReferPath)
    //         .SingleOrDefaultAsync();
    //     if (path == null) return Ok(new { });
    //     if (!path.Contains(repUid.ToString()) || !path.StartsWith(repUid.ToString()))
    //         return Ok(new { });
    //     var result = await _tradingSvc.GetChildAccountTradeSymbolGroupedStatByUid(uid, from, to);
    //     return Ok(result);
    // }
}