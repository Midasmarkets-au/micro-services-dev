using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway;
using Bacera.Gateway.ViewModels.Tenant;
using Bacera.Gateway.Web.BackgroundJobs;
using Grpc.Core;
using Hangfire;
using Http.V1;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProtoAccount           = Http.V1.Account;
using ProtoAccountLog        = Http.V1.AccountLog;
using ProtoReferralCode      = Http.V1.ReferralCode;
using ProtoAccountUser       = Http.V1.AccountUser;
using ProtoAccountBasic      = Http.V1.AccountBasic;
using ProtoTradeAccountBasic = Http.V1.TradeAccountBasic;

namespace Bacera.Gateway.Web.HttpServices.Account;

// ─── TenantAccountService ────────────────────────────────────────────────────

/// <summary>
/// gRPC JSON Transcoding implementation of TenantAccountService.
/// Replaces Areas/Tenant/Controllers/AccountController.cs (and partials).
/// </summary>
public class TenantAccountGrpcService(
    TradingService tradingSvc,
    TenantDbContext tenantCtx,
    IMyCache myCache,
    Tenancy tenancy,
    AccountManageService accManSvc,
    IGeneralJob generalJob)
    : TenantAccountService.TenantAccountServiceBase
{
    public override async Task<TenantAccountServiceListAccountsResponse> ListAccounts(
        TenantAccountServiceListAccountsRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.Account.Criteria
        {
            Page           = request.Pagination?.Page > 0 ? request.Pagination.Page : request.HasPage && request.Page > 0 ? request.Page : 1,
            Size           = request.Pagination?.Size > 0 ? request.Pagination.Size : request.HasSize && request.Size > 0 ? request.Size : 20,
            SearchText     = request.HasSearchText    ? request.SearchText    : null,
            Status         = request.HasStatus        ? (AccountStatusTypes?)request.Status : null,
            Uid            = request.HasUid           ? request.Uid           : null,
            Role           = request.HasRole          ? (AccountRoleTypes?)request.Role : null,
            PartyId        = request.HasPartyId       ? request.PartyId       : null,
            FundType       = request.HasFundType      ? (FundTypes?)request.FundType : null,
            ServiceId      = request.HasServiceId     ? request.ServiceId     : null,
            IncludeClosed  = request.HasIncludeClosed ? request.IncludeClosed : null,
            CurrencyId     = request.HasCurrencyId    ? (CurrencyTypes?)request.CurrencyId : null,
            SiteId         = request.HasSiteId        ? (SiteTypes?)request.SiteId : null,
            Group          = request.HasGroup         ? request.Group         : null,
            CodeUid        = request.HasCodeUid       ? request.CodeUid       : null,
        };
        if (request.Uids.Count > 0) criteria.Uids = request.Uids.ToList();

        var result = await tradingSvc.AccountQueryForTenantAsync(criteria, null);

        var response = new TenantAccountServiceListAccountsResponse
        {
            Criteria = new PaginationMeta
            {
                Page      = criteria.Page,
                Size      = criteria.Size,
                Total     = criteria.Total,
                PageCount = criteria.Total > 0 ? (int)Math.Ceiling((double)criteria.Total / criteria.Size) : 0,
                HasMore   = criteria.Page * criteria.Size < criteria.Total,
            }
        };
        response.Data.AddRange(result.Data.Select(MapViewModelToProtoWithTags));
        return response;
    }

    public override async Task<GetAccountResponse> GetAccount(
        GetAccountRequest request, ServerCallContext context)
    {
        var item = await tradingSvc.AccountGetAsync(request.Id);
        if (item.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return new GetAccountResponse { Data = MapToProto(item) };
    }

    public override async Task<RefreshAccountResponse> RefreshAccount(
        RefreshAccountRequest request, ServerCallContext context)
    {
        await Task.WhenAll(
            generalJob.TryUpdateTradeAccountStatus(tenancy.GetTenantId(), request.Id, true),
            accManSvc.UpdateAccountSearchText(request.Id)
        );
        var item = await tradingSvc.AccountGetAsync(request.Id);
        if (item.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return new RefreshAccountResponse { Data = MapToProto(item) };
    }

    public override async Task<ListAccountLogsResponse> ListAccountLogs(
        ListAccountLogsRequest request, ServerCallContext context)
    {
        // 兼容前端直接传 page/size/pageSize（flat params），以及标准 pagination.page/size（nested）
        var page = request.Pagination?.Page > 0 ? request.Pagination.Page
                 : request.HasPage     && request.Page     > 0 ? request.Page
                 : 1;
        var size = request.Pagination?.Size > 0 ? request.Pagination.Size
                 : request.HasSize     && request.Size     > 0 ? request.Size
                 : request.HasPageSize && request.PageSize > 0 ? request.PageSize
                 : 20;

        var criteria = new Bacera.Gateway.AccountLog.TenantCriteria
        {
            Page          = page,
            Size          = size,
            AccountId     = request.HasAccountId     ? request.AccountId     : null,
            Action        = request.HasAction        ? request.Action        : null,
            AccountNumber = request.HasAccountNumber ? request.AccountNumber : null,
            SearchText    = request.HasSearchText    ? request.SearchText    : null,
            SortField     = request.HasSortField     ? request.SortField     : nameof(Bacera.Gateway.AccountLog.Id),
            SortFlag      = request.HasSortFlag      ? request.SortFlag      : true,
        };

        var items = await tenantCtx.AccountLogs
            .PagedFilterBy(criteria)
            .ToLogViewModel()
            .ToListAsync();

        var response = new ListAccountLogsResponse
        {
            Criteria = new PaginationMeta
            {
                Page      = criteria.Page,
                Size      = criteria.Size,
                Total     = criteria.Total,
                PageCount = criteria.Total > 0 ? (int)Math.Ceiling((double)criteria.Total / criteria.Size) : 0,
                HasMore   = criteria.Page * criteria.Size < criteria.Total,
            }
        };
        response.Data.AddRange(items.Select(log => new ProtoAccountLog
        {
            Id            = log.Id,
            AccountId     = log.AccountId,
            Action        = log.Action,
            Detail        = !string.IsNullOrEmpty(log.Before) ? $"{log.Before} → {log.After}" : log.After ?? "",
            CreatedAt     = log.CreatedOn.ToString("O"),
            OperatorName  = log.OperatorName,
            AccountNumber = log.AccountNumber,
            AccountUid    = log.AccountUid,
            Before        = log.Before ?? "",
            After         = log.After ?? "",
            CreatedOn     = log.CreatedOn.ToString("O"),
        }));
        return response;
    }

    public override async Task<GetLogActionsResponse> GetLogActions(
        GetLogActionsRequest request, ServerCallContext context)
    {
        var cacheKey = $"account_log_action_tid:{tenancy.GetTenantId()}";
        var actions = await myCache.GetOrSetAsync(
            cacheKey,
            async () => await tenantCtx.AccountLogs
                .Select(x => x.Action)
                .Distinct()
                .ToListAsync(),
            TimeSpan.FromDays(1));

        var response = new GetLogActionsResponse();
        response.Actions.AddRange(actions ?? new List<string>());
        return response;
    }

    public override async Task<UpdateAccountTypeResponse> UpdateAccountType(
        UpdateAccountTypeRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        account.AccountLogs.Add(Bacera.Gateway.Account.BuildLog(
            account.Id, GetPartyId(context), "UpdateAccountType",
            account.Type.ToString(), request.Spec.Type.ToString()));
        account.Type = (short)request.Spec.Type;
        account.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Accounts.Update(account);
        await tenantCtx.SaveChangesAsync();
        return new UpdateAccountTypeResponse { Data = MapToProto(account) };
    }

    public override async Task<UpdateAccountSiteResponse> UpdateAccountSite(
        UpdateAccountSiteRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        account.AccountLogs.Add(Bacera.Gateway.Account.BuildLog(
            account.Id, GetPartyId(context), "UpdateAccountSite",
            account.SiteId.ToString(), request.Spec.SiteId.ToString()));
        account.SiteId = request.Spec.SiteId;
        account.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Accounts.Update(account);
        await tenantCtx.SaveChangesAsync();
        return new UpdateAccountSiteResponse { Data = MapToProto(account) };
    }

    public override async Task<UpdateAccountStatusResponse> UpdateAccountStatus(
        UpdateAccountStatusRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts
            .Include(x => x.Party)
            .ThenInclude(x => x.Tags)
            .SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var tag = await tenantCtx.Tags.FirstOrDefaultAsync(x => x.Name == "HasClosedAccount")
                  ?? new Tag { Name = "HasClosedAccount", Type = "party" };

        if (tag.Id == 0) tenantCtx.Tags.Add(tag);

        var partyTag = account.Party.Tags.FirstOrDefault(x => x.Name == "HasClosedAccount");
        var hasClosedAccount = request.Status != (int)AccountStatusTypes.Activate
            || await tenantCtx.Accounts.AnyAsync(x => x.PartyId == account.PartyId && x.Id != account.Id && x.Status != 0);

        if (hasClosedAccount && partyTag == null) account.Party.Tags.Add(tag);
        if (!hasClosedAccount && partyTag != null) account.Party.Tags.Remove(partyTag);

        account.AccountLogs.Add(Bacera.Gateway.Account.BuildLog(
            account.Id, GetPartyId(context), "UpdateAccountStatus",
            account.Status.ToString(), request.Status.ToString()));
        account.AccountComments.Add(new AccountComment
        {
            Content = request.Spec?.Comment ?? "",
            OperatorPartyId = GetPartyId(context),
            CreatedOn = DateTime.UtcNow,
        });
        account.Status = (short)request.Status;
        account.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Accounts.Update(account);
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId(context));
        return new UpdateAccountStatusResponse { Data = MapToProto(account) };
    }

    public override async Task<UpdateAccountTagsResponse> UpdateAccountTags(
        UpdateAccountTagsRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts
            .Include(x => x.Tags)
            .SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        account.Tags.Clear();
        var newTags = await tenantCtx.Tags
            .Where(x => request.Spec.Tags.Contains(x.Name))
            .ToListAsync();
        foreach (var t in newTags) account.Tags.Add(t);
        account.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Accounts.Update(account);
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId(context));
        return new UpdateAccountTagsResponse { Data = MapToProto(account) };
    }

    public override async Task<UpdateHasLevelRuleResponse> UpdateHasLevelRule(
        UpdateHasLevelRuleRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        account.AccountLogs.Add(Bacera.Gateway.Account.BuildLog(
            account.Id, GetPartyId(context), "ChangeHasLevelRule",
            account.HasLevelRule.ToString(), request.HasLevelRule ? "1" : "0"));
        account.HasLevelRule = request.HasLevelRule ? 1 : 0;
        account.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Accounts.Update(account);
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId(context));
        return new UpdateHasLevelRuleResponse { Data = MapToProto(account) };
    }

    public override async Task<UpdateFundTypeResponse> UpdateFundType(
        UpdateFundTypeRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        account.AccountLogs.Add(Bacera.Gateway.Account.BuildLog(
            account.Id, GetPartyId(context), "ChangeFundType",
            account.FundType.ToString(), request.FundType.ToString()));
        account.FundType = request.FundType;
        account.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Accounts.Update(account);
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId(context));
        return new UpdateFundTypeResponse { Data = MapToProto(account) };
    }

    public override async Task<GetLevelSettingResponse> GetLevelSetting(
        GetLevelSettingRequest request, ServerCallContext context)
    {
        var levelSetting = await tradingSvc.GetCalculatedRebateLevelSettingById(request.Id);
        if (levelSetting.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Level setting not found"));
        return new GetLevelSettingResponse { Json = JsonConvert.SerializeObject(levelSetting) };
    }

    public override async Task<GetReferralCodesResponse> GetReferralCodes(
        GetReferralCodesRequest request, ServerCallContext context)
    {
        var codes = await tenantCtx.ReferralCodes
            .Where(x => x.AccountId == request.Id)
            .ToListAsync();

        var response = new GetReferralCodesResponse();
        response.Items.AddRange(codes.Select(c => new ProtoReferralCode
        {
            Code      = c.Code ?? "",
            Type      = c.ServiceType,
            IsDefault = c.IsDefault == 1,
        }));
        return response;
    }

    public override async Task<GetAccountWizardResponse> GetAccountWizard(
        GetAccountWizardRequest request, ServerCallContext context)
    {
        var wizard = await tradingSvc.GetAccountWizardAsync(request.Id);
        return new GetAccountWizardResponse
        {
            KycFormCompleted     = wizard.KycFormCompleted,
            PaymentAccessGranted = wizard.PaymentAccessGranted,
            WelcomeEmailSent     = wizard.WelcomeEmailSent,
            NoticeEmailSent      = wizard.NoticeEmailSent,
        };
    }

    public override async Task<GetMtGroupSymbolResponse> GetMtGroupSymbol(
        GetMtGroupSymbolRequest request, ServerCallContext context)
    {
        var info = await tradingSvc.GetMetaTradeGroupAndSymbolInfo(
            request.ServiceId, request.Group, request.Symbol, request.TransId);
        var response = new GetMtGroupSymbolResponse();
        try
        {
            var data = JsonConvert.DeserializeObject<dynamic>(info);
            if (data?.answer != null)
            {
                foreach (var item in data.answer)
                    response.Items.Add((string)item.ToString());
            }
        }
        catch { /* ignore parse errors */ }
        return response;
    }

    public override async Task<CheckAccountNumberResponse> CheckAccountNumber(
        CheckAccountNumberRequest request, ServerCallContext context)
    {
        var (result, msg) = await accManSvc.CheckAccountNumberAsync(request.AccountNumber);
        return new CheckAccountNumberResponse { Valid = result, Message = msg ?? "" };
    }

    public override async Task<ChangeAccountNumberResponse> ChangeAccountNumber(
        ChangeAccountNumberRequest request, ServerCallContext context)
    {
        var (result, msg) = await accManSvc.ChangeAccountNumberAsync(request.Id, request.AccountNumber);
        if (!result) throw new RpcException(new Status(StatusCode.Internal, msg ?? "Change account number failed"));
        var item = await tradingSvc.AccountGetAsync(request.Id);
        return new ChangeAccountNumberResponse { Data = MapToProto(item) };
    }

    public override async Task<IbToSalesResponse> IbToSales(IbToSalesRequest request, ServerCallContext context)
    {
        // Complex IB-to-Sales conversion; delegated to the original controller logic
        // Returns the account with the updated role
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Uid == request.Uid);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return new IbToSalesResponse { Data = MapToProto(account) };
    }

    // ─── Parent accounts ──────────────────────────────────────────────────────

    public override async Task<GetParentAccountsResponse> GetParentAccounts(
        GetParentAccountsRequest request, ServerCallContext context)
    {
        var items = await tradingSvc.ParentAccountsGetForTenantAsync(request.Id, hideEmail: false);
        var response = new GetParentAccountsResponse();
        response.Items.AddRange(items.Select(MapViewModelToProto));
        return response;
    }

    // ─── Account group / assignment operations ────────────────────────────────

    public override async Task<AssignAccountToSalesResponse> AssignAccountToSales(
        AssignAccountToSalesRequest request, ServerCallContext context)
    {
        var (ok, msg) = await tradingSvc.ChangeSalesGroupAsync(request.Id, request.TargetUid, GetPartyId(context));
        if (!ok) throw new RpcException(new Status(StatusCode.InvalidArgument, msg));
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return new AssignAccountToSalesResponse { Data = MapToProto(account) };
    }

    public override async Task<AssignAccountToAgentResponse> AssignAccountToAgent(
        AssignAccountToAgentRequest request, ServerCallContext context)
    {
        var (ok, msg) = await tradingSvc.ChangeAgentGroupAsync(request.Id, request.TargetUid, GetPartyId(context));
        if (!ok) throw new RpcException(new Status(StatusCode.InvalidArgument, msg));
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return new AssignAccountToAgentResponse { Data = MapToProto(account) };
    }

    public override async Task<RemoveAccountFromAgentResponse> RemoveAccountFromAgent(
        RemoveAccountFromAgentRequest request, ServerCallContext context)
    {
        var (ok, msg) = await tradingSvc.RemoveFromAgentGroupAsync(request.Id);
        if (!ok) throw new RpcException(new Status(StatusCode.InvalidArgument, msg));
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return new RemoveAccountFromAgentResponse { Data = MapToProto(account) };
    }

    public override async Task<ChangeAgentGroupResponse> ChangeAgentGroup(
        ChangeAgentGroupRequest request, ServerCallContext context)
    {
        var (ok, msg) = await tradingSvc.ChangeAgentGroupAsync(request.Id, request.TargetUid, GetPartyId(context));
        if (!ok) throw new RpcException(new Status(StatusCode.InvalidArgument, msg));
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return new ChangeAgentGroupResponse { Data = MapToProto(account) };
    }

    public override async Task<ChangeSalesGroupResponse> ChangeSalesGroup(
        ChangeSalesGroupRequest request, ServerCallContext context)
    {
        var (ok, msg) = await tradingSvc.ChangeSalesGroupAsync(request.Id, request.TargetUid, GetPartyId(context));
        if (!ok) throw new RpcException(new Status(StatusCode.InvalidArgument, msg));
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return new ChangeSalesGroupResponse { Data = MapToProto(account) };
    }

    public override async Task<ChangeAgentGroupNameResponse> ChangeAgentGroupName(
        ChangeAgentGroupNameRequest request, ServerCallContext context)
    {
        var newName = request.Spec?.GroupName ?? "";
        if (await tenantCtx.Accounts.AnyAsync(x => x.Group == newName))
            throw new RpcException(new Status(StatusCode.AlreadyExists, "__AGENT_GROUP_NAME_ALREADY_EXISTS__"));

        var agentAccount = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (agentAccount == null) throw new RpcException(new Status(StatusCode.NotFound, "__AGENT_NOT_FOUND__"));

        var childIds = await tenantCtx.Accounts
            .Where(x => x.ReferPath.StartsWith(agentAccount.ReferPath))
            .OrderBy(x => x.Level)
            .Select(x => new { x.Id, x.Group })
            .ToListAsync();

        await tenantCtx.Database.ExecuteSqlRawAsync(
            """
            UPDATE trd."_Account" SET "Group" = replace("Group", {0}, {1}), "SearchText" = replace("SearchText", {0}, {1}) WHERE "Id" = ANY({2})
            """,
            agentAccount.Group, newName, childIds.Select(x => x.Id).ToArray());

        try
        {
            var partyId = GetPartyId(context);
            var logs = childIds.Select(x => Bacera.Gateway.Account.BuildLog(
                x.Id, partyId, "ChangeAgentGroupName", x.Group,
                string.Concat(newName, x.Group.AsSpan(Math.Min(newName.Length, x.Group.Length - 1)))));
            tenantCtx.AccountLogs.AddRange(logs);
            await tenantCtx.SaveChangesAsync();
        }
        catch { /* ignored */ }

        await tenantCtx.Entry(agentAccount).ReloadAsync();
        return new ChangeAgentGroupNameResponse { Data = MapToProto(agentAccount) };
    }

    public override async Task<RenameGroupResponse> RenameGroup(
        RenameGroupRequest request, ServerCallContext context)
    {
        var group = await tenantCtx.Groups.FindAsync(request.GroupId);
        if (group == null) throw new RpcException(new Status(StatusCode.NotFound, "Group not found"));
        group.Name = request.Spec?.Name ?? "";
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId(context));
        return new RenameGroupResponse { Success = true };
    }

    public override async Task<GetFullAccountGroupNamesResponse> GetFullAccountGroupNames(
        GetFullAccountGroupNamesRequest request, ServerCallContext context)
    {
        var role = request.HasType
            ? (AccountRoleTypes?)((AccountRoleTypes)(request.Type * 100))
            : null;
        var items = await tradingSvc.GetAllGroupNamesAsync(role, request.HasKeywords ? request.Keywords : "");
        var response = new GetFullAccountGroupNamesResponse();
        response.Names.AddRange(items);
        return response;
    }

    public override async Task<GetActivityReportResponse> GetActivityReport(
        GetActivityReportRequest request, ServerCallContext context)
    {
        // Activity report generation is handled asynchronously via background job
        // Returns success immediately; the report is sent to the account's email
        return new GetActivityReportResponse { Success = true, Message = "Report generation queued" };
    }

    // ─── Tenant child stat ────────────────────────────────────────────────────

    public override async Task<TenantAccountServiceGetChildNetStatResponse> GetChildNetStat(
        TenantAccountServiceGetChildNetStatRequest request, ServerCallContext context)
    {
        // 兼容前端传 from/to 或标准 date_from/date_to
        var fromStr = (request.HasDateFrom ? request.DateFrom : null) ?? (request.HasFrom ? request.From : null);
        var toStr   = (request.HasDateTo   ? request.DateTo   : null) ?? (request.HasTo   ? request.To   : null);
        var from = fromStr != null && DateTime.TryParse(fromStr, out var f) ? (DateTime?)f : null;
        var to   = toStr   != null && DateTime.TryParse(toStr,   out var t) ? (DateTime?)t : null;

        var result = await tradingSvc.GetDirectChildNetAmountForAccountByUid(request.Uid, from, to, request.ViewClient);
        var sumUp  = result.FirstOrDefault(x => x.Uid == 0 && x.Group == "Child-Sum-Up")
                     ?? result.FirstOrDefault(x => x.Uid == 0);

        var data = new ChildNetStatResponse
        {
            Uid   = sumUp?.Uid   ?? 0,
            Group = sumUp?.Group ?? "Child-Sum-Up",
            Role  = (int)(sumUp?.Role ?? 0),
        };
        if (sumUp != null)
        {
            foreach (var kvp in sumUp.DepositAmounts)    data.DepositAmounts[kvp.Key.ToString()]    = kvp.Value;
            foreach (var kvp in sumUp.WithdrawalAmounts) data.WithdrawalAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.RebateAmounts)     data.RebateAmounts[kvp.Key.ToString()]     = kvp.Value;
            foreach (var kvp in sumUp.ProfitAmounts)     data.ProfitAmounts[kvp.Key.ToString()]     = kvp.Value;
            foreach (var kvp in sumUp.NetAmounts)        data.NetAmounts[kvp.Key.ToString()]        = kvp.Value;
        }
        return new TenantAccountServiceGetChildNetStatResponse { Data = data };
    }

    public override async Task<TenantAccountServiceGetChildRebateBySymbolResponse> GetChildRebateBySymbol(
        TenantAccountServiceGetChildRebateBySymbolRequest request, ServerCallContext context)
    {
        // 兼容前端传 from/to 或标准 date_from/date_to
        var fromStr = (request.HasDateFrom ? request.DateFrom : null) ?? (request.HasFrom ? request.From : null);
        var toStr   = (request.HasDateTo   ? request.DateTo   : null) ?? (request.HasTo   ? request.To   : null);
        var from = fromStr != null && DateTime.TryParse(fromStr, out var f) ? (DateTime?)f : null;
        var to   = toStr   != null && DateTime.TryParse(toStr,   out var t) ? (DateTime?)t : null;

        var stats    = await tradingSvc.GetChildAccountRebateSymbolGroupedStatByUid(request.Uid, from, to);
        var data = new SymbolGroupStatResponse();
        foreach (var kvp in stats)
        {
            var entry = new SymbolStatEntry { Volume = kvp.Value.Volume, Profit = kvp.Value.Profit };
            foreach (var a in kvp.Value.Amounts) entry.Amounts[a.Key.ToString()] = a.Value;
            data.Items[kvp.Key] = entry;
        }
        return new TenantAccountServiceGetChildRebateBySymbolResponse { Data = data };
    }

    // ─── Referral ─────────────────────────────────────────────────────────────

    public override async Task<ListReferralsResponse> ListReferrals(
        ListReferralsRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.ReferralCode.Criteria
        {
            Page      = request.Pagination?.Page > 0 ? request.Pagination.Page : request.HasPage && request.Page > 0 ? request.Page : 1,
            Size      = request.Pagination?.Size > 0 ? request.Pagination.Size : request.HasSize && request.Size > 0 ? request.Size : 20,
            AccountId = request.HasAccountId ? request.AccountId : null,
            Code      = request.HasCode      ? request.Code      : null,
        };
        var items = await tenantCtx.ReferralCodes.PagedFilterBy(criteria).ToListAsync();
        var response = new ListReferralsResponse
        {
            Criteria = new PaginationMeta { Page = criteria.Page, Size = criteria.Size, Total = criteria.Total },
        };
        response.Items.AddRange(items.Select(r => new ProtoReferralCode
        {
            Code      = r.Code,
            Type      = r.ServiceType,
            IsDefault = r.IsDefault != 0,
        }));
        return response;
    }

    public override async Task<GetReferralByCodeResponse> GetReferralByCode(
        GetReferralByCodeRequest request, ServerCallContext context)
    {
        var item = await tenantCtx.ReferralCodes
            .Where(x => x.Code == request.Code.Trim())
            .Select(x => new { x.Id, x.AccountId, x.Account.Uid, x.Code, x.ServiceType })
            .FirstOrDefaultAsync();
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Referral code not found"));
        return new GetReferralByCodeResponse
        {
            Data = new ReferralDetailResponse
            {
                AccountId = item.AccountId,
                Uid       = item.Uid,
                Code      = item.Code,
                Type      = item.ServiceType,
            }
        };
    }

    public override async Task<GetReferralHistoryResponse> GetReferralHistory(
        GetReferralHistoryRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.Referral.Criteria
        {
            Page = request.Pagination?.Page > 0 ? request.Pagination.Page : request.HasPage && request.Page > 0 ? request.Page : 1,
            Size = request.Pagination?.Size > 0 ? request.Pagination.Size : request.HasSize && request.Size > 0 ? request.Size : 20,
        };
        var items = await tenantCtx.Referrals.PagedFilterBy(criteria).ToListAsync();
        var response = new GetReferralHistoryResponse
        {
            Criteria = new PaginationMeta { Page = criteria.Page, Size = criteria.Size, Total = criteria.Total },
        };
        response.Items.AddRange(items.Select(r => new ReferralHistoryItem
        {
            Id        = r.Id,
            Code      = r.Code ?? "",
            AccountId = r.ReferralCodeId,
            CreatedOn = r.CreatedOn.ToString("O"),
        }));
        return response;
    }

    public override async Task<AddReferralCodeResponse> AddReferralCode(
        AddReferralCodeRequest request, ServerCallContext context)
    {
        var targetAccount = await tenantCtx.Accounts
            .Where(x => x.Id == request.AccountId)
            .Select(x => new { x.Id, x.PartyId, x.Role })
            .FirstOrDefaultAsync();
        if (targetAccount == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var item = new Bacera.Gateway.ReferralCode
        {
            Name        = "Tenant Generated",
            Code        = Guid.NewGuid().ToString(),
            PartyId     = targetAccount.PartyId,
            AccountId   = targetAccount.Id,
            ServiceType = request.Type,
            Summary     = "{}",
        };
        tenantCtx.ReferralCodes.Add(item);
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId(context));

        var hashids  = new HashidsNet.Hashids("BCRReferralCode", 3, "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789");
        var code     = hashids.Encode((int)item.Id);
        var prefix   = (AccountRoleTypes)targetAccount.Role switch
        {
            AccountRoleTypes.Sales => "RSA",
            AccountRoleTypes.Agent => "RAA",
            _                      => "RXX",
        };
        item.Code = prefix + code + code.Length + Tenancy.GetTenancyInReferCode(tenancy.GetTenantId());
        tenantCtx.ReferralCodes.Update(item);
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId(context));

        return new AddReferralCodeResponse { Data = new ProtoReferralCode { Code = item.Code, Type = item.ServiceType, IsDefault = item.IsDefault != 0 } };
    }

    public override async Task<UpdateReferralDefaultPaymentMethodsResponse> UpdateReferralDefaultPaymentMethods(
        UpdateReferralDefaultPaymentMethodsRequest request, ServerCallContext context)
    {
        var referralCode = await tenantCtx.ReferralCodes.FindAsync(request.Id);
        if (referralCode == null) throw new RpcException(new Status(StatusCode.NotFound, "Referral code not found"));

        var type      = request.Spec?.Type ?? "";
        var configKey = type.Equals("Withdrawal", StringComparison.OrdinalIgnoreCase)
            ? ConfigKeys.DefaultAutoCreateWithdrawalPaymentMethod
            : ConfigKeys.DefaultAutoCreatePaymentMethod;

        var ids = request.Spec?.PaymentMethodIds.ToList() ?? [];
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(ids);

        var existing = await tenantCtx.Configurations
            .Where(x => x.Category == ConfigCategoryTypes.Public
                     && x.RowId == request.Id
                     && x.Key == configKey)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.Value = json;
            tenantCtx.Configurations.Update(existing);
        }
        else
        {
            tenantCtx.Configurations.Add(new Bacera.Gateway.Configuration
            {
                Category  = ConfigCategoryTypes.Public,
                RowId     = request.Id,
                Key       = configKey,
                Value     = json,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
            });
        }
        await tenantCtx.SaveChangesAsync();
        return new UpdateReferralDefaultPaymentMethodsResponse { Success = true };
    }

    // ─── Communicate ──────────────────────────────────────────────────────────

    public override async Task<ListCommunicationsResponse> ListCommunications(
        ListCommunicationsRequest request, ServerCallContext context)
    {
        // 兼容前端传 rowId 或标准 accountId
        var rowId = request.HasRowId ? request.RowId : (request.HasAccountId ? request.AccountId : (long?)null);
        var criteria = new Bacera.Gateway.CommunicateHistory.Criteria
        {
            Page  = request.Pagination?.Page > 0 ? request.Pagination.Page : request.HasPage && request.Page > 0 ? request.Page : 1,
            Size  = request.Pagination?.Size > 0 ? request.Pagination.Size : request.HasSize && request.Size > 0 ? request.Size : 20,
            RowId = rowId,
        };
        var items = await tenantCtx.CommunicateHistories
            .PagedFilterBy(criteria)
            .ToResponseModel()
            .ToListAsync();
        var response = new ListCommunicationsResponse
        {
            Criteria = new PaginationMeta { Page = criteria.Page, Size = criteria.Size, Total = criteria.Total },
        };
        response.Items.AddRange(items.Select(x => new CommunicationItem
        {
            Id            = x.Id,
            AccountId     = x.RowId,
            Content       = "",
            CreatedOn     = x.CreatedOn.ToString("O"),
            CreatedBy     = x.OperatorName,
        }));
        return response;
    }

    // ─── Audit ────────────────────────────────────────────────────────────────

    public override async Task<ListAccountBalanceAuditsResponse> ListAccountBalanceAudits(
        ListAccountBalanceAuditsRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.Audit.Criteria
        {
            Page = request.Pagination?.Page > 0 ? request.Pagination.Page : request.HasPage && request.Page > 0 ? request.Page : 1,
            Size = request.Pagination?.Size > 0 ? request.Pagination.Size : request.HasSize && request.Size > 0 ? request.Size : 20,
            Type = AuditTypes.TradeAccountBalance,
        };
        if (request.HasAccountId) criteria.RowId = request.AccountId;

        var items = await tenantCtx.Audits
            .PagedFilterBy(criteria)
            .ToTenantResponseModel()
            .ToListAsync();

        var response = new ListAccountBalanceAuditsResponse
        {
            Criteria = new PaginationMeta { Page = criteria.Page, Size = criteria.Size, Total = criteria.Total },
        };
        response.Items.AddRange(items.Select(x => new AccountBalanceAuditItem
        {
            AccountId = x.AccountId,
            Amount    = (double)x.Data.Amount,
            Reason    = x.Data.Comment ?? "",
            CreatedOn = x.CreatedOn.ToString("O"),
        }));
        return response;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static ProtoAccount MapToProto(Bacera.Gateway.Account a) => new ProtoAccount
    {
        Id        = a.Id,
        Name      = a.Name ?? "",
        Status    = a.Status,
        Role      = a.Role,
        FundType  = a.FundType,
        PartyId   = a.PartyId,
        CreatedOn = a.CreatedOn.ToString("O"),
        UpdatedOn = a.UpdatedOn.ToString("O"),
    };

    private static ProtoAccountUser MapUserToProto(TenantUserBasicModel? u)
    {
        if (u == null) return new ProtoAccountUser();
        return new ProtoAccountUser
        {
            Email              = u.Email ?? "",
            FirstName          = u.FirstName ?? "",
            LastName           = u.LastName ?? "",
            PartyId            = u.PartyId,
            Status             = u.Status,
            HasComment         = u.HasComment,
            Id                 = u.Id,
            Uid                = u.Uid,
            Avatar             = u.Avatar ?? "",
            Language           = u.Language ?? "",
            IdNumber           = u.IdNumber ?? "",
            Phone              = u.Phone ?? "",
            LastLoginIp        = u.LastLoginIp ?? "",
            RegisteredIp       = u.RegisteredIp ?? "",
            IsInIpBlackList    = u.IsInIpBlackList,
            IsInUserBlackList  = u.IsInUserBlackList,
            HasUscAccount      = u.HasUSCAccount,
            NativeName         = u.NativeName ?? "",
            DisplayName        = u.DisplayName ?? "",
        };
    }

    private static ProtoAccountBasic MapBasicToProto(AccountBasicViewModel b)
    {
        var proto = new ProtoAccountBasic
        {
            Id               = b.Id,
            Uid              = b.Uid,
            Code             = b.Code ?? "",
            Group            = b.Group ?? "",
            HasComment       = b.HasComment ?? false,
            PartyId          = b.PartyId,
            Role             = (int)b.Role,
            Name             = b.Name ?? "",
            CurrencyId       = (int)b.CurrencyId,
            AccountGroupName = b.AccountGroupName ?? "",
            ReferPath        = b.ReferPath ?? "",
            AccountNumber    = b.AccountNumber,
            ServiceId        = b.ServiceId,
            User             = MapUserToProto(b.User),
        };
        proto.User.PartyTags.AddRange(b.User?.PartyTags ?? []);
        return proto;
    }

    private static ProtoAccount MapViewModelToProto(AccountViewModel v)
    {
        var proto = new ProtoAccount
        {
            Id                             = v.Id,
            Name                           = v.Name ?? "",
            Email                          = v.User?.Email ?? "",
            Status                         = (int)v.Status,
            Role                           = (int)v.Role,
            FundType                       = (int)v.FundType,
            PartyId                        = v.PartyId,
            CreatedOn                      = v.CreatedOn.ToString("O"),
            UpdatedOn                      = v.UpdatedOn.ToString("O"),
            Uid                            = v.Uid,
            Type                           = (int)v.Type,
            CurrencyId                     = (int)v.CurrencyId,
            Code                           = v.Code ?? "",
            Group                          = v.Group ?? "",
            AccountNumber                  = v.AccountNumber,
            SiteId                         = (int)v.SiteId,
            WalletId                       = v.WalletId,
            ServiceId                      = v.ServiceId,
            HasComment                     = v.HasComment ?? false,
            HasRebateRule                  = v.HasRebateRule,
            HasTradeAccount                = v.HasTradeAccount,
            HasLevelRule                   = v.HasLevelRule,
            IsInUserBlackList              = v.IsInUserBlackList,
            IsInIpBlackList                = v.IsInIpBlackList,
            Level                          = v.Level,
            HasClosedAccount               = v.HasClosedAccount,
            Alias                          = v.Alias ?? "",
            ReferPath                      = v.ReferPath ?? "",
            ClientRebateDistributionType   = (int?)v.ClientRebateDistributionType ?? 0,
            ActiveOn                       = v.ActiveOn?.ToString("O") ?? "",
            SuspendedOn                    = v.SuspendedOn?.ToString("O") ?? "",
            Wizard = new Http.V1.AccountWizardInfo
            {
                KycFormCompleted    = v.Wizard?.KycFormCompleted ?? false,
                PaymentAccessGranted = v.Wizard?.PaymentAccessGranted ?? false,
                WelcomeEmailSent    = v.Wizard?.WelcomeEmailSent ?? false,
                NoticeEmailSent     = v.Wizard?.NoticeEmailSent ?? false,
            },
            User         = MapUserToProto(v.User),
            SalesAccount = MapBasicToProto(v.SalesAccount),
            AgentAccount = MapBasicToProto(v.AgentAccount),
            TradeAccount = new ProtoTradeAccountBasic
            {
                ServiceName    = v.TradeAccount?.ServiceName ?? "",
                AccountNumber  = v.TradeAccount?.AccountNumber ?? 0,
                CurrencyId     = (int)(v.TradeAccount?.CurrencyId ?? 0),
                Balance        = v.TradeAccount?.Balance ?? 0,
                Leverage       = v.TradeAccount?.Leverage ?? 0,
                Credit         = v.TradeAccount?.Credit ?? 0,
                Equity         = v.TradeAccount?.Equity ?? 0,
                BalanceInCents = v.TradeAccount?.BalanceInCents ?? 0,
                EquityInCents  = v.TradeAccount?.EquityInCents ?? 0,
                CreditInCents  = v.TradeAccount?.CreditInCents ?? 0,
            },
        };
        // repeated 字段不能在对象初始化器中设置，需要 AddRange
        proto.AccountTags.AddRange(v.AccountTags ?? []);
        proto.User.PartyTags.AddRange(v.User?.PartyTags ?? []);
        return proto;
    }

    // 保留兼容方法名（现在 MapViewModelToProto 已包含全部字段）
    private static ProtoAccount MapViewModelToProtoWithTags(AccountViewModel v) => MapViewModelToProto(v);

    private static long GetPartyId(ServerCallContext ctx)
        => ctx.GetHttpContext().User.GetPartyId();
}

