using OpenIddict.Validation.AspNetCore;
using System.Dynamic;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Connection;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.ViewModels.Tenant;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using M = Bacera.Gateway.Account;
using MSG = Bacera.Gateway.ResultMessage.Account;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Account")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public partial class AccountController(
    IMediator mediator,
    TradingService tradingService,
    TenantDbContext tenantCtx,
    IBackgroundJobClient backgroundJobClient,
    UserManager<User> userManager,
    AuthDbContext authDbContext,
    Tenancy tenancy,
    IServiceProvider serviceProvider,
    CentralDbContext centralCtx,
    IMyCache myCache,
    IGeneralJob generalJob,
    MyDbContextPool pool,
    AccountManageService accManSvc,
    ITradingApiService tradingApiService)
    : TenantBaseController
{
    /// <summary>
    /// Account pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [NonAction]
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<AccountViewModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        if (criteria.IncludeClosed == null && User.IsInRole(UserRoleTypesString.Compliance))
        {
            criteria.IncludeClosed = true;
        }

        var hideEmail = ShouldHideEmail();

        return Ok(await tradingService.AccountQueryForTenantAsync(criteria, GetPartyId(), hideEmail));
    }

    /// <summary>
    /// Get Account
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [NonAction]
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id)
    {
        var item = await tradingService.AccountGetAsync(id);
        return item.IsEmpty() ? NotFound() : Ok(item);
    }


    /// <summary>
    /// Get Account's Logs
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [NonAction]
    [HttpGet("log")]
    [ProducesResponseType(typeof(M.LogViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs([FromQuery] AccountLog.TenantCriteria? criteria)
    {
        criteria ??= new AccountLog.TenantCriteria();
        var items = await tenantCtx.AccountLogs
            .PagedFilterBy(criteria)
            .ToLogViewModel()
            .ToListAsync();

        return Ok(Result<List<M.LogViewModel>, AccountLog.TenantCriteria>.Of(items, criteria));
    }

    private static string GetLogActionKey(long tid) => $"account_log_action_tid:{tid}";

    [NonAction]
    [HttpGet("log-actions")]
    public async Task<IActionResult> GetLogActions()
    {
        var actions = await myCache.GetOrSetAsync(
            GetLogActionKey(tenancy.GetTenantId())
            , async () =>
            {
                var items = await tenantCtx.AccountLogs
                    .Select(x => x.Action)
                    .Distinct()
                    .ToListAsync();
                return items;
            }
            , TimeSpan.FromDays(1));

        actions = actions.Select(x => MyRegex().Replace(x, " ")).ToList();
        return Ok(actions);
    }

    /// <summary>
    /// Get Account
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [NonAction]
    [HttpGet("{id:long}/refresh")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefreshTradeAccountStatus(long id)
    {
        await Task.WhenAll(
            generalJob.TryUpdateTradeAccountStatus(GetTenantId(), id, true),
            accManSvc.UpdateAccountSearchText(id)
        );
        var item = await tradingService.AccountGetAsync(id);
        return item.IsEmpty() ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Change Account's Type
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [NonAction]
    [HttpPut("{id:long}/type")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateType(long id, [FromBody] M.UpdateType spec)
    {
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == id);
        if (account == null)
            return NotFound();

        account.AccountLogs.Add(M.BuildLog(GetPartyId(), "UpdateAccountType"
            , Enum.GetName((AccountTypes)account.Type) ?? account.Type.ToString()
            , Enum.GetName(spec.Type) ?? spec.Type.ToString()));

        account.Type = (short)spec.Type;
        account.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Accounts.Update(account);
        await tenantCtx.SaveChangesAsync();
        return Ok(account);
    }

    /// <summary>
    /// Change Account's Site
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [NonAction]
    [HttpPut("{id:long}/site")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSite(long id, [FromBody] M.UpdateSite spec)
    {
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == id);
        if (account == null)
            return NotFound();

        account.AccountLogs.Add(M.BuildLog(GetPartyId(), "UpdateAccountSite"
            , Enum.GetName((SiteTypes)account.SiteId) ?? account.SiteId.ToString()
            , Enum.GetName(spec.Site) ?? spec.Site.ToString()));

        account.SiteId = (short)spec.Site;
        account.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Accounts.Update(account);
        await tenantCtx.SaveChangesAsync();
        return Ok(account);
    }

    [NonAction]
    [HttpGet("{id:long}/referral-codes")]
    public async Task<IActionResult> GetReferralCodes(long id)
    {
        var results = await tenantCtx.ReferralCodes
            .Where(x => x.AccountId == id)
            .ToListAsync();
        return Ok(results);
    }

    /// <summary>
    /// Get account wizard status
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [NonAction]
    [HttpGet("{id:long}/wizard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetWizard(long id)
    {
        var result = await tradingService.GetAccountWizardAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Update account status
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [NonAction]
    [HttpPut("{id:long}/status/{status:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> UpdateStatus(long id, int status, [FromBody] Comment.WithCommentSpec spec)
    {
        if (Enum.GetValues<AccountStatusTypes>().Select(x => (int)x).All(x => x != status))
            return BadRequest(Result.Error(MSG.InvalidAccountStatus));

        var account = await tenantCtx.Accounts
            .Include(x => x.Party)
            // .ThenInclude(x => x.PartyTags)
            .ThenInclude(x => x.Tags)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (account == null)
            return NotFound();

        var tag = await tenantCtx.Tags.FirstOrDefaultAsync(x => x.Name == "HasClosedAccount");
        if (tag == null)
        {
            tag = new Tag { Name = "HasClosedAccount", Type = "party" };
            tenantCtx.Tags.Add(tag);
        }

        var partyTag = account.Party.Tags.FirstOrDefault(x => x.Name == tag.Name);
        var hasClosedAccount = status != (int)AccountStatusTypes.Activate || await tenantCtx.Accounts
            .AnyAsync(x => x.PartyId == account.PartyId && x.Id != account.Id && x.Status != 0);

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (hasClosedAccount && partyTag == null) account.Party.Tags.Add(tag);
        if (!hasClosedAccount && partyTag != null) account.Party.Tags.Remove(partyTag);

        account.AccountLogs.Add(M.BuildLog(GetPartyId(), "UpdateAccountStatus"
            , Enum.GetName((AccountStatusTypes)account.Status) ?? account.Status.ToString()
            , Enum.GetName((AccountStatusTypes)status) ?? status.ToString()));
        account.Status = (short)status;
        account.UpdatedOn = DateTime.UtcNow;

        // tenantDbContext.Comments.Add(Comment.Build(account.Id, GetPartyId(), CommentTypes.Account, spec.Comment));
        account.AccountComments.Add(new AccountComment
        {
            Content = spec.Comment,
            OperatorPartyId = GetPartyId(),
            CreatedOn = DateTime.UtcNow
        });
        tenantCtx.Accounts.Update(account);
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId());
        return NoContent();
    }

    /// <summary>
    /// Update tags for an account
    /// </summary>
    /// <param name="id"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [NonAction]
    [HttpPut("{id:long}/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateTags(long id, Account.UpdateTag spec)
    {
        if (id != spec.Id)
            return BadRequest();

        var account = await tenantCtx.Accounts
            .Include(x => x.Tags)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (account == null)
            return NotFound();

        var log = M.BuildLog(GetPartyId(), "UpdateAccountTags",
            string.Join(",", account.Tags.Select(x => x.Name)),
            "");

        account.Tags.Clear();

        var accountTags = await tenantCtx.Tags
            .Where(x => spec.TagNames.Contains(x.Name))
            .ToListAsync();

        log.After = string.Join(",", accountTags.Select(x => x.Name));
        account.AccountLogs.Add(log);

        foreach (var tag in accountTags)
        {
            account.Tags.Add(tag);
        }

        account.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Accounts.Update(account);
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId());
        return NoContent();
    }

    /// <summary>
    /// Enable or Disable HasLevelRule
    /// </summary>
    /// <param name="id"></param>
    /// <param name="hasLevelRule"></param>
    /// <returns></returns>
    [NonAction]
    [HttpPut("{id:long}/has-level-rule")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableHasLevelRule(long id, [FromQuery] HasLevelRuleTypes hasLevelRule)
    {
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == id);
        if (account == null)
            return NotFound();

        account.AccountLogs.Add(M.BuildLog(GetPartyId(), "ChangeHasLevelRule",
            account.HasLevelRule.ToString(), ((int)hasLevelRule).ToString()));
        account.HasLevelRule = (int)hasLevelRule;
        account.UpdatedOn = DateTime.UtcNow;

        tenantCtx.Accounts.Update(account);
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId());
        return NoContent();
    }


    [NonAction]
    [HttpPut("{id:long}/fund-type")]
    public async Task<IActionResult> ChangeFundType(long id, [FromQuery] FundTypes fundType)
    {
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == id);
        if (account == null)
            return NotFound();
        var log = M.BuildLog(GetPartyId(), "ChangeFundType",
            account.FundType.ToString(), fundType.ToString());
        account.AccountLogs.Add(log);
        account.FundType = (int)fundType;
        account.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Accounts.Update(account);
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId());
        return NoContent();
    }


    [NonAction]
    [HttpGet("{id:long}/level-setting")]
    public async Task<IActionResult> GetLevelSetting(long id)
    {
        var levelSetting = await tradingService.GetCalculatedRebateLevelSettingById(id);
        if (levelSetting.IsEmpty()) return NotFound();
        return Ok(levelSetting);
    }

    [NonAction]
    [HttpGet("mt-group-symbol")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MetaTradeGroupAndSymbol([FromQuery] int serviceId, [FromQuery] string group,
        [FromQuery] string symbol, [FromQuery] int transId)
    {
        var info = await tradingService.GetMetaTradeGroupAndSymbolInfo(serviceId, group, symbol, transId);
        try
        {
            var result = JsonConvert.DeserializeObject<dynamic>(info);
            var length = (int)result?.answer?.Count!;
            Console.WriteLine($"Result answer length {length}.");
            return Ok(result);
        }
        catch
        {
            // ignored
        }

        return Ok(info);
    }

    // [HttpPost("build-search-text")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<IActionResult> BuildAccountSearchText([FromQuery] M.Criteria? criteria)
    // {
    //     criteria ??= new M.Criteria();
    //     return Ok(await tradingService.UpdateAccountSearchText(criteria));
    // }

    [NonAction]
    [HttpGet("check-account-number/{accountNumber:long}")]
    public async Task<IActionResult> CheckAccountNumber(long accountNumber)
    {
        var (result, msg) = await accManSvc.CheckAccountNumberAsync(accountNumber);
        return result ? Ok() : BadRequest(msg);
    }

    [NonAction]
    [HttpGet("{id:long}/change-account-number/{accountNumber:long}")]
    public async Task<IActionResult> ChangeAccountNumber(long id, long accountNumber)
    {
        var (result, msg) = await accManSvc.ChangeAccountNumberAsync(id, accountNumber);
        return result ? Ok() : BadRequest(msg);
    }

    public class Item
    {
        public int Cid { get; set; }
        public double R { get; set; }
    }

    public class Schema
    {
        public int AccountType { get; set; }
        public double Pips { get; set; }
        public double Commission { get; set; }
        public double Percentage { get; set; }
        public List<Item> Items { get; set; } = [];
        public List<object> AllowPips { get; set; } = [];
        public List<object> AllowCommissions { get; set; } = [];
    }

    public class ClientSchema
    {
        public int AccountType { get; set; }
        public double Pips { get; set; }
        public double Commission { get; set; }
    }

    public class RebateRuleStructure
    {
        public List<Schema> AllowedAccounts { get; set; } = [];
        public string Language { get; set; } = "";
    }

    public class SummaryStructure
    {
        public string Name { get; set; } = "";
        public object Code { get; set; } = "";
        public List<Schema> Schema { get; set; } = [];
        public string Language { get; set; } = "";
        public int SiteId { get; set; }
    }

    public class ClientSummaryStructure
    {
        public string Name { get; set; } = "";
        public object Code { get; set; } = "";
        public List<ClientSchema> AllowAccountTypes { get; set; } = [];
        public string Language { get; set; } = "";
        public int SiteId { get; set; }
    }

    /// <summary>
    /// Get Account infos by Referral Code
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="code"></param>
    /// <param name="realCode"></param>
    /// <returns></returns>
    [NonAction]
    [HttpGet("ib-to-sales/{uid:long}/{code}/{realCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IbtoSales(long uid, string code, int realCode)
    {
        // return info
        dynamic processInfo = new ExpandoObject();
        var tenant = await centralCtx.Tenants.SingleAsync(x => x.Id == 10004);
        using var scope = serviceProvider.CreateScope();

        var s = scope.ServiceProvider.GetRequiredService<Tenancy>();
        // s.SetTenant(tenant);
        s.SetTenantId(tenant.Id);
        var scopedTenantDbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();


        var me = await scopedTenantDbContext.Accounts.SingleOrDefaultAsync(x => x.Uid == uid);
        if (me == null) return NotFound();

        // Check parents are sales/rep ==================================================================
        // List<long> referPathUids = me.ReferPath.Split('.', StringSplitOptions.RemoveEmptyEntries)
        //     .Select(long.Parse)
        //     .ToList();
        //
        // var check = scopedTenantDbContext.Accounts
        //     .Any(x => referPathUids.Contains(x.Uid) && (x.Role != 100 && x.Role != 110));
        // if (check) return BadRequest(ToErrorResult("referPath Error"));
        // ===============================================================================================

        // Update role = Sales,
        // salesAccountId = 自己,
        // agentAccountId = null
        // code = sales code
        me.Role = (short)AccountRoleTypes.Sales;
        // me.AgentAccountId = null; 不改, 上級才能找到我
        me.SalesAccountId = me.Id;
        me.Code = code;

        processInfo.me = me.Uid;
        processInfo.referPath = me.ReferPathUids;
        scopedTenantDbContext.Accounts.Update(me);

        // ===============================================================================================
        // Update 底下 IBs ( 分別為 Top IB, 二級以下 IBs )
        // Top IB salesAccountId = me
        // Top IB agentAccountId = null
        // 二級以下 IB salesAccountId = me
        // 二級以下 IB agentAccountId = 不變

        processInfo.topLevelAccounts = new List<long>();
        processInfo.otherLevelAccounts = new List<long>();

        var belowAccounts = scopedTenantDbContext.Accounts
            .Where(x => x.AgentAccountId == me.Id)
            .ToList();

        foreach (var account in belowAccounts)
        {
            processInfo.topLevelAccounts.Add(account.Uid);

            if (account.Role == (short)UserRoleTypes.IB)
            {
                processInfo.otherLevelAccounts = await
                    updateBelowAccounts(scopedTenantDbContext, account.Id, me.Id, processInfo.otherLevelAccounts,
                        realCode);

                // Top IB 沒有 AgentAccountID, 但 IB 的 IB 有
                account.AgentAccountId = null;
                account.SalesAccountId = me.Id;
            }
            else if (account.Role == (short)UserRoleTypes.Sales)
            {
                account.AgentAccountId = null;
                account.SalesAccountId = me.Id;
            }
            else if (account.Role == (short)UserRoleTypes.Client)
            {
                // sales 直客
                account.AgentAccountId = null;
                account.SalesAccountId = me.Id;
            }

            scopedTenantDbContext.Accounts.Update(account);
        }

        // check account path
        var salesUids = await scopedTenantDbContext.Accounts
            .Where(x => x.Role == (short)UserRoleTypes.Sales)
            .Select(x => x.Uid)
            .ToListAsync();

        if (salesUids.Any(uid => processInfo.otherLevelAccounts.Contains(uid)))
            return BadRequest(ToErrorResult("level error has Sales between IBs"));

        processInfo.topNums = processInfo.topLevelAccounts.Count;
        processInfo.otherNums = processInfo.otherLevelAccounts.Count;

        // ===============================================================================================
        // var myRebateAgentRule = scopedTenantDbContext.RebateAgentRules
        //     .FirstOrDefault(x => x.AgentAccountId == me.Id);
        //
        // if (myRebateAgentRule == null)
        //     return BadRequest(ToErrorResult("myRebateAgentRule not found"));
        //
        // if (JsonConvert.SerializeObject(myRebateAgentRule.LevelSetting) == "\"{}\"")
        //     return BadRequest(ToErrorResult("myRebateAgentRule LevelSetting is Empty"));
        // ===============================================================================================

        // Update Top IBs Level Setting
        var uids = me.ReferPathUids;
        var parentRules = await scopedTenantDbContext.RebateAgentRules
            .Where(x => uids.Contains(x.AgentAccount.Uid))
            .OrderBy(x => x.AgentAccount.Level)
            .ToResponseModel()
            .ToListAsync();

        var myRebateAgentRule = parentRules.FirstOrDefault(x => x.AgentAccountId == me.Id);
        if (myRebateAgentRule == null && realCode != 00234)
            return BadRequest(ToErrorResult("myRebateAgentRule not found"));
        if (myRebateAgentRule != null)
            return BadRequest(ToErrorResult("myRebateAgentRule LevelSetting is Empty"));

        var baseRebateRule = parentRules
            .Last(x => !string.IsNullOrEmpty(x.LevelSettingJson) && x.LevelSettingJson != "{}");

        if (me.IsTopLevelAgent()
            && JsonConvert.SerializeObject(myRebateAgentRule!.LevelSetting) != "\"{}\"")
            baseRebateRule = myRebateAgentRule;

        // check level setting has value
        if (JsonConvert.SerializeObject(baseRebateRule.LevelSetting) == "\"{}\"")
            return BadRequest(ToErrorResult("LevelSetting is Empty"));

        processInfo.baseLevelSetting = baseRebateRule.Id;

        var belowAgentAccountIds = belowAccounts
            .Where(x => x.Role == (short)AccountRoleTypes.Agent)
            .Select(x => x.Id)
            .ToList();

        processInfo.belowAgentAccount = belowAgentAccountIds;
        processInfo.belowAgentNums = processInfo.belowAgentAccount.Count;

        var rebateAgentRules = scopedTenantDbContext.RebateAgentRules
            .Where(x => belowAgentAccountIds.Contains(x.AgentAccountId))
            .ToList();

        var rebateAgentRuleAgentIds = rebateAgentRules.Select(x => x.AgentAccountId).ToList();
        processInfo.ruleIds = new List<int>(); // Update
        processInfo.addRules = new List<int>(); // Add New()

        foreach (var id in belowAgentAccountIds)
        {
            if (rebateAgentRuleAgentIds.Contains(id))
            {
                var rule = rebateAgentRules.First(x => x.AgentAccountId == id);
                rule.LevelSetting = JsonConvert.SerializeObject(baseRebateRule.LevelSetting);
                processInfo.ruleIds.Add((int)id);
                scopedTenantDbContext.RebateAgentRules.Update(rule);
            }
            else
            {
                var item = new RebateAgentRule
                {
                    AgentAccountId = id,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    Schema = "{}",
                    LevelSetting = JsonConvert.SerializeObject(baseRebateRule.LevelSetting),
                };
                processInfo.addRules.Add((int)id);
                scopedTenantDbContext.RebateAgentRules.Add(item);
            }
        }

        // //Update Referral Code
        processInfo.ibReferralCode = new List<int>();
        processInfo.clientReferralCode = new List<int>();
        var referralCodes = scopedTenantDbContext.ReferralCodes
            .Where(x => x.AccountId == me.Id)
            .ToList();

        var baseRebateRuleForCode = scopedTenantDbContext.RebateAgentRules
            .First(x => x.Id == baseRebateRule.Id);

        var myLevelSetting =
            JsonConvert.DeserializeObject<RebateRuleStructure>(baseRebateRuleForCode.LevelSetting);

        foreach (var item in referralCodes)
        {
            if (item.ServiceType == (short)UserRoleTypes.IB)
            {
                processInfo.ibReferralCode.Add((int)item.Id);
                item.ServiceType = (short)UserRoleTypes.Broker;

                var summary = JsonConvert.DeserializeObject<SummaryStructure>(item.Summary)!;
                summary.Name = item.Name;
                summary.Code = item.Code;

                foreach (var schema in summary.Schema)
                {
                    schema.Pips = 0;
                    schema.Commission = 0;
                    schema.Percentage = 0;

                    var acc = myLevelSetting!.AllowedAccounts.FirstOrDefault(x => x.AccountType == schema.AccountType);
                    schema.AllowPips = acc!.AllowPips;
                    schema.AllowCommissions = acc.AllowCommissions;
                    schema.Items = acc.Items;
                }

                item.Summary = JsonConvert.SerializeObject(summary);
                scopedTenantDbContext.ReferralCodes.Update(item);
            }

            if (item.ServiceType == (short)UserRoleTypes.Client)
            {
                var summary = JsonConvert.DeserializeObject<SummaryStructure>(item.Summary);
                if (summary?.Schema == null) continue;

                processInfo.clientReferralCode.Add((int)item.Id);

                var newSummary = new ClientSummaryStructure
                {
                    Name = item.Name,
                    Code = item.Code,
                    Language = summary.Language,
                    SiteId = summary.SiteId
                };

                foreach (var account in summary.Schema)
                {
                    newSummary.AllowAccountTypes.Add(new ClientSchema
                    {
                        AccountType = account.AccountType,
                        Pips = 0.0,
                        Commission = 0.0
                    });
                }

                item.Summary = JsonConvert.SerializeObject(newSummary);
                scopedTenantDbContext.ReferralCodes.Update(item);
            }
        }

        if (realCode == 00234)
        {
            await scopedTenantDbContext.SaveChangesAsync();

            // ============================================================================

            var user = await authDbContext.Users
                .Where(x => x.PartyId == me.PartyId && x.TenantId == tenancy.GetTenantId())
                .FirstAsync();

            await userManager.ReplaceClaimAsync(user, new Claim(UserClaimTypes.AgentAccount, me.Uid.ToString()),
                new Claim(UserClaimTypes.SalesAccount, me.Uid.ToString()));

            var myAccountsRole = scopedTenantDbContext.Accounts
                .Where(x => x.PartyId == me.PartyId)
                .Select(x => x.Role)
                .ToList();

            processInfo.myAccountsRole = myAccountsRole;

            if (!myAccountsRole.Contains((short)AccountRoleTypes.Agent))
            {
                if (await userManager.IsInRoleAsync(user, UserRoleTypesString.IB))
                {
                    await userManager.RemoveFromRoleAsync(user, UserRoleTypesString.IB);
                }
            }

            if (false == await userManager.IsInRoleAsync(user, UserRoleTypesString.Sales))
            {
                await userManager.AddToRoleAsync(user, UserRoleTypesString.Sales);
            }
        }

        return Ok(processInfo);
    }

    private async Task<List<long>> updateBelowAccounts(TenantDbContext scopedTenantDbContext, long currentId,
        long salesId, List<long> processBelowAccounts,
        int realCode)
    {
        var belowAccounts = await scopedTenantDbContext.Accounts
            .Where(x => x.AgentAccountId == currentId)
            .ToListAsync();

        foreach (var account in belowAccounts)
        {
            processBelowAccounts.Add(account.Uid);

            if (account.Role == (short)UserRoleTypes.IB)
            {
                processBelowAccounts = await updateBelowAccounts(scopedTenantDbContext, account.Id, salesId,
                    processBelowAccounts, realCode);
                account.SalesAccountId = salesId;
            }
            else if (account.Role == (short)UserRoleTypes.Sales)
            {
                // account.AgentAccountId = null;
                // account.SalesAccountId = salesId;
            }
            else if (account.Role == (short)UserRoleTypes.Client)
            {
                account.SalesAccountId = salesId;
            }

            scopedTenantDbContext.Accounts.Update(account);
        }

        if (realCode == 00234)
        {
            await scopedTenantDbContext.SaveChangesAsync();
        }

        return processBelowAccounts;
    }

    [GeneratedRegex("(?<!^)(?=[A-Z])")]
    private static partial Regex MyRegex();
}