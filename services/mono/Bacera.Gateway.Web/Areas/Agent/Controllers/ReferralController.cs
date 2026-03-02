using System.Text.RegularExpressions;
using Bacera.Gateway.Agent;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.ViewModels.Parent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using HashidsNet;

namespace Bacera.Gateway.Web.Areas.Agent.Controllers;

using M = ReferralCode;
using MSG = ResultMessage.Referral;

[Tags("IB/Referral")]
public class ReferralController(
    TenantDbContext tenantCtx,
    TradingService tradingService,
    AccountManageService accountManageSvc,
    ReferralCodeService referralCodeSvc)
    : AgentBaseController
{
    /// <summary>
    /// Referral Code pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <param name="agentUid"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index(long agentUid, [FromQuery] M.Criteria? criteria = null)
    {
        var account = await tenantCtx.Accounts.FirstAsync(x => x.Uid == agentUid);
        criteria ??= new M.Criteria();
        criteria.PartyId = GetPartyId();
        criteria.AccountId = account.Id;
        var items = await tenantCtx.ReferralCodes
            .PagedFilterBy(criteria)
            .ToResponse()
            .ToListAsync();
        return Ok(Result<List<M.ResponseModel>, M.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Referral History pagination
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(Result<List<Referral>, Referral.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> History(long agentUid, [FromQuery] Referral.Criteria? criteria = null)
    {
        var account = await tenantCtx.Accounts.FirstAsync(x => x.Uid == agentUid);
        criteria ??= new Referral.Criteria();
        criteria.ReferrerPartyId = GetPartyId();
        criteria.ReferrerAccountId = account.Id;
        var items = await tenantCtx.Referrals
            .PagedFilterBy(criteria)
            .ToListAsync();
        return Ok(Result<List<Referral>, Referral.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Get referral code whit supplement
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpGet("{code}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(M.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var referralCode = await tenantCtx.ReferralCodes
            .Where(x => x.Code == code.Trim())
            .Where(x => x.PartyId == GetPartyId())
            .SingleOrDefaultAsync();
        if (referralCode == null)
            return NotFound();

        var supplement = await tenantCtx.Supplements
            .Where(x => x.Type == (int)SupplementTypes.ReferralCode)
            .Where(x => x.RowId == referralCode.Id)
            .FirstOrDefaultAsync();

        var response = referralCode.ToClientResponse();

        if (supplement != null)
            response.SupplementJson = supplement.Data;

        return Ok(response);
    }

    /// <summary>
    /// Create Referral Code
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("ib")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateForAgent(long agentUid, [FromBody] M.AgentCreateSpec spec)
    {
        var (referralCode, result) = await tradingService.TryCreateReferralCodeForAgent(agentUid, spec, AccountRoleTypes.Agent, GetPartyId(),
                GetTenantId());

        if (referralCode == null)
            return BadRequest(result);

        return Ok(referralCode);
    }

    /// <summary>
    /// Create Referral Code
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("client")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateForClient(long agentUid, [FromBody] M.ClientCreateSpec spec)
    {
        var (referralCode, result) =
            await tradingService.TryCreateReferralCodeForClient(agentUid, spec, AccountRoleTypes.Agent, GetPartyId(),
                GetTenantId());

        if (referralCode == null)
            return BadRequest(result);

        return Ok(referralCode);
    }

    /// <summary>
    /// Get unfinished user verification list under current agent uid
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("user-history")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(Result<List<ReferredUserBasicInfoForAgentViewModel>, Referral.Criteria>))]
    public async Task<IActionResult> UnfinishedVerification(long agentUid,
        [FromQuery] Referral.Criteria? criteria = null)
    {
        criteria ??= new Referral.Criteria();
        var result = await tradingService.QueryUnfinishedReferredUserAsync(agentUid, criteria);
        return Ok(result);
    }

    /// <summary>
    /// Update Referral Code for a child agent's
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="code"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("code/{code}")]
    // [HttpPut("code/{id:long}/status")]
    [ProducesResponseType(typeof(ReferralCode), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateForAgent(long agentUid, string code, [FromBody] ReferralCode.UpdateSpec spec)
    {
        var referCode = await tenantCtx.ReferralCodes
            .Where(x => x.Code == code)
            .Where(x => x.Account.Uid == agentUid)
            .SingleOrDefaultAsync();

        if (referCode == null)
        {
            return BadRequest(Result.Error(ResultMessage.Common.ActionNotAllow));
        }

        referCode.Name = spec.Name;
        var summary = JsonConvert.DeserializeObject<dynamic>(referCode.Summary);
        if (summary != null)
        {
            summary.name = referCode.Name;
        }

        referCode.Summary = Utils.JsonSerializeObject(summary);

        await tenantCtx.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Set Agent Default Client Referral Code
    /// </summary>
    /// <param name="agentUid"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpPut("code/{code}/default-client")]
    public async Task<IActionResult> SetDefaultClient(long agentUid, string code)
    {
        var accountId = await accountManageSvc.GetAccountIdByUidAsync(agentUid);
        var result = await referralCodeSvc.SetAgentDefaultClientReferralCodeAsync(accountId, code);
        return result ? Ok() : BadRequest();
    }
}