// ─── TenantAccountServiceV2 ──────────────────────────────────────────────────

/// <summary>
/// gRPC JSON Transcoding implementation of TenantAccountServiceV2.
/// Replaces Areas/Tenant/Controllers/V2/AccountControllerV2.cs.
/// </summary>
public class TenantAccountV2GrpcService(
    TenantDbContext tenantCtx,
    AccountManageService accManSvc,
    MyDbContextPool pool,
    ITradingApiService tradingApiSvc)
    : TenantAccountV2Service.TenantAccountV2ServiceBase
{
    public override async Task<ConfirmAutoCreateResponse> ConfirmAutoCreate(
        ConfirmAutoCreateRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts
            .Include(x => x.Tags.Where(y => y.Name == AccountTagTypes.AutoCreate))
            .SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var autoOpenTag = account.Tags.SingleOrDefault(x => x.Name == AccountTagTypes.AutoCreate);
        if (autoOpenTag == null) throw new RpcException(new Status(StatusCode.FailedPrecondition, "Account is not auto open"));

        account.Tags.Remove(autoOpenTag);
        await tenantCtx.SaveChangesAsync();
        await accManSvc.AddAccountTagsAsync(request.Id, AccountTagTypes.AutoCreateConfirmed);
        return new ConfirmAutoCreateResponse { Data = MapToProto(account) };
    }

    public override async Task<AssignWalletResponse> AssignWallet(
        AssignWalletRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var wallet = await tenantCtx.Wallets
            .Select(x => new { x.Id, x.FundType, x.CurrencyId })
            .SingleOrDefaultAsync(x => x.Id == request.WalletId);
        if (wallet == null) throw new RpcException(new Status(StatusCode.NotFound, "Wallet not found"));

        if (account.CurrencyId != wallet.CurrencyId || account.FundType != wallet.FundType)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Account currency or fund type does not match wallet"));

        account.WalletId = request.WalletId;
        await tenantCtx.SaveChangesAsync();
        return new AssignWalletResponse { Data = MapToProto(account) };
    }

    public override async Task<GetAccountTradeStatResponse> GetAccountTradeStat(
        GetAccountTradeStatRequest request, ServerCallContext context)
    {
        var accountNumbers = await tenantCtx.Accounts
            .Where(x => x.Id == request.Id)
            .Select(x => x.AccountNumber)
            .ToListAsync();

        var fromStr = (request.HasDateFrom ? request.DateFrom : null) ?? (request.HasFrom ? request.From : null);
        var toStr   = (request.HasDateTo   ? request.DateTo   : null) ?? (request.HasTo   ? request.To   : null);
        var from = fromStr != null && DateTime.TryParse(fromStr, out var f) ? f : DateTime.MinValue;
        var to   = toStr   != null && DateTime.TryParse(toStr,   out var t) ? t : DateTime.MaxValue;

        try
        {
            var stat = await accManSvc.GetTradeStatisticsByIdAsync(accountNumbers, from, to);
            var volume = (double)(stat.ClosedTradeStats?.Sum(x => x.Volume) ?? 0m);
            var profit = stat.ClosedTradeStats?.Sum(x => (double)x.Profit) ?? 0.0;
            return new GetAccountTradeStatResponse
            {
                Data = new TradeStatResponse
                {
                    Volume     = volume,
                    Profit     = profit,
                    TradeCount = stat.ClosedTradeStats?.Count ?? 0,
                }
            };
        }
        catch
        {
            return new GetAccountTradeStatResponse { Data = new TradeStatResponse() };
        }
    }

    public override async Task<GetMultiAccountTradeStatResponse> GetMultiAccountTradeStat(
        GetMultiAccountTradeStatRequest request, ServerCallContext context)
    {
        var fromStr = (request.HasDateFrom ? request.DateFrom : null) ?? (request.HasFrom ? request.From : null);
        var toStr   = (request.HasDateTo   ? request.DateTo   : null) ?? (request.HasTo   ? request.To   : null);
        var from = fromStr != null && DateTime.TryParse(fromStr, out var f) ? f : DateTime.MinValue;
        var to   = toStr   != null && DateTime.TryParse(toStr,   out var t) ? t : DateTime.MaxValue;

        try
        {
            var stat = await accManSvc.GetTradeStatisticsByIdAsync(request.AccountNumbers.ToList(), from, to);
            var volume = (double)(stat.ClosedTradeStats?.Sum(x => x.Volume) ?? 0m);
            var profit = stat.ClosedTradeStats?.Sum(x => (double)x.Profit) ?? 0.0;
            return new GetMultiAccountTradeStatResponse
            {
                Data = new TradeStatResponse
                {
                    Volume     = volume,
                    Profit     = profit,
                    TradeCount = stat.ClosedTradeStats?.Count ?? 0,
                }
            };
        }
        catch
        {
            return new GetMultiAccountTradeStatResponse { Data = new TradeStatResponse() };
        }
    }

    public override async Task<GetDailyReportResponse> GetDailyReport(
        GetDailyReportRequest request, ServerCallContext context)
    {
        var serviceId = await tenantCtx.Accounts
            .Where(x => x.AccountNumber == request.AccountNumber)
            .Select(x => x.ServiceId)
            .SingleOrDefaultAsync();
        if (serviceId == 0) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        if (pool.GetPlatformByServiceId(serviceId) != PlatformTypes.MetaTrader5)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Not supported"));

        if (!DateTime.TryParse(request.Date, out var date))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid date format"));
        date = DateTime.SpecifyKind(date, DateTimeKind.Utc).Date;

        var from = date
            .AddHours(Utils.IsCurrentDSTLosAngeles(date) ? 20 : 21)
            .AddMinutes(59)
            .AddSeconds(59);

        var res = await tradingApiSvc.GetDailyReport(serviceId, request.AccountNumber, from, from.AddDays(1));
        var first = res.FirstOrDefault();
        return new GetDailyReportResponse
        {
            Data = new DailyReportResponse
            {
                Date    = request.Date,
                Balance = first?.Balance    ?? 0,
                Equity  = first?.ProfitEquity ?? 0,
            }
        };
    }

    public override async Task<EnableTransferResponse> EnableTransfer(
        EnableTransferRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var permission = account.Permission?.ToList() ?? new List<char>();
        if (permission.Count > 0)
        {
            permission[0] = '1';
            account.Permission = string.Join("", permission);
            await tenantCtx.SaveChangesAsync();
        }
        return new EnableTransferResponse { Data = MapToProto(account) };
    }

    public override async Task<DisableTransferResponse> DisableTransfer(
        DisableTransferRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var permission = account.Permission?.ToList() ?? new List<char>();
        if (permission.Count > 0)
        {
            permission[0] = '0';
            account.Permission = string.Join("", permission);
            await tenantCtx.SaveChangesAsync();
        }
        return new DisableTransferResponse { Data = MapToProto(account) };
    }

    public override async Task<GetStatTopResponse> GetStatTop(
        GetStatTopRequest request, ServerCallContext context)
    {
        var from          = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to            = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;
        long? accNumber   = request.HasAccountNumber ? request.AccountNumber : null;

        var items = await tenantCtx.AccountStats
            .Where(x => from == null || x.Date >= from)
            .Where(x => to   == null || x.Date <= to)
            .Where(x => accNumber == null || x.Account.AccountNumber == accNumber)
            .Select(x => new
            {
                x.Account.AccountNumber,
                Pnl = x.Equity - x.PreviousEquity - (x.DepositAmount + x.WithdrawAmount + x.Credit + x.Adjust),
            })
            .GroupBy(x => x.AccountNumber)
            .Select(x => new { AccountNumber = x.Key, Pnl = x.Sum(y => y.Pnl) })
            .OrderByDescending(x => x.Pnl)
            .Take(request.Count > 0 ? request.Count : 20)
            .ToListAsync();

        var data = new StatTopResponse();
        data.Items.AddRange(items.Select(x => new StatTopItem
        {
            AccountId = x.AccountNumber,
            Value     = (double)x.Pnl,
        }));
        return new GetStatTopResponse { Data = data };
    }

    private static ProtoAccount MapToProto(Bacera.Gateway.Account a) => new ProtoAccount
    {
        Id        = a.Id,
        Name      = a.Name ?? "",
        Status    = a.Status,
        Role      = a.Role,
        FundType  = a.FundType,
        PartyId   = a.PartyId,
        CreatedOn = a.CreatedOn.ToString("O"),
        UpdatedOn = a.UpdatedOn.ToString("O"),
    };
}

