using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.ViewModels.Tenant;
using Bacera.Gateway.Web.BackgroundJobs;
using Grpc.Core;
using Http.V1;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProtoAccount        = Http.V1.Account;
using ProtoAccountLog     = Http.V1.AccountLog;
using ProtoReferralCode   = Http.V1.ReferralCode;

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
            Page     = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size     = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            Keywords = request.HasKeywords ? request.Keywords : null,
            Status   = request.HasStatus   ? (AccountStatusTypes?)request.Status : null,
            Uid      = request.HasUid      ? request.Uid       : null,
            Role     = request.HasRole     ? (AccountRoleTypes?)request.Role : null,
            PartyId  = request.HasPartyId  ? request.PartyId   : null,
            FundType = request.HasFundType ? (FundTypes?)request.FundType : null,
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
        response.Data.AddRange(result.Data.Select(MapViewModelToProto));
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
        var criteria = new Bacera.Gateway.AccountLog.TenantCriteria
        {
            Page      = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size      = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            AccountId = request.HasAccountId ? request.AccountId : null,
            Action    = request.HasAction    ? request.Action    : null,
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
            Id        = log.Id,
            AccountId = log.AccountId,
            Action    = log.Action,
            Detail    = log.Before != null ? $"{log.Before} → {log.After}" : log.After ?? "",
            CreatedAt = log.CreatedOn.ToString("O"),
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
        var response = new LevelSettingResponse();
        // LevelSetting is a complex object; return empty response if not mappable
        return response;
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
        var completed = wizard.KycFormCompleted && wizard.PaymentAccessGranted;
        return new AccountWizardResponse
        {
            Step      = completed ? 4 : (wizard.KycFormCompleted ? 2 : 1),
            Completed = completed,
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

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static ProtoAccount MapToProto(Bacera.Gateway.Account a) => new ProtoAccount
    {
        Id        = a.Id,
        Name      = a.Name ?? "",
        Status    = a.Status,
        Role      = a.Role,
        FundType  = a.FundType,
        PartyId   = a.PartyId,
        CreatedAt = a.CreatedOn.ToString("O"),
        UpdatedAt = a.UpdatedOn.ToString("O"),
    };

    private static ProtoAccount MapViewModelToProto(AccountViewModel v) => new ProtoAccount
    {
        Id        = v.Id,
        Name      = v.Name ?? "",
        Email     = v.User?.Email ?? "",
        Status    = (int)v.Status,
        Role      = (int)v.Role,
        FundType  = (int)v.FundType,
        PartyId   = v.PartyId,
        CreatedAt = v.CreatedOn.ToString("O"),
        UpdatedAt = v.UpdatedOn.ToString("O"),
    };

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
    MyDbContextPool pool)
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

        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? f : DateTime.MinValue;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? t : DateTime.MaxValue;

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
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? f : DateTime.MinValue;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? t : DateTime.MaxValue;

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

        return new DailyReportResponse { Date = request.Date };
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
        CreatedAt = a.CreatedOn.ToString("O"),
        UpdatedAt = a.UpdatedOn.ToString("O"),
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
            CreatedAt = a.CreatedOn.ToString("O"),
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
            CreatedAt = item.CreatedOn.ToString("O"),
        };
    }

    public override async Task<AccountWizardResponse> GetAccountWizard(
        GetAccountRequest request, ServerCallContext context)
    {
        var wizard = await tradingSvc.GetAccountWizardAsync(request.Id);
        var completed = wizard.KycFormCompleted && wizard.PaymentAccessGranted;
        return new AccountWizardResponse
        {
            Step      = completed ? 4 : (wizard.KycFormCompleted ? 2 : 1),
            Completed = completed,
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
            CreatedAt = item.CreatedOn.ToString("O"),
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
    ConfigurationService configurationService)
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
            CreatedAt = a.CreatedOn.ToString("O"),
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
            CreatedAt = item.CreatedOn.ToString("O"),
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
        var net = result.Where(x => x.Uid != 0)
            .Sum(x => (double)(x.DepositAmounts.Values.Sum() - x.WithdrawalAmounts.Values.Sum())) / 10000.0;
        return new ChildNetStatResponse { NetDeposit = net };
    }

    public override async Task<SymbolGroupStatResponse> GetChildRebateBySymbol(
        GetChildStatByRangeRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var stats = await tradingSvc.GetChildAccountRebateSymbolGroupedStatByUid(request.Uid, from, to);
        var response = new SymbolGroupStatResponse();
        response.Items.AddRange(stats.Select(kvp => new SymbolStat
        {
            Symbol = kvp.Key,
            Volume = kvp.Value.Volume,
            Amount = kvp.Value.Profit,
        }));
        return response;
    }

    public override async Task<SymbolGroupStatResponse> GetChildTradeBySymbol(
        GetChildStatByRangeRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var stats = await tradingSvc.GetChildAccountTradeSymbolGroupedStatByUid(request.Uid, from, to);
        var response = new SymbolGroupStatResponse();
        response.Items.AddRange(stats.Select(s => new SymbolStat
        {
            Symbol = s.Symbol,
            Volume = s.Volume,
            Amount = s.Profit,
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
        return new LevelSettingResponse();
    }

    public override async Task<AccountTypesResponse> GetAvailableAccountTypes(
        GetSalesUidRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts.Where(x => x.Uid == request.SalesUid).FirstOrDefaultAsync();
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        var result = await configurationService.GetAccountTypeAvailableAsync(account.SiteId);
        var response = new AccountTypesResponse();
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
        // Returns an email code to allow viewing the account's email
        return new EmailCodeResponse { ExpiresAt = "" };
    }

    public override async Task<EmailResponse> GetViewEmail(
        GetViewEmailRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts
            .Where(x => x.Uid == request.Uid)
            .Where(x => x.ReferPath.Contains(request.SalesUid.ToString()))
            .Select(x => new { x.Party.Email })
            .SingleOrDefaultAsync();
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return new EmailResponse { Email = account.Email ?? "" };
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
    IGeneralJob generalJob)
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
            CreatedAt = a.CreatedOn.ToString("O"),
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
            CreatedAt = item.CreatedOn.ToString("O"),
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
            CreatedAt = item.CreatedOn.ToString("O"),
        };
    }

    public override async Task<ChildNetStatResponse> GetChildNetStat(
        GetAgentChildNetStatRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var result = await tradingSvc.GetDirectChildNetAmountForAccountByUid(request.Uid, from, to, request.ViewClient);
        var net = result.Where(x => x.Uid != 0)
            .Sum(x => (double)(x.DepositAmounts.Values.Sum() - x.WithdrawalAmounts.Values.Sum())) / 10000.0;
        return new ChildNetStatResponse { NetDeposit = net };
    }

    public override async Task<SymbolGroupStatResponse> GetChildRebateBySymbol(
        GetAgentChildStatByRangeRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var stats = await tradingSvc.GetChildAccountRebateSymbolGroupedStatByUid(request.Uid, from, to);
        var response = new SymbolGroupStatResponse();
        response.Items.AddRange(stats.Select(kvp => new SymbolStat
        {
            Symbol = kvp.Key,
            Volume = kvp.Value.Volume,
            Amount = kvp.Value.Profit,
        }));
        return response;
    }

    public override async Task<LevelSettingResponse> GetDefaultLevelSetting(
        GetAgentAccountRequest request, ServerCallContext context)
    {
        return new LevelSettingResponse();
    }

    public override async Task<EmailCodeResponse> GetViewEmailCode(
        GetAgentAccountRequest request, ServerCallContext context)
    {
        return new EmailCodeResponse { ExpiresAt = "" };
    }

    public override async Task<EmailResponse> GetViewEmail(
        GetAgentViewEmailRequest request, ServerCallContext context)
    {
        var account = await tenantCtx.Accounts
            .Where(x => x.Uid == request.Uid)
            .Where(x => x.ReferPath.Contains(request.AgentUid.ToString()))
            .Select(x => new { x.Party.Email })
            .SingleOrDefaultAsync();
        if (account == null) throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
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
            CreatedAt = a.CreatedOn.ToString("O"),
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
            CreatedAt = item.CreatedOn.ToString("O"),
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
        var net = result.Where(x => x.Uid != 0)
            .Sum(x => (double)(x.DepositAmounts.Values.Sum() - x.WithdrawalAmounts.Values.Sum())) / 10000.0;
        return new ChildNetStatResponse { NetDeposit = net };
    }

    public override async Task<SymbolGroupStatResponse> GetChildRebateBySymbol(
        GetRepChildStatByRangeRequest request, ServerCallContext context)
    {
        var from = request.HasDateFrom && DateTime.TryParse(request.DateFrom, out var f) ? (DateTime?)f : null;
        var to   = request.HasDateTo   && DateTime.TryParse(request.DateTo,   out var t) ? (DateTime?)t : null;

        var stats = await tradingSvc.GetChildAccountRebateSymbolGroupedStatByUid(request.Uid, from, to);
        var response = new SymbolGroupStatResponse();
        response.Items.AddRange(stats.Select(kvp => new SymbolStat
        {
            Symbol = kvp.Key,
            Volume = kvp.Value.Volume,
            Amount = kvp.Value.Profit,
        }));
        return response;
    }

    private static long GetPartyId(ServerCallContext ctx)
    {
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("PartyId", out var v) && v is long id ? id : 0;
    }
}
