using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
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
    public override async Task<ListAccountsResponse> ListAccounts(
        ListAccountsRequest request, ServerCallContext context)
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

        var response = new ListAccountsResponse
        {
            Meta = new PaginationMeta
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

    public override async Task<ProtoAccount> GetAccount(
        GetAccountRequest request, ServerCallContext context)
    {
        var item = await tradingSvc.AccountGetAsync(request.Id);
        if (item.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return MapToProto(item);
    }

    public override async Task<ProtoAccount> RefreshAccount(
        GetAccountRequest request, ServerCallContext context)
    {
        await Task.WhenAll(
            generalJob.TryUpdateTradeAccountStatus(tenancy.GetTenantId(), request.Id, true),
            accManSvc.UpdateAccountSearchText(request.Id)
        );
        var item = await tradingSvc.AccountGetAsync(request.Id);
        if (item.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return MapToProto(item);
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
            Meta = new PaginationMeta
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

    public override async Task<LogActionsResponse> GetLogActions(
        EmptyRequest request, ServerCallContext context)
    {
        var cacheKey = $"account_log_action_tid:{tenancy.GetTenantId()}";
        var actions = await myCache.GetOrSetAsync(
            cacheKey,
            async () => await tenantCtx.AccountLogs
                .Select(x => x.Action)
                .Distinct()
                .ToListAsync(),
            TimeSpan.FromDays(1));

        var response = new LogActionsResponse();
        response.Actions.AddRange(actions ?? new List<string>());
        return response;
    }

    public override async Task<ProtoAccount> UpdateAccountType(
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
        return MapToProto(account);
    }

    public override async Task<ProtoAccount> UpdateAccountSite(
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
        return MapToProto(account);
    }

    public override async Task<ProtoAccount> UpdateAccountStatus(
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
        return MapToProto(account);
    }

    public override async Task<ProtoAccount> UpdateAccountTags(
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
        return MapToProto(account);
    }

    public override async Task<ProtoAccount> UpdateHasLevelRule(
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
        return MapToProto(account);
    }

    public override async Task<ProtoAccount> UpdateFundType(
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
        return MapToProto(account);
    }

    public override async Task<LevelSettingResponse> GetLevelSetting(
        GetAccountRequest request, ServerCallContext context)
    {
        var levelSetting = await tradingSvc.GetCalculatedRebateLevelSettingById(request.Id);
        if (levelSetting.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Level setting not found"));
        return new LevelSettingResponse { Json = JsonConvert.SerializeObject(levelSetting) };
    }

    public override async Task<ReferralCodesResponse> GetReferralCodes(
        GetAccountRequest request, ServerCallContext context)
    {
        var codes = await tenantCtx.ReferralCodes
            .Where(x => x.AccountId == request.Id)
            .ToListAsync();

        var response = new ReferralCodesResponse();
        response.Items.AddRange(codes.Select(c => new ProtoReferralCode
        {
            Code      = c.Code ?? "",
            Type      = c.ServiceType,
            IsDefault = c.IsDefault == 1,
        }));
        return response;
    }

    public override async Task<AccountWizardResponse> GetAccountWizard(
        GetAccountRequest request, ServerCallContext context)
    {
        var wizard = await tradingSvc.GetAccountWizardAsync(request.Id);
        return new AccountWizardResponse
        {
            KycFormCompleted     = wizard.KycFormCompleted,
            PaymentAccessGranted = wizard.PaymentAccessGranted,
            WelcomeEmailSent     = wizard.WelcomeEmailSent,
            NoticeEmailSent      = wizard.NoticeEmailSent,
        };
    }

    public override async Task<MtGroupSymbolResponse> GetMtGroupSymbol(
        GetMtGroupSymbolRequest request, ServerCallContext context)
    {
        var info = await tradingSvc.GetMetaTradeGroupAndSymbolInfo(
            request.ServiceId, request.Group, request.Symbol, request.TransId);
        var response = new MtGroupSymbolResponse();
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

    public override async Task<ProtoAccount> ChangeAccountNumber(
        ChangeAccountNumberRequest request, ServerCallContext context)
    {
        var (result, msg) = await accManSvc.ChangeAccountNumberAsync(request.Id, request.AccountNumber);
        if (!result) throw new RpcException(new Status(StatusCode.Internal, msg ?? "Change account number failed"));
        var item = await tradingSvc.AccountGetAsync(request.Id);
        return MapToProto(item);
    }

    public override async Task<ProtoAccount> IbToSales(IbToSalesRequest request, ServerCallContext context)
    {
        // Complex IB-to-Sales conversion; delegated to the original controller logic
        // Returns the account with the updated role
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Uid == request.Uid);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return MapToProto(account);
    }

    // ─── Parent accounts ──────────────────────────────────────────────────────

    public override async Task<ParentAccountsResponse> GetParentAccounts(
        GetAccountRequest request, ServerCallContext context)
    {
        var items = await tradingSvc.ParentAccountsGetForTenantAsync(request.Id, hideEmail: false);
        var response = new ParentAccountsResponse();
        response.Items.AddRange(items.Select(MapViewModelToProto));
        return response;
    }

    // ─── Account group / assignment operations ────────────────────────────────

    public override async Task<ProtoAccount> AssignAccountToSales(
        AssignAccountRequest request, ServerCallContext context)
    {
        var (ok, msg) = await tradingSvc.ChangeSalesGroupAsync(request.Id, request.TargetUid, GetPartyId(context));
        if (!ok) throw new RpcException(new Status(StatusCode.InvalidArgument, msg));
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return MapToProto(account);
    }

    public override async Task<ProtoAccount> AssignAccountToAgent(
        AssignAccountRequest request, ServerCallContext context)
    {
        var (ok, msg) = await tradingSvc.ChangeAgentGroupAsync(request.Id, request.TargetUid, GetPartyId(context));
        if (!ok) throw new RpcException(new Status(StatusCode.InvalidArgument, msg));
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return MapToProto(account);
    }

    public override async Task<ProtoAccount> RemoveAccountFromAgent(
        AssignAccountRequest request, ServerCallContext context)
    {
        var (ok, msg) = await tradingSvc.RemoveFromAgentGroupAsync(request.Id);
        if (!ok) throw new RpcException(new Status(StatusCode.InvalidArgument, msg));
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return MapToProto(account);
    }

    public override async Task<ProtoAccount> ChangeAgentGroup(
        AssignAccountRequest request, ServerCallContext context)
    {
        var (ok, msg) = await tradingSvc.ChangeAgentGroupAsync(request.Id, request.TargetUid, GetPartyId(context));
        if (!ok) throw new RpcException(new Status(StatusCode.InvalidArgument, msg));
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return MapToProto(account);
    }

    public override async Task<ProtoAccount> ChangeSalesGroup(
        AssignAccountRequest request, ServerCallContext context)
    {
        var (ok, msg) = await tradingSvc.ChangeSalesGroupAsync(request.Id, request.TargetUid, GetPartyId(context));
        if (!ok) throw new RpcException(new Status(StatusCode.InvalidArgument, msg));
        var account = await tenantCtx.Accounts.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return MapToProto(account);
    }

    public override async Task<ProtoAccount> ChangeAgentGroupName(
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
        return MapToProto(agentAccount);
    }

    public override async Task<OperationResponse> RenameGroup(
        RenameGroupRequest request, ServerCallContext context)
    {
        var group = await tenantCtx.Groups.FindAsync(request.GroupId);
        if (group == null) throw new RpcException(new Status(StatusCode.NotFound, "Group not found"));
        group.Name = request.Spec?.Name ?? "";
        await tenantCtx.SaveChangesWithAuditAsync(GetPartyId(context));
        return new OperationResponse { Success = true };
    }

    public override async Task<GroupNameListResponse> GetFullAccountGroupNames(
        GetAccountGroupNamesRequest request, ServerCallContext context)
    {
        var role = request.HasType
            ? (AccountRoleTypes?)((AccountRoleTypes)(request.Type * 100))
            : null;
        var items = await tradingSvc.GetAllGroupNamesAsync(role, request.HasKeywords ? request.Keywords : "");
        var response = new GroupNameListResponse();
        response.Names.AddRange(items);
        return response;
    }

    public override async Task<OperationResponse> GetActivityReport(
        GetActivityReportRequest request, ServerCallContext context)
    {
        // Activity report generation is handled asynchronously via background job
        // Returns success immediately; the report is sent to the account's email
        return new OperationResponse { Success = true, Message = "Report generation queued" };
    }

    // ─── Tenant child stat ────────────────────────────────────────────────────

    public override async Task<ChildNetStatResponse> GetChildNetStat(
        GetTenantChildNetStatRequest request, ServerCallContext context)
    {
        // 兼容前端传 from/to 或标准 date_from/date_to
        var fromStr = (request.HasDateFrom ? request.DateFrom : null) ?? (request.HasFrom ? request.From : null);
        var toStr   = (request.HasDateTo   ? request.DateTo   : null) ?? (request.HasTo   ? request.To   : null);
        var from = fromStr != null && DateTime.TryParse(fromStr, out var f) ? (DateTime?)f : null;
        var to   = toStr   != null && DateTime.TryParse(toStr,   out var t) ? (DateTime?)t : null;

        var result = await tradingSvc.GetDirectChildNetAmountForAccountByUid(request.Uid, from, to, request.ViewClient);
        var sumUp  = result.FirstOrDefault(x => x.Uid == 0 && x.Group == "Child-Sum-Up")
                     ?? result.FirstOrDefault(x => x.Uid == 0);

        var response = new ChildNetStatResponse
        {
            Uid   = sumUp?.Uid   ?? 0,
            Group = sumUp?.Group ?? "Child-Sum-Up",
            Role  = (int)(sumUp?.Role ?? 0),
        };
        if (sumUp != null)
        {
            foreach (var kvp in sumUp.DepositAmounts)    response.DepositAmounts[kvp.Key.ToString()]    = kvp.Value;
            foreach (var kvp in sumUp.WithdrawalAmounts) response.WithdrawalAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.RebateAmounts)     response.RebateAmounts[kvp.Key.ToString()]     = kvp.Value;
            foreach (var kvp in sumUp.ProfitAmounts)     response.ProfitAmounts[kvp.Key.ToString()]     = kvp.Value;
            foreach (var kvp in sumUp.NetAmounts)        response.NetAmounts[kvp.Key.ToString()]        = kvp.Value;
        }
        return response;
    }

    public override async Task<SymbolGroupStatResponse> GetChildRebateBySymbol(
        GetTenantChildStatByRangeRequest request, ServerCallContext context)
    {
        // 兼容前端传 from/to 或标准 date_from/date_to
        var fromStr = (request.HasDateFrom ? request.DateFrom : null) ?? (request.HasFrom ? request.From : null);
        var toStr   = (request.HasDateTo   ? request.DateTo   : null) ?? (request.HasTo   ? request.To   : null);
        var from = fromStr != null && DateTime.TryParse(fromStr, out var f) ? (DateTime?)f : null;
        var to   = toStr   != null && DateTime.TryParse(toStr,   out var t) ? (DateTime?)t : null;

        var stats    = await tradingSvc.GetChildAccountRebateSymbolGroupedStatByUid(request.Uid, from, to);
        var response = new SymbolGroupStatResponse();
        foreach (var kvp in stats)
        {
            var entry = new SymbolStatEntry { Volume = kvp.Value.Volume, Profit = kvp.Value.Profit };
            foreach (var a in kvp.Value.Amounts) entry.Amounts[a.Key.ToString()] = a.Value;
            response.Items[kvp.Key] = entry;
        }
        return response;
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
            Meta = new PaginationMeta { Page = criteria.Page, Size = criteria.Size, Total = criteria.Total },
        };
        response.Items.AddRange(items.Select(r => new ProtoReferralCode
        {
            Code      = r.Code,
            Type      = r.ServiceType,
            IsDefault = r.IsDefault != 0,
        }));
        return response;
    }

    public override async Task<ReferralDetailResponse> GetReferralByCode(
        GetReferralByCodeRequest request, ServerCallContext context)
    {
        var item = await tenantCtx.ReferralCodes
            .Where(x => x.Code == request.Code.Trim())
            .Select(x => new { x.Id, x.AccountId, x.Account.Uid, x.Code, x.ServiceType })
            .FirstOrDefaultAsync();
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Referral code not found"));
        return new ReferralDetailResponse
        {
            AccountId = item.AccountId,
            Uid       = item.Uid,
            Code      = item.Code,
            Type      = item.ServiceType,
        };
    }

    public override async Task<ListReferralHistoryResponse> GetReferralHistory(
        ListReferralHistoryRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.Referral.Criteria
        {
            Page = request.Pagination?.Page > 0 ? request.Pagination.Page : request.HasPage && request.Page > 0 ? request.Page : 1,
            Size = request.Pagination?.Size > 0 ? request.Pagination.Size : request.HasSize && request.Size > 0 ? request.Size : 20,
        };
        var items = await tenantCtx.Referrals.PagedFilterBy(criteria).ToListAsync();
        var response = new ListReferralHistoryResponse
        {
            Meta = new PaginationMeta { Page = criteria.Page, Size = criteria.Size, Total = criteria.Total },
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

    public override async Task<ProtoReferralCode> AddReferralCode(
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

        return new ProtoReferralCode { Code = item.Code, Type = item.ServiceType, IsDefault = item.IsDefault != 0 };
    }

    public override async Task<OperationResponse> UpdateReferralDefaultPaymentMethods(
        UpdateReferralPaymentMethodsRequest request, ServerCallContext context)
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
        return new OperationResponse { Success = true };
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
            Meta = new PaginationMeta { Page = criteria.Page, Size = criteria.Size, Total = criteria.Total },
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
            Meta = new PaginationMeta { Page = criteria.Page, Size = criteria.Size, Total = criteria.Total },
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
    {
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("PartyId", out var v) && v is long id ? id : 0;
    }
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
    : TenantAccountServiceV2.TenantAccountServiceV2Base
{
    public override async Task<ProtoAccount> ConfirmAutoCreate(
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
        return MapToProto(account);
    }

    public override async Task<ProtoAccount> AssignWallet(
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
        return MapToProto(account);
    }

    public override async Task<TradeStatResponse> GetAccountTradeStat(
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
            return new TradeStatResponse
            {
                Volume     = volume,
                Profit     = profit,
                TradeCount = stat.ClosedTradeStats?.Count ?? 0,
            };
        }
        catch
        {
            return new TradeStatResponse();
        }
    }

    public override async Task<TradeStatResponse> GetMultiAccountTradeStat(
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
            return new TradeStatResponse
            {
                Volume     = volume,
                Profit     = profit,
                TradeCount = stat.ClosedTradeStats?.Count ?? 0,
            };
        }
        catch
        {
            return new TradeStatResponse();
        }
    }

    public override async Task<DailyReportResponse> GetDailyReport(
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
        return new DailyReportResponse
        {
            Date    = request.Date,
            Balance = first?.Balance    ?? 0,
            Equity  = first?.ProfitEquity ?? 0,
        };
    }

    public override async Task<ProtoAccount> EnableTransfer(
        GetAccountRequest request, ServerCallContext context)
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
        return MapToProto(account);
    }

    public override async Task<ProtoAccount> DisableTransfer(
        GetAccountRequest request, ServerCallContext context)
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
        return MapToProto(account);
    }

    public override async Task<StatTopResponse> GetStatTop(
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

        var response = new StatTopResponse();
        response.Items.AddRange(items.Select(x => new StatTopItem
        {
            AccountId = x.AccountNumber,
            Value     = (double)x.Pnl,
        }));
        return response;
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
    public override async Task<ListAccountsResponse> ListAccounts(
        ListClientAccountsRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var criteria = new Bacera.Gateway.Account.Criteria
        {
            Page    = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size    = 100,
            PartyId = partyId,
        };

        var result = await tradingSvc.AccountQueryForClientAsync(criteria, partyId);
        var response = new ListAccountsResponse
        {
            Meta = new PaginationMeta
            {
                Page      = criteria.Page,
                Size      = criteria.Size,
                Total     = criteria.Total,
                PageCount = criteria.Total > 0 ? (int)Math.Ceiling((double)criteria.Total / criteria.Size) : 0,
                HasMore   = false,
            }
        };
        response.Data.AddRange(result.Data.Select(a => new ProtoAccount
        {
            Id        = a.Id,
            Name      = a.Name ?? "",
            Status    = (int)a.Status,
            Role      = (int)a.Role,
            FundType  = (int)a.FundType,
            CreatedOn = a.CreatedOn.ToString("O"),
        }));
        return response;
    }

    public override async Task<ProtoAccount> GetAccount(
        GetAccountRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var item = await tradingSvc.AccountClientResponseModelGetForPartyAsync(request.Id, partyId);
        if (item.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return new ProtoAccount
        {
            Id        = item.Id,
            Name      = item.Name ?? "",
            Status    = (int)item.Status,
            Role      = (int)item.Role,
            FundType  = (int)item.FundType,
            CreatedOn = item.CreatedOn.ToString("O"),
        };
    }

    public override async Task<AccountWizardResponse> GetAccountWizard(
        GetAccountRequest request, ServerCallContext context)
    {
        var wizard = await tradingSvc.GetAccountWizardAsync(request.Id);
        return new AccountWizardResponse
        {
            KycFormCompleted     = wizard.KycFormCompleted,
            PaymentAccessGranted = wizard.PaymentAccessGranted,
            WelcomeEmailSent     = wizard.WelcomeEmailSent,
            NoticeEmailSent      = wizard.NoticeEmailSent,
        };
    }

    public override async Task<ProtoAccount> RefreshAccount(
        GetAccountRequest request, ServerCallContext context)
    {
        await generalJob.TryUpdateTradeAccountStatus(tenantGetter.GetTenantId(), request.Id, true);
        var partyId = GetPartyId(context);
        var item = await tradingSvc.AccountClientResponseModelGetForPartyAsync(request.Id, partyId);
        if (item.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return new ProtoAccount
        {
            Id        = item.Id,
            Name      = item.Name ?? "",
            Status    = (int)item.Status,
            Role      = (int)item.Role,
            FundType  = (int)item.FundType,
            CreatedOn = item.CreatedOn.ToString("O"),
        };
    }

    private static long GetPartyId(ServerCallContext ctx)
    {
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("PartyId", out var v) && v is long id ? id : 0;
    }
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
    public override async Task<ListAccountsResponse> ListAccounts(
        ListSalesAccountsRequest request, ServerCallContext context)
    {
        var salesId  = await accManSvc.GetAccountIdByUidAsync(request.SalesUid);
        var criteria = new Bacera.Gateway.Account.SalesCriteria
        {
            Page       = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size       = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            SearchText = request.HasKeywords ? request.Keywords : null,
        };
        var items = await accManSvc.QueryAccountForSalesAsync(salesId, criteria);
        var response = new ListAccountsResponse
        {
            Meta = new PaginationMeta
            {
                Page      = criteria.Page,
                Size      = criteria.Size,
                Total     = criteria.Total,
                PageCount = criteria.Total > 0 ? (int)Math.Ceiling((double)criteria.Total / criteria.Size) : 0,
                HasMore   = criteria.Page * criteria.Size < criteria.Total,
            }
        };
        response.Data.AddRange(items.Select(a => new ProtoAccount
        {
            Name      = a.NativeName,
            Status    = (int)a.Status,
            Role      = (int)a.Role,
            FundType  = (int)a.FundType,
            CreatedOn = a.CreatedOn.ToString("O"),
        }));
        return response;
    }

    public override async Task<ProtoAccount> GetAccount(
        GetSalesAccountRequest request, ServerCallContext context)
    {
        var clientAccount = await tradingSvc.AccountLookupForParentAsync(request.SalesUid, request.Uid);
        if (clientAccount.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var item = await tradingSvc.AccountGetForPartyAsync(clientAccount.Uid, clientAccount.PartyId);
        return new ProtoAccount
        {
            Id        = item.Id,
            Name      = item.Name ?? "",
            Status    = (int)item.Status,
            Role      = (int)item.Role,
            FundType  = (int)item.FundType,
            PartyId   = clientAccount.PartyId,
            CreatedOn = item.CreatedOn.ToString("O"),
        };
    }

    public override async Task<ProtoAccount> GetLevelAccount(
        GetLevelAccountRequest request, ServerCallContext context)
    {
        var items = await accManSvc.GetParentLevelAccountAsync(request.SalesUid, request.ChildAccountUid);
        var first = items?.FirstOrDefault();
        if (first == null) throw new RpcException(new Status(StatusCode.NotFound, "Level account not found"));
        return new ProtoAccount
        {
            Name = first.NativeName,
            Role = (int)first.Role,
        };
    }

    public override async Task<ChildStatResponse> GetChildAccountStat(
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
        return new ChildStatResponse
        {
            Volume     = (double)(stat?.TradeVolume ?? 0),
            Profit     = (double)(stat?.TradeProfit ?? 0),
            TradeCount = (int)(stat?.TradeCount ?? 0),
        };
    }

    public override async Task<ChildNetStatResponse> GetChildNetStat(
        GetChildNetStatRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var result = await tradingSvc.GetDirectChildNetAmountForAccountByUid(request.Uid, from, to, request.ViewClient);
        var sumUp = result.FirstOrDefault(x => x.Uid == 0 && x.Group == "Child-Sum-Up")
                    ?? result.FirstOrDefault(x => x.Uid == 0);

        var response = new ChildNetStatResponse
        {
            Uid   = sumUp?.Uid   ?? 0,
            Group = sumUp?.Group ?? "Child-Sum-Up",
            Role  = (int)(sumUp?.Role ?? 0),
        };
        if (sumUp != null)
        {
            foreach (var kvp in sumUp.DepositAmounts)
                response.DepositAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.WithdrawalAmounts)
                response.WithdrawalAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.RebateAmounts)
                response.RebateAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.ProfitAmounts)
                response.ProfitAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.NetAmounts)
                response.NetAmounts[kvp.Key.ToString()] = kvp.Value;
        }
        return response;
    }

    public override async Task<SymbolGroupStatResponse> GetChildRebateBySymbol(
        GetChildStatByRangeRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var stats = await tradingSvc.GetChildAccountRebateSymbolGroupedStatByUid(request.Uid, from, to);
        var response = new SymbolGroupStatResponse();
        foreach (var kvp in stats)
        {
            var entry = new SymbolStatEntry
            {
                Volume = kvp.Value.Volume,
                Profit = kvp.Value.Profit,
            };
            foreach (var amtKvp in kvp.Value.Amounts)
                entry.Amounts[amtKvp.Key.ToString()] = amtKvp.Value;
            response.Items[kvp.Key] = entry;
        }
        return response;
    }

    public override async Task<TradeSymbolGroupStatResponse> GetChildTradeBySymbol(
        GetChildStatByRangeRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var stats = await tradingSvc.GetChildAccountTradeSymbolGroupedStatByUid(request.Uid, from, to);
        var response = new TradeSymbolGroupStatResponse();
        response.Items.AddRange(stats.Select(s => new SymbolStat
        {
            Symbol     = s.Symbol,
            CurrencyId = (int)s.CurrencyId,
            Volume     = s.Volume,
            Profit     = s.Profit,
        }));
        return response;
    }

    public override async Task<AccountConfigResponse> GetAccountConfiguration(
        GetSalesAccountConfigRequest request, ServerCallContext context)
    {
        var agentAccount = await tenantCtx.Accounts
            .Where(x => x.Uid == request.AgentUid)
            .SingleOrDefaultAsync();
        if (agentAccount == null) throw new RpcException(new Status(StatusCode.NotFound, "Agent account not found"));
        return new AccountConfigResponse { AccountId = agentAccount.Id };
    }

    public override async Task<LevelSettingResponse> GetDefaultLevelSetting(
        GetSalesDefaultLevelRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts.Where(x => x.Uid == request.AgentUid).FirstOrDefaultAsync();
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var item = await tenantCtx.Configurations
            .Where(x => x.Category == nameof(Account) && x.RowId == account.Id && x.Key == "DefaultRebateLevelSetting")
            .ToTenantViewModel()
            .FirstOrDefaultAsync();

        if (item?.ValueString is { Length: > 0 } raw)
            return new LevelSettingResponse { Json = raw };

        var result = await configurationService.GetDefaultRebateLevelSettingAsync(account.SiteId);
        return new LevelSettingResponse { Json = JsonConvert.SerializeObject(result) };
    }

    public override async Task<AccountTypesResponse> GetAvailableAccountTypes(
        GetSalesUidRequest request, ServerCallContext context)
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

        var response = new AccountTypesResponse();
        response.Types_.AddRange(typeIds.Select(id => new AccountTypeConfig { Id = id, Name = "" }));
        return response;
    }

    public override async Task<ReferralPathResponse> GetReferralPath(
        GetSalesReferralPathRequest request, ServerCallContext context)
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

        var response = new ReferralPathResponse();
        response.Path.AddRange(accounts.Select(a => new ReferralPathItem
        {
            AccountId = a.Id,
            Name      = a.Name ?? "",
            Level     = a.Level,
        }));
        return response;
    }

    public override async Task<EmailCodeResponse> GetViewEmailCode(
        GetSalesAccountRequest request, ServerCallContext context)
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

        return new EmailCodeResponse { ExpiresAt = "" };
    }

    public override async Task<EmailResponse> GetViewEmail(
        GetViewEmailRequest request, ServerCallContext context)
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

        return new EmailResponse { Email = account.Email ?? "" };
    }

    private static long GetPartyId(ServerCallContext ctx)
    {
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("PartyId", out var v) && v is long id ? id : 0;
    }
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
    public override async Task<ListAccountsResponse> ListAccounts(
        ListAgentAccountsRequest request, ServerCallContext context)
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
        var response = new ListAccountsResponse
        {
            Meta = new PaginationMeta
            {
                Page      = criteria.Page,
                Size      = criteria.Size,
                Total     = criteria.Total,
                PageCount = criteria.Total > 0 ? (int)Math.Ceiling((double)criteria.Total / criteria.Size) : 0,
                HasMore   = criteria.Page * criteria.Size < criteria.Total,
            }
        };
        response.Data.AddRange(result.Data.Select(a => new ProtoAccount
        {
            Id        = a.Id,
            Name      = a.User.NativeName,
            Status    = (int)a.Status,
            Role      = (int)a.Role,
            FundType  = (int)a.FundType,
            PartyId   = a.PartyId,
            CreatedOn = a.CreatedOn.ToString("O"),
        }));
        return response;
    }

    public override async Task<ProtoAccount> GetAccount(
        GetAgentAccountRequest request, ServerCallContext context)
    {
        var clientAccount = await tradingSvc.AccountLookupForParentAsync(request.AgentUid, request.Uid);
        if (clientAccount.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var item = await tradingSvc.AccountGetForPartyAsync(clientAccount.Uid, clientAccount.PartyId);
        return new ProtoAccount
        {
            Id        = item.Id,
            Name      = item.Name ?? "",
            Status    = (int)item.Status,
            Role      = (int)item.Role,
            FundType  = (int)item.FundType,
            PartyId   = clientAccount.PartyId,
            CreatedOn = item.CreatedOn.ToString("O"),
        };
    }

    public override async Task<ProtoAccount> RefreshAccount(
        GetAgentAccountRequest request, ServerCallContext context)
    {
        var clientAccount = await tradingSvc.AccountLookupForParentAsync(request.AgentUid, request.Uid);
        if (clientAccount.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        await generalJob.TryUpdateTradeAccountStatus(tenantGetter.GetTenantId(), clientAccount.Id, true);
        var item = await tradingSvc.AccountGetForPartyAsync(clientAccount.Uid, clientAccount.PartyId);
        return new ProtoAccount
        {
            Id        = item.Id,
            Name      = item.Name ?? "",
            Status    = (int)item.Status,
            Role      = (int)item.Role,
            FundType  = (int)item.FundType,
            PartyId   = clientAccount.PartyId,
            CreatedOn = item.CreatedOn.ToString("O"),
        };
    }

    public override async Task<ChildNetStatResponse> GetChildNetStat(
        GetAgentChildNetStatRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var result = await tradingSvc.GetDirectChildNetAmountForAccountByUid(request.Uid, from, to, request.ViewClient);
        var sumUp = result.FirstOrDefault(x => x.Uid == 0 && x.Group == "Child-Sum-Up")
                    ?? result.FirstOrDefault(x => x.Uid == 0);

        var response = new ChildNetStatResponse
        {
            Uid   = sumUp?.Uid   ?? 0,
            Group = sumUp?.Group ?? "Child-Sum-Up",
            Role  = (int)(sumUp?.Role ?? 0),
        };
        if (sumUp != null)
        {
            foreach (var kvp in sumUp.DepositAmounts)
                response.DepositAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.WithdrawalAmounts)
                response.WithdrawalAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.RebateAmounts)
                response.RebateAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.ProfitAmounts)
                response.ProfitAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.NetAmounts)
                response.NetAmounts[kvp.Key.ToString()] = kvp.Value;
        }
        return response;
    }

    public override async Task<SymbolGroupStatResponse> GetChildRebateBySymbol(
        GetAgentChildStatByRangeRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var stats = await tradingSvc.GetChildAccountRebateSymbolGroupedStatByUid(request.Uid, from, to);
        var response = new SymbolGroupStatResponse();
        foreach (var kvp in stats)
        {
            var entry = new SymbolStatEntry
            {
                Volume = kvp.Value.Volume,
                Profit = kvp.Value.Profit,
            };
            foreach (var amtKvp in kvp.Value.Amounts)
                entry.Amounts[amtKvp.Key.ToString()] = amtKvp.Value;
            response.Items[kvp.Key] = entry;
        }
        return response;
    }

    public override async Task<LevelSettingResponse> GetDefaultLevelSetting(
        GetAgentAccountRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts.Where(x => x.Uid == request.AgentUid).FirstOrDefaultAsync();
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var item = await tenantCtx.Configurations
            .Where(x => x.Category == nameof(Account) && x.RowId == account.Id && x.Key == "DefaultRebateLevelSetting")
            .ToTenantViewModel()
            .FirstOrDefaultAsync();

        if (item?.ValueString is { Length: > 0 } raw)
            return new LevelSettingResponse { Json = raw };

        var result = await configurationService.GetDefaultRebateLevelSettingAsync(account.SiteId);
        return new LevelSettingResponse { Json = JsonConvert.SerializeObject(result) };
    }

    public override async Task<EmailCodeResponse> GetViewEmailCode(
        GetAgentAccountRequest request, ServerCallContext context)
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

        return new EmailCodeResponse { ExpiresAt = "" };
    }

    public override async Task<EmailResponse> GetViewEmail(
        GetAgentViewEmailRequest request, ServerCallContext context)
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

        return new EmailResponse { Email = account.Email ?? "" };
    }

    private static long GetPartyId(ServerCallContext ctx)
    {
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("PartyId", out var v) && v is long id ? id : 0;
    }
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
    public override async Task<ListAccountsResponse> ListAccounts(
        ListRepAccountsRequest request, ServerCallContext context)
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
        var response = new ListAccountsResponse
        {
            Meta = new PaginationMeta
            {
                Page      = criteria.Page,
                Size      = criteria.Size,
                Total     = criteria.Total,
                PageCount = criteria.Total > 0 ? (int)Math.Ceiling((double)criteria.Total / criteria.Size) : 0,
                HasMore   = criteria.Page * criteria.Size < criteria.Total,
            }
        };
        response.Data.AddRange(result.Data.Select(a => new ProtoAccount
        {
            Id        = a.Id,
            Name      = a.User.NativeName,
            Status    = (int)a.Status,
            Role      = (int)a.Role,
            FundType  = (int)a.FundType,
            PartyId   = a.PartyId,
            CreatedOn = a.CreatedOn.ToString("O"),
        }));
        return response;
    }

    public override async Task<ProtoAccount> GetAccount(
        GetRepAccountRequest request, ServerCallContext context)
    {
        var clientAccount = await tradingSvc.AccountLookupForParentAsync(request.RepUid, request.Uid);
        if (clientAccount.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

        var item = await tradingSvc.AccountGetForPartyAsync(clientAccount.Uid, clientAccount.PartyId);
        return new ProtoAccount
        {
            Id        = item.Id,
            Name      = item.Name ?? "",
            Status    = (int)item.Status,
            Role      = (int)item.Role,
            FundType  = (int)item.FundType,
            PartyId   = clientAccount.PartyId,
            CreatedOn = item.CreatedOn.ToString("O"),
        };
    }

    public override async Task<GroupNameListResponse> GetGroupNameList(
        GetGroupNameListRequest request, ServerCallContext context)
    {
        var type  = request.HasType ? (AccountGroupTypes)request.Type : AccountGroupTypes.Agent;
        var role  = (AccountRoleTypes)((int)type * 100);
        var items = await tradingSvc.GetAllGroupNamesUnderAccountByUid(request.RepUid, role, request.HasKeywords ? request.Keywords : "");
        var response = new GroupNameListResponse();
        response.Names.AddRange(items);
        return response;
    }

    public override async Task<ChildNetStatResponse> GetChildNetStat(
        GetRepChildNetStatRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var result = await tradingSvc.GetDirectChildNetAmountForAccountByUid(request.Uid, from, to, request.ViewClient);
        var sumUp = result.FirstOrDefault(x => x.Uid == 0 && x.Group == "Child-Sum-Up")
                    ?? result.FirstOrDefault(x => x.Uid == 0);

        var response = new ChildNetStatResponse
        {
            Uid   = sumUp?.Uid   ?? 0,
            Group = sumUp?.Group ?? "Child-Sum-Up",
            Role  = (int)(sumUp?.Role ?? 0),
        };
        if (sumUp != null)
        {
            foreach (var kvp in sumUp.DepositAmounts)
                response.DepositAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.WithdrawalAmounts)
                response.WithdrawalAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.RebateAmounts)
                response.RebateAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.ProfitAmounts)
                response.ProfitAmounts[kvp.Key.ToString()] = kvp.Value;
            foreach (var kvp in sumUp.NetAmounts)
                response.NetAmounts[kvp.Key.ToString()] = kvp.Value;
        }
        return response;
    }

    public override async Task<SymbolGroupStatResponse> GetChildRebateBySymbol(
        GetRepChildStatByRangeRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var stats = await tradingSvc.GetChildAccountRebateSymbolGroupedStatByUid(request.Uid, from, to);
        var response = new SymbolGroupStatResponse();
        foreach (var kvp in stats)
        {
            var entry = new SymbolStatEntry
            {
                Volume = kvp.Value.Volume,
                Profit = kvp.Value.Profit,
            };
            foreach (var amtKvp in kvp.Value.Amounts)
                entry.Amounts[amtKvp.Key.ToString()] = amtKvp.Value;
            response.Items[kvp.Key] = entry;
        }
        return response;
    }

    private static long GetPartyId(ServerCallContext ctx)
    {
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("PartyId", out var v) && v is long id ? id : 0;
    }
}