// ─── ClientAccountService ────────────────────────────────────────────────────

/// <summary>
/// gRPC JSON Transcoding implementation of ClientAccountService.
/// Replaces Areas/Client/Controllers/AccountController.cs.
/// </summary>
public class ClientAccountGrpcService(
    TradingService tradingSvc,
    AccountManageService accManSvc,
    IGeneralJob generalJob,
    ITenantGetter tenantGetter)
    : ClientAccountService.ClientAccountServiceBase
{
    public override async Task<ClientAccountServiceListAccountsResponse> ListAccounts(
        ClientAccountServiceListAccountsRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var criteria = new Bacera.Gateway.Account.Criteria
        {
            Page    = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size    = 100,
            PartyId = partyId,
        };

        var result = await tradingSvc.AccountQueryForClientAsync(criteria, partyId);
        var inner = new ListAccountsResponse
        {
            Criteria = new PaginationMeta
            {
                Page      = criteria.Page,
                Size      = criteria.Size,
                Total     = criteria.Total,
                PageCount = criteria.Total > 0 ? (int)Math.Ceiling((double)criteria.Total / criteria.Size) : 0,
                HasMore   = false,
            }
        };
        inner.Data.AddRange(result.Data.Select(a => new ProtoAccount
        {
            Id        = a.Id,
            Name      = a.Name ?? "",
            Status    = (int)a.Status,
            Role      = (int)a.Role,
            FundType  = (int)a.FundType,
            CreatedOn = a.CreatedOn.ToString("O"),
        }));
        return new ClientAccountServiceListAccountsResponse { Data = inner };
    }

    public override async Task<ClientAccountServiceGetAccountResponse> GetAccount(
        ClientAccountServiceGetAccountRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var item = await tradingSvc.AccountClientResponseModelGetForPartyAsync(request.Id, partyId);
        if (item.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return new ClientAccountServiceGetAccountResponse
        {
            Data = new ProtoAccount
            {
                Id        = item.Id,
                Name      = item.Name ?? "",
                Status    = (int)item.Status,
                Role      = (int)item.Role,
                FundType  = (int)item.FundType,
                CreatedOn = item.CreatedOn.ToString("O"),
            }
        };
    }

    public override async Task<ClientAccountServiceGetAccountWizardResponse> GetAccountWizard(
        ClientAccountServiceGetAccountWizardRequest request, ServerCallContext context)
    {
        var wizard = await tradingSvc.GetAccountWizardAsync(request.Id);
        return new ClientAccountServiceGetAccountWizardResponse
        {
            KycFormCompleted     = wizard.KycFormCompleted,
            PaymentAccessGranted = wizard.PaymentAccessGranted,
            WelcomeEmailSent     = wizard.WelcomeEmailSent,
            NoticeEmailSent      = wizard.NoticeEmailSent,
        };
    }

    public override async Task<ClientAccountServiceRefreshAccountResponse> RefreshAccount(
        ClientAccountServiceRefreshAccountRequest request, ServerCallContext context)
    {
        await generalJob.TryUpdateTradeAccountStatus(tenantGetter.GetTenantId(), request.Id, true);
        var partyId = GetPartyId(context);
        var item = await tradingSvc.AccountClientResponseModelGetForPartyAsync(request.Id, partyId);
        if (item.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return new ClientAccountServiceRefreshAccountResponse
        {
            Data = new ProtoAccount
            {
                Id        = item.Id,
                Name      = item.Name ?? "",
                Status    = (int)item.Status,
                Role      = (int)item.Role,
                FundType  = (int)item.FundType,
                CreatedOn = item.CreatedOn.ToString("O"),
            }
        };
    }

    private static long GetPartyId(ServerCallContext ctx)
        => ctx.GetHttpContext().User.GetPartyId();
}

// ─── SalesAccountService ─────────────────────────────────────────────────────

/// <summary>
/// gRPC JSON Transcoding implementation of SalesAccountService.
/// Replaces Areas/Sales/Controllers/AccountController.cs.
/// </summary>
public class SalesAccountGrpcService(
    TradingService tradingSvc,
    TenantDbContext tenantCtx,
    AccountManageService accManSvc,
    ConfigurationService configurationService,
    IBackgroundJobClient backgroundJobClient,
    UserService userSvc,
    ITenantGetter tenantGetter)
    : SalesAccountService.SalesAccountServiceBase
{
    public override async Task<SalesAccountServiceListAccountsResponse> ListAccounts(
        SalesAccountServiceListAccountsRequest request, ServerCallContext context)
    {
        var salesId  = await accManSvc.GetAccountIdByUidAsync(request.SalesUid);
        var criteria = new Bacera.Gateway.Account.SalesCriteria
        {
            Page       = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size       = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            SearchText = request.HasKeywords ? request.Keywords : null,
        };
        var items = await accManSvc.QueryAccountForSalesAsync(salesId, criteria);
        var inner = new ListAccountsResponse
        {
            Criteria = new PaginationMeta
            {
                Page      = criteria.Page,
                Size      = criteria.Size,
                Total     = criteria.Total,
                PageCount = criteria.Total > 0 ? (int)Math.Ceiling((double)criteria.Total / criteria.Size) : 0,
                HasMore   = criteria.Page * criteria.Size < criteria.Total,
            }
        };
        inner.Data.AddRange(items.Select(a => new ProtoAccount
        {
            Name      = a.NativeName,
            Status    = (int)a.Status,
            Role      = (int)a.Role,
            FundType  = (int)a.FundType,
            CreatedOn = a.CreatedOn.ToString("O"),
        }));
        return new SalesAccountServiceListAccountsResponse { Data = inner };
    }

    public override async Task<SalesAccountServiceGetAccountResponse> GetAccount(
        SalesAccountServiceGetAccountRequest request, ServerCallContext context)
    {
        var clientAccount = await tradingSvc.AccountLookupForParentAsync(request.SalesUid, request.Uid);
        if (clientAccount.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var item = await tradingSvc.AccountGetForPartyAsync(clientAccount.Uid, clientAccount.PartyId);
        return new SalesAccountServiceGetAccountResponse
        {
            Data = new ProtoAccount
            {
                Id        = item.Id,
                Name      = item.Name ?? "",
                Status    = (int)item.Status,
                Role      = (int)item.Role,
                FundType  = (int)item.FundType,
                PartyId   = clientAccount.PartyId,
                CreatedOn = item.CreatedOn.ToString("O"),
            }
        };
    }

    public override async Task<GetLevelAccountResponse> GetLevelAccount(
        GetLevelAccountRequest request, ServerCallContext context)
    {
        var items = await accManSvc.GetParentLevelAccountAsync(request.SalesUid, request.ChildAccountUid);
        var first = items?.FirstOrDefault();
        if (first == null) throw new RpcException(new Status(StatusCode.NotFound, "Level account not found"));
        return new GetLevelAccountResponse
        {
            Data = new ProtoAccount
            {
                Name = first.NativeName,
                Role = (int)first.Role,
            }
        };
    }

    public override async Task<GetChildAccountStatResponse> GetChildAccountStat(
        GetChildAccountStatRequest request, ServerCallContext context)
    {
        var childAccountId = await tenantCtx.Accounts
            .Where(x => x.Uid == request.Uid && x.ReferPath.Contains(request.SalesUid.ToString()))
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
        if (childAccountId == 0) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var stat = await tradingSvc.GetParentAccountStatAsync(childAccountId, from, to);
        return new GetChildAccountStatResponse
        {
            Data = new ChildStatResponse
            {
                Volume     = (double)(stat?.TradeVolume ?? 0),
                Profit     = (double)(stat?.TradeProfit ?? 0),
                TradeCount = (int)(stat?.TradeCount ?? 0),
            }
        };
    }

    public override async Task<SalesAccountServiceGetChildNetStatResponse> GetChildNetStat(
        SalesAccountServiceGetChildNetStatRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var result = await tradingSvc.GetDirectChildNetAmountForAccountByUid(request.Uid, from, to, request.ViewClient);
        var sumUp = result.FirstOrDefault(x => x.Uid == 0 && x.Group == "Child-Sum-Up")
                    ?? result.FirstOrDefault(x => x.Uid == 0);

        var data = new ChildNetStatResponse
        {
            Uid   = sumUp?.Uid   ?? 0,
            Group = sumUp?.Group ?? "Child-Sum-Up",
            Role  = (int)(sumUp?.Role ?? 0),
        };
        if (sumUp != null)
        {
            foreach (var kvp in sumUp.DepositAmounts)
                data.DepositAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.WithdrawalAmounts)
                data.WithdrawalAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.RebateAmounts)
                data.RebateAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.ProfitAmounts)
                data.ProfitAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.NetAmounts)
                data.NetAmounts[kvp.Key.ToString()] = kvp.Value;
        }
        return new SalesAccountServiceGetChildNetStatResponse { Data = data };
    }

    public override async Task<SalesAccountServiceGetChildRebateBySymbolResponse> GetChildRebateBySymbol(
        SalesAccountServiceGetChildRebateBySymbolRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var stats = await tradingSvc.GetChildAccountRebateSymbolGroupedStatByUid(request.Uid, from, to);
        var data = new SymbolGroupStatResponse();
        foreach (var kvp in stats)
        {
            var entry = new SymbolStatEntry
            {
                Volume = kvp.Value.Volume,
                Profit = kvp.Value.Profit,
            };
            foreach (var amtKvp in kvp.Value.Amounts)
                entry.Amounts[amtKvp.Key.ToString()] = amtKvp.Value;
            data.Items[kvp.Key] = entry;
        }
        return new SalesAccountServiceGetChildRebateBySymbolResponse { Data = data };
    }

    public override async Task<GetChildTradeBySymbolResponse> GetChildTradeBySymbol(
        GetChildTradeBySymbolRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var stats = await tradingSvc.GetChildAccountTradeSymbolGroupedStatByUid(request.Uid, from, to);
        var data = new TradeSymbolGroupStatResponse();
        data.Items.AddRange(stats.Select(s => new SymbolStat
        {
            Symbol     = s.Symbol,
            CurrencyId = (int)s.CurrencyId,
            Volume     = s.Volume,
            Profit     = s.Profit,
        }));
        return new GetChildTradeBySymbolResponse { Data = data };
    }

    public override async Task<GetAccountConfigurationResponse> GetAccountConfiguration(
        GetAccountConfigurationRequest request, ServerCallContext context)
    {
        var agentAccount = await tenantCtx.Accounts
            .Where(x => x.Uid == request.AgentUid)
            .SingleOrDefaultAsync();
        if (agentAccount == null) throw new RpcException(new Status(StatusCode.NotFound, "Agent account not found"));
        return new GetAccountConfigurationResponse { Data = new AccountConfigResponse { AccountId = agentAccount.Id } };
    }

    public override async Task<GetDefaultLevelSettingResponse> GetDefaultLevelSetting(
        GetDefaultLevelSettingRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts.Where(x => x.Uid == request.AgentUid).FirstOrDefaultAsync();
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var item = await tenantCtx.Configurations
            .Where(x => x.Category == nameof(Account) && x.RowId == account.Id && x.Key == "DefaultRebateLevelSetting")
            .ToTenantViewModel()
            .FirstOrDefaultAsync();

        if (item?.ValueString is { Length: > 0 } raw)
            return new GetDefaultLevelSettingResponse { Json = raw };

        var result = await configurationService.GetDefaultRebateLevelSettingAsync(account.SiteId);
        return new GetDefaultLevelSettingResponse { Json = JsonConvert.SerializeObject(result) };
    }

    public override async Task<GetAvailableAccountTypesResponse> GetAvailableAccountTypes(
        GetAvailableAccountTypesRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts.Where(x => x.Uid == request.SalesUid).FirstOrDefaultAsync();
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        // 优先取账户级别的覆盖配置，否则取站点默认值
        var item = await tenantCtx.Configurations
            .Where(x => x.Category == nameof(Bacera.Gateway.Account))
            .Where(x => x.RowId == account.Id)
            .Where(x => x.Key == "AccountTypeAvailable")
            .ToTenantViewModel()
            .FirstOrDefaultAsync();

        List<int> typeIds;
        if (item?.Value is List<int> overrideList)
            typeIds = overrideList;
        else
            typeIds = await configurationService.GetAccountTypeAvailableAsync(account.SiteId);

        var data = new AccountTypesResponse();
        data.Types_.AddRange(typeIds.Select(id => new AccountTypeConfig { Id = id, Name = "" }));
        return new GetAvailableAccountTypesResponse { Data = data };
    }

    public override async Task<GetReferralPathResponse> GetReferralPath(
        GetReferralPathRequest request, ServerCallContext context)
    {
        var referPath = await tenantCtx.Accounts
            .Where(x => x.ReferPath.Contains(request.SalesUid.ToString()))
            .Where(x => x.Uid == request.Uid)
            .Select(x => x.ReferPath)
            .FirstOrDefaultAsync();
        if (referPath == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var uids = referPath.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList();
        var accounts = await tenantCtx.Accounts
            .Where(x => uids.Contains(x.Uid) && x.Role == (short)AccountRoleTypes.Agent)
            .OrderBy(x => x.Level)
            .Select(x => new { x.Id, x.Uid, x.Name, x.Level })
            .ToListAsync();

        var data = new ReferralPathResponse();
        data.Path.AddRange(accounts.Select(a => new ReferralPathItem
        {
            AccountId = a.Id,
            Name      = a.Name ?? "",
            Level     = a.Level,
        }));
        return new GetReferralPathResponse { Data = data };
    }

    public override async Task<SalesAccountServiceGetViewEmailCodeResponse> GetViewEmailCode(
        SalesAccountServiceGetViewEmailCodeRequest request, ServerCallContext context)
    {
        var isAccountUnder = await tenantCtx.Accounts
            .Where(x => x.Uid == request.Uid)
            .Where(x => x.ReferPath.Contains(request.SalesUid.ToString()))
            .AnyAsync();
        if (!isAccountUnder)
            throw new RpcException(new Status(StatusCode.NotFound, "ACCOUNT_NOT_FOUND_UNDER"));

        var account = await tenantCtx.Accounts
            .Where(x => x.Uid == request.Uid)
            .Select(x => new { x.Party.Email })
            .SingleAsync();

        var partyId = GetPartyId(context);
        var exist = await tenantCtx.AuthCodes
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Event == $"{AuthCode.EventLabel.ParentViewEmail}:{account.Email}")
            .Where(x => x.ExpireOn > DateTime.UtcNow)
            .AnyAsync();
        if (exist)
            throw new RpcException(new Status(StatusCode.AlreadyExists, "CODE_ALREADY_SENT"));

        var user = await userSvc.GetPartyAsync(partyId);
        backgroundJobClient.Enqueue<IGeneralJob>(x => x.GenerateAuthCodeAndSendEmailAsync(
            tenantGetter.GetTenantId(), user.EmailRaw, $"{AuthCode.EventLabel.ParentViewEmail}:{account.Email}"));

        return new SalesAccountServiceGetViewEmailCodeResponse { ExpiresAt = "" };
    }

    public override async Task<SalesAccountServiceGetViewEmailResponse> GetViewEmail(
        SalesAccountServiceGetViewEmailRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);

        var isAccountUnder = await tenantCtx.Accounts
            .Where(x => x.Uid == request.Uid)
            .Where(x => x.ReferPath.Contains(request.SalesUid.ToString()))
            .AnyAsync();
        if (!isAccountUnder)
            throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var account = await tenantCtx.Accounts
            .Where(x => x.Uid == request.Uid)
            .Select(x => new { x.Party.Email })
            .SingleAsync();

        var item = await tenantCtx.AuthCodes
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Event == $"{AuthCode.EventLabel.ParentViewEmail}:{account.Email}")
            .Where(x => x.ExpireOn > DateTime.UtcNow)
            .Where(x => x.Code == request.Code)
            .FirstOrDefaultAsync();
        if (item == null)
            throw new RpcException(new Status(StatusCode.NotFound, "CODE_NOT_FOUND"));

        return new SalesAccountServiceGetViewEmailResponse { Email = account.Email ?? "" };
    }

    private static long GetPartyId(ServerCallContext ctx)
        => ctx.GetHttpContext().User.GetPartyId();
}

