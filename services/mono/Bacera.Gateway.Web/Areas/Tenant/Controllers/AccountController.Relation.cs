using System.Data.SqlClient;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.EventHandlers;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

public partial class AccountController
{
    [HttpGet("{id:long}/parent-accounts")]
    public async Task<ActionResult> GetParentAccounts(long id)
    {
        var hideEmail = ShouldHideEmail();
        return Ok(await tradingService.ParentAccountsGetForTenantAsync(id, hideEmail));
    }
        

    /// <summary>
    /// Change account's sales group
    /// </summary>
    /// <param name="id"></param>
    /// <param name="salesId"></param>
    /// <returns></returns>
    [HttpPost("{id:long}/change/sales/{salesId:long}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeSalesGroup(long id, long salesId)
    {
        var (result, message) = await tradingService.ChangeSalesGroupAsync(id, salesId, GetPartyId());

        if (!result) return BadRequest(Result.Error(message));

        await mediator.Publish(new AccountUpdatedEvent(id));
        return NoContent();
    }

    /// <summary>
    /// Change account's agent group
    /// </summary>
    /// <param name="id"></param>
    /// <param name="agentId"></param>
    /// <returns></returns>
    [HttpPost("{id:long}/change/agent/{agentId:long}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeAgentGroup(long id, long agentId)
    {
        var (result, message) = await tradingService.ChangeAgentGroupAsync(id, agentId, GetPartyId());

        if (!result) return BadRequest(Result.Error(message));

        await mediator.Publish(new AccountUpdatedEvent(id));
        return NoContent();
    }

    /// <summary>
    /// Remove account from an agent group
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id:long}/remove/agent/{agentId:long}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveAgentGroup(long id)
    {
        var (result, message) = await tradingService.RemoveFromAgentGroupAsync(id);

        if (!result) return BadRequest(Result.Error(message));

        await mediator.Publish(new AccountUpdatedEvent(id));
        return NoContent();
    }

    /// <summary>
    /// Change Agent Group Name and all child group name
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("{id:long}/change/agent-group-name")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeAgentGroupName(long id, [FromBody] Account.UpdateGroupName spec)
    {
        if (await tenantCtx.Accounts.AnyAsync(x => x.Group == spec.GroupName))
            return BadRequest(Result.Error("__AGENT_GROUP_NAME_ALREADY_EXISTS__"));

        var agentAccount = await tenantCtx.Accounts
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();
        if (agentAccount == null) return BadRequest(Result.Error("__AGENT_NOT_FOUND__"));

        var childAccounts = await tenantCtx.Accounts
            .Where(x => x.ReferPath.StartsWith(agentAccount.ReferPath))
            .OrderBy(x => x.Level)
            .Select(x => new { x.Id, x.Group })
            .ToListAsync();

        await tenantCtx.Database.ExecuteSqlRawAsync(
            """
            UPDATE trd."_Account" SET "Group" = replace("Group", {0}, {1}), "SearchText" = replace("SearchText", {0}, {1}) WHERE "Id" = ANY({2})
            """,
            agentAccount.Group,
            spec.GroupName,
            childAccounts.Select(x => x.Id).ToArray()
        );


        try
        {
            var accountLogs = childAccounts.Select(x => Account.BuildLog(
                x.Id, GetPartyId(), "ChangeAgentGroupName", x.Group,
                string.Concat(spec.GroupName, x.Group.AsSpan(Math.Min(spec.GroupName.Length, x.Group.Length - 1)))));
            tenantCtx.AccountLogs.AddRange(accountLogs);
            await tenantCtx.SaveChangesAsync();
        }
        catch
        {
            // ignored
        }

        return Ok();
    }

    /// <summary>
    /// Get group name list
    /// </summary>
    /// <returns></returns>
    [HttpGet("group/name-list")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupNameList([FromQuery] AccountGroupTypes? type,
        [FromQuery] string keywords = "")
    {
        var role = type == null ? (AccountRoleTypes?)null : (AccountRoleTypes)((int)type * 100);
        var items = await tradingService.GetAllGroupNamesAsync(role, keywords);
        return Ok(items);
    }

    [HttpGet("{id:long}/stat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetParentAccountStat(long id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        long timezoneOffsetInHour = 0)
    {
        var exist = await tenantCtx.Accounts.AnyAsync(x => x.Id == id && x.AccountStats.Any());
        if (!exist) return NotFound("__ACCOUNT_NOT_FOUND__");
        // convert to UTC
        from = from?.AddHours(-timezoneOffsetInHour).Date;
        to = to?.AddHours(-timezoneOffsetInHour).Date;

        // if to is before today
        if (to < DateTime.UtcNow.Date)
        {
            var result = await tradingService.GetParentAccountStatAsync(id, from, to);
            return Ok(result);
        }

        var beforeResult = await tradingService.GetParentAccountStatAsync(id, from, DateTime.UtcNow.Date.AddDays(-1));

        // get today's data
        var (todayFrom, todayTo) = Utils.CalculateTradePeriod("today");

        return Ok(await tradingService.GetParentAccountStatAsync(id, from, to));
    }

    [HttpGet("{id:long}/child/stat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> NetAmountOfChildStat(long id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool viewClient = false)
        => Ok(await tradingService.GetDirectChildNetAmountForAccountById(id, from, to, viewClient));

    [HttpGet("child/stat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> NetAmountOfChildStatByUid([FromQuery] long uid,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool viewClient = false)
        => Ok(await tradingService.GetDirectChildNetAmountForAccountByUid(uid, from, to, viewClient));

    [HttpGet("child/stat/rebate/symbol-grouped")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RebateSymbolGroupedStat([FromQuery] long uid,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
        => Ok(await tradingService.GetChildAccountRebateSymbolGroupedStatByUid(uid, from, to));

    // [HttpGet("child/stat/trade/symbol-grouped")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<IActionResult> RebateTradeGroupedStat([FromQuery] long uid,
    //     [FromQuery] DateTime? from,
    //     [FromQuery] DateTime? to)
    //     => Ok(await _tradingSvc.GetChildAccountTradeSymbolGroupedStatByUid(uid, from, to));
}