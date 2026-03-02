using Bacera.Gateway.Agent;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using HashidsNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static Bacera.Gateway.Web.Areas.Tenant.Controllers.AccountController;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

[Tags("Sales/Referral Code")]
public class ReferralController(
    TenantDbContext tenantCtx,
    TradingService tradingSvc,
    ReferralCodeService referralCodeSvc,
    CentralDbContext centralDbContext,
    ConfigService configSvc,
    ILogger<ReferralController> logger)
    : SalesBaseController
{
    private const int MaxCodeCount = 100;

    /// <summary>
    /// Referral Code Pagination
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<ReferralCode>, ReferralCode.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index(long salesUid, [FromQuery] ReferralCode.Criteria? criteria = null)
    {
        criteria ??= new ReferralCode.Criteria();
        criteria.ParentAccountUid = salesUid;
        if (criteria.ChildAccountUid != null)
        {
            var childLevel = await tenantCtx.Accounts
                .Where(x => x.Uid == criteria.ChildAccountUid)
                .Select(x => x.Level)
                .FirstOrDefaultAsync();
            criteria.Level = childLevel;
        }

        var items = await tenantCtx.ReferralCodes
            .PagedFilterBy(criteria)
            .ToListAsync();
        return Ok(Result<List<ReferralCode>, ReferralCode.Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// Get Referral History
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(Result<List<Referral>, Referral.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> History(long salesUid, [FromQuery] Referral.Criteria? criteria = null)
    {
        var account = await tenantCtx.Accounts.FirstAsync(x => x.Uid == salesUid);
        criteria ??= new Referral.Criteria();
        criteria.ReferrerPartyId = GetPartyId();
        criteria.ReferrerAccountId = account.Id;
        var items = await tenantCtx.Referrals
            .PagedFilterBy(criteria)
            .ToListAsync();
        return Ok(Result<List<Referral>, Referral.Criteria>.Of(items, criteria));
    }

    [HttpGet("user-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UnfinishedVerification(long salesUid,
        [FromQuery] Referral.Criteria? criteria = null)
    {
        criteria ??= new Referral.Criteria();
        var result = await tradingSvc.QueryUnfinishedReferredUserAsync(salesUid, criteria);
        return Ok(result);
    }

    /// <summary>
    /// Create Referral Code for Broker(Top-Agent)
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("top-agent")]
    [ProducesResponseType(typeof(ReferralCode), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateForBroker(long salesUid, [FromBody] ReferralCode.AgentCreateSpec spec)
    {
        var rebateCategoryIds = await tenantCtx.Symbols
            .ToRebateCategories(300)
            .Select(x => x.Key)
            .ToListAsync();
        foreach (var schema in spec.Schema)
        {
            foreach (var schemaItem in schema.Items)
            {
                if (!rebateCategoryIds.Contains(schemaItem.CategoryId))
                {
                    return BadRequest(Result.Error(ResultMessage.RebateRule.SymbolCategoryIdNotExists,
                        new { schemaItem.CategoryId }));
                }
            }

            // sorting and add default zero value
            // if (schema.AllowPips.Count == 0) schema.AllowPips.Add(0);
            // if (schema.AllowCommissions.Count == 0) schema.AllowCommissions.Add(0);
            schema.AllowPips = schema.AllowPips.OrderBy(x => x).Distinct().ToList();
            schema.AllowCommissions = schema.AllowCommissions.OrderBy(x => x).Distinct().ToList();
        }

        // if (spec.DistributionType == RebateDistributionTypes.LevelPercentage)
        // {
        //     var total = spec.PercentageSchema.LevelAmount.Sum();
        //     spec.PercentageSchema.LevelAmount = spec.PercentageSchema.LevelAmount.Select(x => x / total).ToList();
        // }

        // Get account info
        var account = await tenantCtx.Accounts.FirstAsync(x => x.Uid == salesUid);
        if (account.Role != (int)AccountRoleTypes.Sales)
            return BadRequest(Result.Error(ResultMessage.Common.SalesUidNotFound));

        // Check if the number of referral codes has reached the maximum for Party and Account
        if (MaxCodeCount <= await tenantCtx.ReferralCodes
                .Where(x => x.PartyId == GetPartyId())
                .Where(x => x.AccountId == account.Id)
                .CountAsync())
            return BadRequest(Result.Error(ResultMessage.Referral.YouHaveReachedTheMaximumNumberOfReferralCodes));

        spec.SiteId = (SiteTypes)account.SiteId;
        var item = new ReferralCode
        {
            Name = spec.Name,
            Code = Guid.NewGuid().ToString(),
            PartyId = GetPartyId(),
            AccountId = account.Id,
            ServiceType = (int)ReferralServiceTypes.Broker,
            Summary = Utils.JsonSerializeObject(spec),
        };

        await tenantCtx.ReferralCodes.AddAsync(item);
        await tenantCtx.SaveChangesAsync();

        var hashids = new Hashids("BCRReferralCode", 3, "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789");
        var code = hashids.Encode((int)item.Id);
        item.Code = "RSA" + code + code.Length + Tenancy.GetTenancyInReferCode(GetTenantId());
        await tenantCtx.SaveChangesAsync();
        centralDbContext.CentralReferralCodes.Add(item.ToCentralReferralCode(GetTenantId()));
        await centralDbContext.SaveChangesAsync();

        return Ok(item);
    }

    /// <summary>
    /// Create Agent Referral Code for a child agent's
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="agentUid"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("agent/{agentUid:long}/agent")]
    [ProducesResponseType(typeof(ReferralCode), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateForAgent(long salesUid, long agentUid,
        [FromBody] ReferralCode.AgentCreateSpec spec)
    {
        var account = await tenantCtx.Accounts
            .Where(x => x.Uid == agentUid)
            .Where(x => x.ReferPath.Contains(salesUid.ToString()))
            .Select(x => new { x.RebateAgentRule })
            .FirstOrDefaultAsync();

        if (account == null) return BadRequest(Result.Error(ResultMessage.Common.AgentUidNotFound));


        var levelSetting = account.RebateAgentRule?.GetLevelSetting();
        if (levelSetting is { DistributionType: RebateDistributionTypes.LevelPercentage })
        {
            return BadRequest(Result.Error("Agent's rebate rule is not supported"));
        }

        var (referralCode, result) =
            await tradingSvc.TryCreateReferralCodeForAgent(agentUid, spec, AccountRoleTypes.Sales, GetPartyId(),
                GetTenantId());

        if (referralCode == null)
            return BadRequest(result);

        // If IsAutoCreatePaymentMethod is true, then
        // Create DefaultAutoCreatePaymentMethod configuration with RowId == this referral code
        // Value copied from default site configuration (RowId == 0)
        if (spec.IsAutoCreatePaymentMethod == 1)
        {
            await tradingSvc.CreateDefaultAutoCreatePaymentMethodConfigWithReferralCodeRowIdAsync(referralCode.Id);
            await tradingSvc.CreateDefaultAutoCreateWithdrawalPaymentMethodConfigWithReferralCodeRowIdAsync(referralCode.Id);
        }

        return Ok(referralCode);
    }

    /// <summary>
    /// Create Client Referral Code for child agent's
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="agentUid"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("agent/{agentUid:long}/client")]
    [ProducesResponseType(typeof(ReferralCode), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateForAgentsClient(long salesUid, long agentUid,
        [FromBody] ReferralCode.ClientCreateSpec spec)
    {
        var account = await tenantCtx.Accounts
            .Where(x => x.Uid == agentUid)
            .Where(x => x.ReferPath.Contains(salesUid.ToString()))
            .Include(x => x.RebateAgentRule)
            .FirstOrDefaultAsync();
        if (account == null)
            return BadRequest(Result.Error(ResultMessage.Common.AgentUidNotFound));

        if (account.Role != (int)AccountRoleTypes.Agent)
            return BadRequest(Result.Error(ResultMessage.Account.AccountIsNotAnAgent));

        var levelSetting = account.RebateAgentRule?.GetLevelSetting();
        if (levelSetting is { DistributionType: RebateDistributionTypes.LevelPercentage })
        {
            return BadRequest(Result.Error("Agent's rebate rule is not supported"));
        }

        // Check if the number of referral codes has reached the maximum for Party and Account
        if (MaxCodeCount <= await tenantCtx.ReferralCodes
                .Where(x => x.PartyId == account.PartyId)
                .Where(x => x.AccountId == account.Id)
                .CountAsync())
            return BadRequest(Result.Error(ResultMessage.Referral.YouHaveReachedTheMaximumNumberOfReferralCodes));
        spec.SiteId = (SiteTypes)account.SiteId;
        var item = new ReferralCode
        {
            Name = spec.Name,
            Code = Guid.NewGuid().ToString(),
            PartyId = account.PartyId,
            AccountId = account.Id,
            ServiceType = (int)ReferralServiceTypes.Client,
            Summary = Utils.JsonSerializeObject(spec),
            IsAutoCreatePaymentMethod = spec.IsAutoCreatePaymentMethod,
        };

        await tenantCtx.ReferralCodes.AddAsync(item);
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId());

        var hashids = new Hashids("BCRReferralCode", 3, "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789");
        var code = hashids.Encode((int)item.Id);
        item.Code = "RSC" + code + code.Length + Tenancy.GetTenancyInReferCode(GetTenantId());
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId());

        centralDbContext.CentralReferralCodes.Add(item.ToCentralReferralCode(GetTenantId()));
        await centralDbContext.SaveChangesAsync();

        // If IsAutoCreatePaymentMethod is true, then
        // Create DefaultAutoCreatePaymentMethod configuration with RowId equals this referral code
        // Value copied from default site configuration (RowId == 0)
        if (spec.IsAutoCreatePaymentMethod == 1)
        {
            await tradingSvc.CreateDefaultAutoCreatePaymentMethodConfigWithReferralCodeRowIdAsync(item.Id);
            await tradingSvc.CreateDefaultAutoCreateWithdrawalPaymentMethodConfigWithReferralCodeRowIdAsync(item.Id);
        }

        return Ok(item);
    }

    /// <summary>
    /// Create Referral Code
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("client")]
    [ProducesResponseType(typeof(ReferralCode), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateForClient(long salesUid, [FromBody] ReferralCode.ClientCreateSpec spec)
    {
        var partyId = GetPartyId();
        var account = await tenantCtx.Accounts.FirstAsync(x => x.Uid == salesUid);
        if (account.Role != (int)AccountRoleTypes.Sales)
            return BadRequest(Result.Error(ResultMessage.Common.SalesUidNotFound));

        // Check if the number of referral codes has reached the maximum for Party and Account
        if (MaxCodeCount <= await tenantCtx.ReferralCodes
                .Where(x => x.PartyId == partyId)
                .Where(x => x.AccountId == account.Id)
                .CountAsync())
            return BadRequest(Result.Error(ResultMessage.Referral.YouHaveReachedTheMaximumNumberOfReferralCodes));

        spec.SiteId = (SiteTypes)account.SiteId;
        var item = new ReferralCode
        {
            Name = spec.Name,
            Code = Guid.NewGuid().ToString(),
            PartyId = GetPartyId(),
            AccountId = account.Id,
            ServiceType = (int)ReferralServiceTypes.Client,
            Summary = Utils.JsonSerializeObject(spec),
            IsAutoCreatePaymentMethod = spec.IsAutoCreatePaymentMethod,
        };

        await tenantCtx.ReferralCodes.AddAsync(item);
        await tenantCtx.SaveChangesAsync();

        var hashids = new Hashids("BCRReferralCode", 3, "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789");
        var code = hashids.Encode((int)item.Id);
        item.Code = "RSC" + code + code.Length + Tenancy.GetTenancyInReferCode(GetTenantId());
        await tenantCtx.SaveChangesAsync();
        centralDbContext.CentralReferralCodes.Add(item.ToCentralReferralCode(GetTenantId()));
        await centralDbContext.SaveChangesAsync();

        // If IsAutoCreatePaymentMethod is true, then
        // Create DefaultAutoCreatePaymentMethod configuration with RowId equals this referral code
        // Value copied from default site configuration (RowId == 0)
        if (spec.IsAutoCreatePaymentMethod == 1)
        {
            await tradingSvc.CreateDefaultAutoCreatePaymentMethodConfigWithReferralCodeRowIdAsync(item.Id);
            await tradingSvc.CreateDefaultAutoCreateWithdrawalPaymentMethodConfigWithReferralCodeRowIdAsync(item.Id);
        }
        
        return Ok(item);
    }

    /// <summary>
    /// Get referral code whit supplement
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpGet("code/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReferralCode.ResponseModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(long salesUid, string code)
    {
        var referralCode = await tenantCtx.ReferralCodes
            .Where(x => x.Code == code.Trim().ToUpper())
            .Where(x => x.PartyId == GetPartyId())
            .ToClientResponse()
            .SingleOrDefaultAsync();
        if (referralCode == null)
            return NotFound();

        return Ok(referralCode);
    }

    /// <summary>
    /// Update Referral Code for a child agent's
    /// </summary>
    /// <param name="salesUid"></param>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("code/{id:long}")]
    // [HttpPut("code/{id:long}/status")]
    [ProducesResponseType(typeof(ReferralCode), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateForAgent(long salesUid, long id, [FromBody] ReferralCode.UpdateSpec spec)
    {
        var referCode = await tenantCtx.ReferralCodes
            .Where(x => x.Id == id)
            .Where(x => x.Account.ReferPath.Contains(salesUid.ToString()))
            .SingleOrDefaultAsync();

        if (referCode == null)
        {
            return BadRequest(Result.Error(ResultMessage.Common.ActionNotAllow));
        }

        referCode.Status = spec.Status;
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
    /// <param name="salesUid"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpPut("code/{code}/default-client")]
    public async Task<IActionResult> SetDefaultClient(long salesUid, string code)
    {
        var agentAccountId = await tenantCtx.ReferralCodes
            .Where(x => x.Code == code)
            .Where(x => x.Account.ReferPath.Contains(salesUid.ToString()))
            .Select(x => x.AccountId)
            .SingleOrDefaultAsync();
        if (agentAccountId == 0) return BadRequest(Result.Error(ResultMessage.Common.ActionNotAllow));

        var result = await referralCodeSvc.SetAgentDefaultClientReferralCodeAsync(agentAccountId, code);
        return result ? Ok() : BadRequest();
    }
}