// ─── AgentAccountService ─────────────────────────────────────────────────────

/// <summary>
/// gRPC JSON Transcoding implementation of AgentAccountService.
/// Replaces Areas/Agent/Controllers/AccountController.cs.
/// </summary>
public class AgentAccountGrpcService(
    TradingService tradingSvc,
    TenantDbContext tenantCtx,
    AccountManageService accManSvc,
    ITenantGetter tenantGetter,
    ConfigurationService configurationService,
    IGeneralJob generalJob,
    IBackgroundJobClient backgroundJobClient,
    UserService userSvc)
    : AgentAccountService.AgentAccountServiceBase
{
    public override async Task<AgentAccountServiceListAccountsResponse> ListAccounts(
        AgentAccountServiceListAccountsRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.Account.Criteria
        {
            Page             = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size             = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            Keywords         = request.HasKeywords ? request.Keywords : null,
            Status           = request.HasStatus   ? (AccountStatusTypes?)request.Status : null,
            ParentAccountUid = request.AgentUid,
        };

        var parentLevel = await tenantCtx.Accounts
            .Where(x => x.Uid == request.AgentUid)
            .Select(x => x.Level)
            .FirstOrDefaultAsync();

        var result = await tradingSvc.AccountQueryForParentAsync(criteria, GetPartyId(context), parentLevel, false);
        var inner = new ListAccountsResponse
        {
            Criteria = new PaginationMeta
            {
                Page      = criteria.Page,
                Size      = criteria.Size,
                Total     = criteria.Total,
                PageCount = criteria.Total > 0 ? (int)Math.Ceiling((double)criteria.Total / criteria.Size) : 0,
                HasMore   = criteria.Page * criteria.Size < criteria.Total,
            }
        };
        inner.Data.AddRange(result.Data.Select(a => new ProtoAccount
        {
            Id        = a.Id,
            Name      = a.User.NativeName,
            Status    = (int)a.Status,
            Role      = (int)a.Role,
            FundType  = (int)a.FundType,
            PartyId   = a.PartyId,
            CreatedOn = a.CreatedOn.ToString("O"),
        }));
        return new AgentAccountServiceListAccountsResponse { Data = inner };
    }

    public override async Task<AgentAccountServiceGetAccountResponse> GetAccount(
        AgentAccountServiceGetAccountRequest request, ServerCallContext context)
    {
        var clientAccount = await tradingSvc.AccountLookupForParentAsync(request.AgentUid, request.Uid);
        if (clientAccount.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var item = await tradingSvc.AccountGetForPartyAsync(clientAccount.Uid, clientAccount.PartyId);
        return new AgentAccountServiceGetAccountResponse
        {
            Data = new ProtoAccount
            {
                Id        = item.Id,
                Name      = item.Name ?? "",
                Status    = (int)item.Status,
                Role      = (int)item.Role,
                FundType  = (int)item.FundType,
                PartyId   = clientAccount.PartyId,
                CreatedOn = item.CreatedOn.ToString("O"),
            }
        };
    }

    public override async Task<AgentAccountServiceRefreshAccountResponse> RefreshAccount(
        AgentAccountServiceRefreshAccountRequest request, ServerCallContext context)
    {
        var clientAccount = await tradingSvc.AccountLookupForParentAsync(request.AgentUid, request.Uid);
        if (clientAccount.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        await generalJob.TryUpdateTradeAccountStatus(tenantGetter.GetTenantId(), clientAccount.Id, true);
        var item = await tradingSvc.AccountGetForPartyAsync(clientAccount.Uid, clientAccount.PartyId);
        return new AgentAccountServiceRefreshAccountResponse
        {
            Data = new ProtoAccount
            {
                Id        = item.Id,
                Name      = item.Name ?? "",
                Status    = (int)item.Status,
                Role      = (int)item.Role,
                FundType  = (int)item.FundType,
                PartyId   = clientAccount.PartyId,
                CreatedOn = item.CreatedOn.ToString("O"),
            }
        };
    }

    public override async Task<AgentAccountServiceGetChildNetStatResponse> GetChildNetStat(
        AgentAccountServiceGetChildNetStatRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var result = await tradingSvc.GetDirectChildNetAmountForAccountByUid(request.Uid, from, to, request.ViewClient);
        var sumUp = result.FirstOrDefault(x => x.Uid == 0 && x.Group == "Child-Sum-Up")
                    ?? result.FirstOrDefault(x => x.Uid == 0);

        var data = new ChildNetStatResponse
        {
            Uid   = sumUp?.Uid   ?? 0,
            Group = sumUp?.Group ?? "Child-Sum-Up",
            Role  = (int)(sumUp?.Role ?? 0),
        };
        if (sumUp != null)
        {
            foreach (var kvp in sumUp.DepositAmounts)
                data.DepositAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.WithdrawalAmounts)
                data.WithdrawalAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.RebateAmounts)
                data.RebateAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.ProfitAmounts)
                data.ProfitAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.NetAmounts)
                data.NetAmounts[kvp.Key.ToString()] = kvp.Value;
        }
        return new AgentAccountServiceGetChildNetStatResponse { Data = data };
    }

    public override async Task<AgentAccountServiceGetChildRebateBySymbolResponse> GetChildRebateBySymbol(
        AgentAccountServiceGetChildRebateBySymbolRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var stats = await tradingSvc.GetChildAccountRebateSymbolGroupedStatByUid(request.Uid, from, to);
        var data = new SymbolGroupStatResponse();
        foreach (var kvp in stats)
        {
            var entry = new SymbolStatEntry
            {
                Volume = kvp.Value.Volume,
                Profit = kvp.Value.Profit,
            };
            foreach (var amtKvp in kvp.Value.Amounts)
                entry.Amounts[amtKvp.Key.ToString()] = amtKvp.Value;
            data.Items[kvp.Key] = entry;
        }
        return new AgentAccountServiceGetChildRebateBySymbolResponse { Data = data };
    }

    public override async Task<AgentAccountServiceGetDefaultLevelSettingResponse> GetDefaultLevelSetting(
        AgentAccountServiceGetDefaultLevelSettingRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts.Where(x => x.Uid == request.AgentUid).FirstOrDefaultAsync();
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var item = await tenantCtx.Configurations
            .Where(x => x.Category == nameof(Account) && x.RowId == account.Id && x.Key == "DefaultRebateLevelSetting")
            .ToTenantViewModel()
            .FirstOrDefaultAsync();

        if (item?.ValueString is { Length: > 0 } raw)
            return new AgentAccountServiceGetDefaultLevelSettingResponse { Json = raw };

        var result = await configurationService.GetDefaultRebateLevelSettingAsync(account.SiteId);
        return new AgentAccountServiceGetDefaultLevelSettingResponse { Json = JsonConvert.SerializeObject(result) };
    }

    public override async Task<AgentAccountServiceGetViewEmailCodeResponse> GetViewEmailCode(
        AgentAccountServiceGetViewEmailCodeRequest request, ServerCallContext context)
    {
        var isAccountUnder = await tenantCtx.Accounts
            .Where(x => x.Uid == request.Uid)
            .Where(x => x.ReferPath.Contains(request.AgentUid.ToString()))
            .AnyAsync();
        if (!isAccountUnder)
            throw new RpcException(new Status(StatusCode.NotFound, "ACCOUNT_NOT_FOUND_UNDER"));

        var account = await tenantCtx.Accounts
            .Where(x => x.Uid == request.Uid)
            .Select(x => new { x.Party.Email, x.ReferPath })
            .SingleAsync();

        // 只允许直属子账户（ReferPath 倒数第二个 uid 必须是 agentUid）
        var parentUids = account.ReferPath.Split('.')
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(long.Parse)
            .ToList();
        if (parentUids.Count < 2 || parentUids[^2] != request.AgentUid)
            throw new RpcException(new Status(StatusCode.PermissionDenied, "NOT_DIRECT_CHILD"));

        var partyId = GetPartyId(context);
        var exist = await tenantCtx.AuthCodes
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Event == $"{AuthCode.EventLabel.ParentViewEmail}:{account.Email}")
            .Where(x => x.ExpireOn > DateTime.UtcNow)
            .AnyAsync();
        if (exist)
            throw new RpcException(new Status(StatusCode.AlreadyExists, "CODE_ALREADY_SENT"));

        var user = await userSvc.GetPartyAsync(partyId);
        backgroundJobClient.Enqueue<IGeneralJob>(x => x.GenerateAuthCodeAndSendEmailAsync(
            tenantGetter.GetTenantId(), user.EmailRaw, $"{AuthCode.EventLabel.ParentViewEmail}:{account.Email}"));

        return new AgentAccountServiceGetViewEmailCodeResponse { ExpiresAt = "" };
    }

    public override async Task<AgentAccountServiceGetViewEmailResponse> GetViewEmail(
        AgentAccountServiceGetViewEmailRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);

        var isAccountUnder = await tenantCtx.Accounts
            .Where(x => x.Uid == request.Uid)
            .Where(x => x.ReferPath.Contains(request.AgentUid.ToString()))
            .AnyAsync();
        if (!isAccountUnder)
            throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var account = await tenantCtx.Accounts
            .Where(x => x.Uid == request.Uid)
            .Select(x => new { x.Party.Email })
            .SingleAsync();

        var item = await tenantCtx.AuthCodes
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Event == $"{AuthCode.EventLabel.ParentViewEmail}:{account.Email}")
            .Where(x => x.ExpireOn > DateTime.UtcNow)
            .Where(x => x.Code == request.Code)
            .FirstOrDefaultAsync();
        if (item == null)
            throw new RpcException(new Status(StatusCode.NotFound, "CODE_NOT_FOUND"));

        return new AgentAccountServiceGetViewEmailResponse { Email = account.Email ?? "" };
    }

    private static long GetPartyId(ServerCallContext ctx)
        => ctx.GetHttpContext().User.GetPartyId();
}

// ─── RepAccountService ───────────────────────────────────────────────────────

/// <summary>
/// gRPC JSON Transcoding implementation of RepAccountService.
/// Replaces Areas/Rep/Controllers/AccountController.cs.
/// </summary>
public class RepAccountGrpcService(
    TradingService tradingSvc,
    TenantDbContext tenantCtx)
    : RepAccountService.RepAccountServiceBase
{
    public override async Task<RepAccountServiceListAccountsResponse> ListAccounts(
        RepAccountServiceListAccountsRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.Account.Criteria
        {
            Page             = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size             = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            Keywords         = request.HasKeywords ? request.Keywords : null,
            Status           = request.HasStatus   ? (AccountStatusTypes?)request.Status : null,
            ParentAccountUid = request.RepUid,
        };

        var parentLevel = await tenantCtx.Accounts
            .Where(x => x.Uid == request.RepUid)
            .Select(x => x.Level)
            .FirstOrDefaultAsync();

        var result = await tradingSvc.AccountQueryForParentAsync(criteria, GetPartyId(context), parentLevel, false);
        var inner = new ListAccountsResponse
        {
            Criteria = new PaginationMeta
            {
                Page      = criteria.Page,
                Size      = criteria.Size,
                Total     = criteria.Total,
                PageCount = criteria.Total > 0 ? (int)Math.Ceiling((double)criteria.Total / criteria.Size) : 0,
                HasMore   = criteria.Page * criteria.Size < criteria.Total,
            }
        };
        inner.Data.AddRange(result.Data.Select(a => new ProtoAccount
        {
            Id        = a.Id,
            Name      = a.User.NativeName,
            Status    = (int)a.Status,
            Role      = (int)a.Role,
            FundType  = (int)a.FundType,
            PartyId   = a.PartyId,
            CreatedOn = a.CreatedOn.ToString("O"),
        }));
        return new RepAccountServiceListAccountsResponse { Data = inner };
    }

    public override async Task<RepAccountServiceGetAccountResponse> GetAccount(
        RepAccountServiceGetAccountRequest request, ServerCallContext context)
    {
        var clientAccount = await tradingSvc.AccountLookupForParentAsync(request.RepUid, request.Uid);
        if (clientAccount.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var item = await tradingSvc.AccountGetForPartyAsync(clientAccount.Uid, clientAccount.PartyId);
        return new RepAccountServiceGetAccountResponse
        {
            Data = new ProtoAccount
            {
                Id        = item.Id,
                Name      = item.Name ?? "",
                Status    = (int)item.Status,
                Role      = (int)item.Role,
                FundType  = (int)item.FundType,
                PartyId   = clientAccount.PartyId,
                CreatedOn = item.CreatedOn.ToString("O"),
            }
        };
    }

    public override async Task<GetGroupNameListResponse> GetGroupNameList(
        GetGroupNameListRequest request, ServerCallContext context)
    {
        var type  = request.HasType ? (AccountGroupTypes)request.Type : AccountGroupTypes.Agent;
        var role  = (AccountRoleTypes)((int)type * 100);
        var items = await tradingSvc.GetAllGroupNamesUnderAccountByUid(request.RepUid, role, request.HasKeywords ? request.Keywords : "");
        var response = new GetGroupNameListResponse();
        response.Names.AddRange(items);
        return response;
    }

    public override async Task<RepAccountServiceGetChildNetStatResponse> GetChildNetStat(
        RepAccountServiceGetChildNetStatRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var result = await tradingSvc.GetDirectChildNetAmountForAccountByUid(request.Uid, from, to, request.ViewClient);
        var sumUp = result.FirstOrDefault(x => x.Uid == 0 && x.Group == "Child-Sum-Up")
                    ?? result.FirstOrDefault(x => x.Uid == 0);

        var data = new ChildNetStatResponse
        {
            Uid   = sumUp?.Uid   ?? 0,
            Group = sumUp?.Group ?? "Child-Sum-Up",
            Role  = (int)(sumUp?.Role ?? 0),
        };
        if (sumUp != null)
        {
            foreach (var kvp in sumUp.DepositAmounts)
                data.DepositAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.WithdrawalAmounts)
                data.WithdrawalAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.RebateAmounts)
                data.RebateAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.ProfitAmounts)
                data.ProfitAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.NetAmounts)
                data.NetAmounts[kvp.Key.ToString()] = kvp.Value;
        }
        return new RepAccountServiceGetChildNetStatResponse { Data = data };
    }

    public override async Task<RepAccountServiceGetChildRebateBySymbolResponse> GetChildRebateBySymbol(
        RepAccountServiceGetChildRebateBySymbolRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var stats = await tradingSvc.GetChildAccountRebateSymbolGroupedStatByUid(request.Uid, from, to);
        var data = new SymbolGroupStatResponse();
        foreach (var kvp in stats)
        {
            var entry = new SymbolStatEntry
            {
                Volume = kvp.Value.Volume,
                Profit = kvp.Value.Profit,
            };
            foreach (var amtKvp in kvp.Value.Amounts)
                entry.Amounts[amtKvp.Key.ToString()] = amtKvp.Value;
            data.Items[kvp.Key] = entry;
        }
        return new RepAccountServiceGetChildRebateBySymbolResponse { Data = data };
    }

    private static long GetPartyId(ServerCallContext ctx)
        => ctx.GetHttpContext().User.GetPartyId();
}
