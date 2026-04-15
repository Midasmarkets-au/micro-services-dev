using Bacera.Gateway.Auth;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Services.Permission;
using Bacera.Gateway.ViewModels.Tenant;
using Bacera.Gateway.Web.Areas.Tenant.Helpers;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Grpc.Core;
using Http.V1;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using ProtoAudit  = Http.V1.Audit;
using ProtoApiLog = Http.V1.ApiLog;

namespace Bacera.Gateway.Web.HttpServices.Admin;

// ─── Audit ────────────────────────────────────────────────────────────────────

/// <summary>
/// Replaces Areas/Tenant/Controllers/AuditController.cs
/// Routes defined in admin.proto via google.api.http annotations.
/// </summary>
public class TenantAuditGrpcService(AuthDbContext authDb, TenantDbContext tenantDb)
    : TenantAuditService.TenantAuditServiceBase
{
    public override async Task<ListAuditsResponse> ListAudits(
        ListAuditsRequest request, ServerCallContext context)
    {
        var criteria = new Audit.Criteria
        {
            Page = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
        };

        var items = await tenantDb.Audits
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        var response = new ListAuditsResponse
        {
            Criteria = BuildMeta(criteria.Page, criteria.Size, criteria.Total)
        };
        response.Data.AddRange(items.Select(a => new ProtoAudit
        {
            Id        = a.Id,
            AccountId = a.RowId,
            Action    = a.Type.ToString(),
            Detail    = a.Data,
            Ip        = "",
            CreatedAt = a.CreatedOn.ToString("O"),
        }));
        return response;
    }

    public override async Task<GetAuditResponse> GetAudit(GetAuditRequest request, ServerCallContext context)
    {
        var item = await tenantDb.Audits
            .Where(x => x.Id == request.Id)
            .ToTenantPageModel()
            .FirstOrDefaultAsync();
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Audit not found"));

        return new GetAuditResponse
        {
            Data = new ProtoAudit
            {
                Id        = item.Id,
                AccountId = item.RowId,
                Action    = item.Type.ToString(),
                Detail    = item.Data,
                Ip        = "",
                CreatedAt = item.CreatedOn.ToString("O"),
            },
        };
    }

    public override async Task<ListAccountChangeBalanceAuditsResponse> ListAccountChangeBalanceAudits(
        ListAccountChangeBalanceAuditsRequest request, ServerCallContext context)
    {
        var criteria = new Audit.Criteria
        {
            Page = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            Type = AuditTypes.TradeAccountBalance,
        };

        var items = await tenantDb.Audits
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        var response = new ListAccountChangeBalanceAuditsResponse
        {
            Criteria = BuildMeta(criteria.Page, criteria.Size, criteria.Total)
        };
        response.Data.AddRange(items.Select(a => new ProtoAudit
        {
            Id        = a.Id,
            AccountId = a.RowId,
            Action    = a.Type.ToString(),
            Detail    = a.Data,
            Ip        = "",
            CreatedAt = a.CreatedOn.ToString("O"),
        }));
        return response;
    }

    public override async Task<GetHighDollarLatestResponse> GetHighDollarLatest(
        GetHighDollarLatestRequest request, ServerCallContext context)
    {
        var key = Enum.GetName(typeof(ConfigurationTypes), ConfigurationTypes.HighDollarValue) ?? string.Empty;
        var rowId = await tenantDb.Configurations
            .Where(x => x.Category == nameof(Public) && x.Key == key && x.RowId == 0)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        var criteria = new Audit.Criteria { Type = AuditTypes.Configuration, RowId = rowId, Page = 1, Size = 1 };
        var items = await tenantDb.Audits
            .PagedFilterBy(criteria)
            .OrderByDescending(x => x.Id)
            .ToTenantPageModel()
            .ToListAsync();

        var response = new GetHighDollarLatestResponse { Criteria = BuildMeta(1, 1, items.Count) };
        response.Data.AddRange(items.Select(a => new ProtoAudit
        {
            Id        = a.Id,
            AccountId = a.RowId,
            Action    = a.Type.ToString(),
            Detail    = a.Data,
            Ip        = "",
            CreatedAt = a.CreatedOn.ToString("O"),
        }));
        return response;
    }

    private static long GetTenantId(ServerCallContext ctx)
    {
        // Tenant ID is injected by MultiTenantServiceMiddleware into HttpContext items
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("TenantId", out var v) && v is long id ? id : 0;
    }

    private static PaginationMeta BuildMeta(int page, int size, int total) => new()
    {
        Page      = page,
        Size      = size,
        Total     = total,
        PageCount = total > 0 ? (int)Math.Ceiling((double)total / size) : 0,
        HasMore   = page * size < total,
    };
}

// ─── IP Blacklist ─────────────────────────────────────────────────────────────

/// <summary>
/// Replaces Areas/Tenant/Controllers/IpBlackListController.cs
/// </summary>
public class TenantIpBlacklistGrpcService(
    CentralDbContext centralDb,
    AuthDbContext authDb,
    IMyCache myCache)
    : TenantIpBlacklistService.TenantIpBlacklistServiceBase
{
    private readonly string _ipKey = CacheKeys.GetBlackedIpHashKey();

    public override async Task<ListIpBlacklistResponse> ListIpBlacklist(
        ListIpBlacklistRequest request, ServerCallContext context)
    {
        var criteria = new IpBlackList.Criteria
        {
            Page = request.Pagination?.Page > 0 ? request.Pagination.Page : request.HasPage && request.Page > 0 ? request.Page : 1,
            Size = request.Pagination?.Size > 0 ? request.Pagination.Size : request.HasSize && request.Size > 0 ? request.Size : 20,
            Ip   = request.HasIp ? request.Ip : null,
        };

        var items = await centralDb.IpBlackLists
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        var response = new ListIpBlacklistResponse
        {
            Criteria = BuildMeta(criteria.Page, criteria.Size, criteria.Total)
        };
        response.Data.AddRange(items.Select(MapToProto));
        return response;
    }

    public override async Task<CreateIpBlacklistResponse> CreateIpBlacklist(
        CreateIpBlacklistRequest request, ServerCallContext context)
    {
        if (!System.Net.IPAddress.TryParse(request.Ip, out _))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "__INVALID_IP_ADDRESS__"));
        if (await centralDb.IpBlackLists.AnyAsync(x => x.Ip == request.Ip))
            throw new RpcException(new Status(StatusCode.AlreadyExists, "__IP_ALREADY_EXISTS__"));

        var spec = new IpBlackList.CreateSpec { Ip = request.Ip, Note = request.Note };
        var entity = spec.ToEntity(await GetOperatorNameAsync(context));
        centralDb.IpBlackLists.Add(entity);
        await centralDb.SaveChangesAsync();
        await myCache.HSetStringAsync(_ipKey, entity.Ip, "1");
        return new CreateIpBlacklistResponse
        {
            Data = MapToProto(new IpBlackList.TenantPageModel
            {
                Id = entity.Id, Ip = entity.Ip, Note = entity.Note,
                Enabled = entity.Enabled, CreatedOn = entity.CreatedOn
            }),
        };
    }

    public override async Task<UpdateIpBlacklistResponse> UpdateIpBlacklist(
        UpdateIpBlacklistRequest request, ServerCallContext context)
    {
        var entity = await centralDb.IpBlackLists.FindAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Not found"));
        await myCache.HSetDeleteByKeyFieldAsync(_ipKey, entity.Ip);
        var spec = new IpBlackList.UpdateSpec
        {
            Ip      = request.Spec?.Ip ?? entity.Ip,
            Note    = !string.IsNullOrEmpty(request.Spec?.Note) ? request.Spec.Note : entity.Note,
            Enabled = request.HasEnabled ? request.Enabled : entity.Enabled,
        };
        spec.ApplyTo(entity, await GetOperatorNameAsync(context));
        await centralDb.SaveChangesAsync();
        await myCache.HSetStringAsync(_ipKey, entity.Ip, "1");
        return new UpdateIpBlacklistResponse
        {
            Data = MapToProto(new IpBlackList.TenantPageModel
            {
                Id = entity.Id, Ip = entity.Ip, Note = entity.Note,
                Enabled = entity.Enabled, CreatedOn = entity.CreatedOn
            }),
        };
    }

    public override async Task<DeleteIpBlacklistResponse> DeleteIpBlacklist(
        DeleteIpBlacklistRequest request, ServerCallContext context)
    {
        var entity = await centralDb.IpBlackLists.FindAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Not found"));
        centralDb.IpBlackLists.Remove(entity);
        await centralDb.SaveChangesAsync();
        await myCache.HSetDeleteByKeyFieldAsync(_ipKey, entity.Ip);
        return new DeleteIpBlacklistResponse { Success = true };
    }

    public override async Task<ReloadCacheResponse> ReloadCache(ReloadCacheRequest request, ServerCallContext context)
    {
        await myCache.HSetDeleteByKeyAsync(_ipKey);
        var ips = await centralDb.IpBlackLists.Select(x => x.Ip).ToListAsync();
        foreach (var ip in ips) await myCache.HSetStringAsync(_ipKey, ip, "1");
        return new ReloadCacheResponse { Success = true, Message = $"Reloaded {ips.Count} entries" };
    }

    public override async Task<UploadIpBlacklistResponse> UploadIpBlacklist(
        UploadIpBlacklistRequest request, ServerCallContext context)
    {
        var httpCtx = context.GetHttpContext();
        var file = httpCtx.Request.Form.Files.FirstOrDefault()
            ?? throw new RpcException(new Status(StatusCode.InvalidArgument, "__FILE_REQUIRED__"));

        const long maxBytes = 25 * 1024 * 1024;
        if (file.Length == 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "__FILE_REQUIRED__"));
        if (file.Length > maxBytes)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "__FILE_TOO_LARGE__"));
        if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "__INVALID_FILE_EXTENSION__"));

        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        using var workbook = new ClosedXML.Excel.XLWorkbook(ms);
        var sheetName  = request.HasSheetName  ? request.SheetName  : null;
        var sheetIndex = request.HasSheetIndex ? request.SheetIndex : 0;

        if (!BlacklistExcelImportHelper.TryGetWorksheet(workbook, sheetName, sheetIndex, out var ws, out var sheetErr))
            throw new RpcException(new Status(StatusCode.InvalidArgument, sheetErr));

        var headerRow = BlacklistExcelImportHelper.FindHeaderRow(ws);
        if (headerRow == 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "__EMPTY_SHEET__"));

        var map = BlacklistExcelImportHelper.ResolveColumns(ws, headerRow);
        if (map.IpCol == null || map.ReasonCol == null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "__MISSING_COLUMNS__"));

        var tracked = await centralDb.IpBlackLists.ToListAsync();
        var byCanonical = new Dictionary<string, IpBlackList>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in tracked)
        {
            var key = BlacklistExcelImportHelper.TryCanonicalIp(e.Ip, out var c) ? c : e.Ip.Trim();
            if (!byCanonical.ContainsKey(key)) byCanonical[key] = e;
        }

        var result = new BlacklistUploadResult();
        var op = await GetOperatorNameAsync(context);
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;
        var pendingSinceLastSave = 0;
        var ipColLetter = ws.Column(map.IpCol!.Value).ColumnLetter();
        string Cell(int row, int col) => BlacklistExcelImportHelper.CellText(ws.Cell(row, col));

        for (var r = headerRow + 1; r <= lastRow; r++)
        {
            var reasonRaw = Cell(r, map.ReasonCol!.Value);
            if (string.IsNullOrWhiteSpace(reasonRaw)) { result.SkippedEmptyOrInvalid++; continue; }

            var ipCell = Cell(r, map.IpCol!.Value);
            if (string.IsNullOrWhiteSpace(ipCell)) continue;

            var ips = BlacklistExcelImportHelper.ExtractIps(ipCell);
            if (ips.Count == 0)
            {
                result.SkippedParseError++;
                BlacklistExcelImportHelper.AddError(result,
                    $"Row {r} col {ipColLetter}: no parseable IP; value={BlacklistExcelImportHelper.FormatCellForError(ipCell)}");
                continue;
            }

            var note = BlacklistExcelImportHelper.TruncateNote(reasonRaw.Trim());
            foreach (var ip in ips)
            {
                if (ip.Length > 64 || !System.Net.IPAddress.TryParse(ip, out _))
                {
                    result.SkippedParseError++;
                    BlacklistExcelImportHelper.AddError(result,
                        $"Row {r} col {ipColLetter}: rejected '{BlacklistExcelImportHelper.FormatCellForError(ip, 120)}'; cell={BlacklistExcelImportHelper.FormatCellForError(ipCell, 160)}");
                    continue;
                }
                if (byCanonical.TryGetValue(ip, out var existing))
                {
                    if (existing.Enabled) { result.SkippedDuplicate++; continue; }
                    existing.Enabled = true; existing.Note = note;
                    existing.OperatorName = op; existing.UpdatedOn = DateTime.UtcNow;
                    result.Updated++;
                }
                else
                {
                    var entity = new IpBlackList
                    {
                        Ip = ip, Note = note, Enabled = true,
                        OperatorName = op, CreatedOn = DateTime.UtcNow, UpdatedOn = DateTime.UtcNow,
                    };
                    centralDb.IpBlackLists.Add(entity);
                    byCanonical[ip] = entity;
                    result.Inserted++;
                }
                if (++pendingSinceLastSave >= 200) { await centralDb.SaveChangesAsync(); pendingSinceLastSave = 0; }
            }
        }
        if (pendingSinceLastSave > 0) await centralDb.SaveChangesAsync();
        await myCache.HSetDeleteByKeyAsync(_ipKey);
        var allIps = await centralDb.IpBlackLists.Select(x => x.Ip).ToListAsync();
        foreach (var ip in allIps) await myCache.HSetStringAsync(_ipKey, ip, "1");

        var resp = new UploadIpBlacklistResponse
        {
            Inserted = result.Inserted, Updated = result.Updated,
            SkippedDuplicate = result.SkippedDuplicate,
            SkippedEmptyOrInvalid = result.SkippedEmptyOrInvalid,
            SkippedParseError = result.SkippedParseError,
        };
        resp.Errors.AddRange(result.Errors);
        return resp;
    }

    private static IpBlacklistItem MapToProto(IpBlackList.TenantPageModel m) => new()
    {
        Id           = m.Id,
        Ip           = m.Ip,
        Note         = m.Note,
        Enabled      = m.Enabled,
        CreatedOn    = m.CreatedOn.ToString("O"),
        UpdatedOn    = m.UpdatedOn.ToString("O"),
        OperatorName = m.OperatorName ?? "",
    };

    private async Task<string> GetOperatorNameAsync(ServerCallContext ctx)
    {
        var user = ctx.GetHttpContext().User;
        var tenantId = user.GetTenantId();
        var partyId  = user.GetPartyId();
        var u = await authDb.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == partyId)
            .ToUserNameModel()
            .SingleAsync();
        return u.GuessNativeName();
    }

    private static PaginationMeta BuildMeta(int page, int size, int total) => new()
    {
        Page      = page,
        Size      = size,
        Total     = total,
        PageCount = total > 0 ? (int)Math.Ceiling((double)total / size) : 0,
        HasMore   = page * size < total,
    };
}

// ─── User Blacklist ───────────────────────────────────────────────────────────

/// <summary>
/// Replaces Areas/Tenant/Controllers/UserBlackListController.cs
/// </summary>
public class TenantUserBlacklistGrpcService(
    CentralDbContext centralDb,
    AuthDbContext authDb,
    IMyCache myCache)
    : TenantUserBlacklistService.TenantUserBlacklistServiceBase
{
    private readonly string _nameKey   = CacheKeys.GetBlackedUserNameHashKey();
    private readonly string _phoneKey  = CacheKeys.GetBlackedUserPhoneHashKey();
    private readonly string _emailKey  = CacheKeys.GetBlackedUserEmailHashKey();
    private readonly string _idNumKey  = CacheKeys.GetBlackedUserIdNumberHashKey();

    public override async Task<ListUserBlacklistResponse> ListUserBlacklist(
        ListUserBlacklistRequest request, ServerCallContext context)
    {
        var criteria = new UserBlackList.Criteria
        {
            Page  = request.Pagination?.Page > 0 ? request.Pagination.Page : request.HasPage && request.Page > 0 ? request.Page : 1,
            Size  = request.Pagination?.Size > 0 ? request.Pagination.Size : request.HasSize && request.Size > 0 ? request.Size : 20,
            Email = request.HasEmail ? request.Email : null,
        };

        var items = await centralDb.UserBlackLists
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        var response = new ListUserBlacklistResponse
        {
            Criteria = BuildMeta(criteria.Page, criteria.Size, criteria.Total)
        };
        response.Data.AddRange(items.Select(MapToProto));
        return response;
    }

    public override async Task<CreateUserBlacklistResponse> CreateUserBlacklist(
        CreateUserBlacklistRequest request, ServerCallContext context)
    {
        var exists = await centralDb.UserBlackLists.AnyAsync(x => x.Email == request.Email);
        if (exists) throw new RpcException(new Status(StatusCode.AlreadyExists, "__USER_ALREADY_EXISTS__"));

        var spec = new UserBlackList.CreateSpec { Email = request.Email };
        var entity = spec.ToEntity(await GetOperatorNameAsync(context));
        centralDb.UserBlackLists.Add(entity);
        await centralDb.SaveChangesAsync();
        await AddCacheAsync(entity);
        return new CreateUserBlacklistResponse
        {
            Data = MapToProto(new UserBlackList.TenantPageModel
            {
                Id = entity.Id, Email = entity.Email, CreatedOn = entity.CreatedOn
            }),
        };
    }

    public override async Task<UpdateUserBlacklistResponse> UpdateUserBlacklist(
        UpdateUserBlacklistRequest request, ServerCallContext context)
    {
        var entity = await centralDb.UserBlackLists.FindAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Not found"));
        await RemoveCacheAsync(entity);
        var spec = new UserBlackList.UpdateSpec
        {
            Email    = request.Spec?.Email    ?? entity.Email,
            Name     = entity.Name,
            Phone    = entity.Phone,
            IdNumber = entity.IdNumber,
        };
        spec.ApplyTo(entity, await GetOperatorNameAsync(context));
        await centralDb.SaveChangesAsync();
        await AddCacheAsync(entity);
        return new UpdateUserBlacklistResponse
        {
            Data = MapToProto(new UserBlackList.TenantPageModel
            {
                Id = entity.Id, Email = entity.Email, CreatedOn = entity.CreatedOn
            }),
        };
    }

    public override async Task<DeleteUserBlacklistResponse> DeleteUserBlacklist(
        DeleteUserBlacklistRequest request, ServerCallContext context)
    {
        var entity = await centralDb.UserBlackLists.FindAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Not found"));
        centralDb.UserBlackLists.Remove(entity);
        await centralDb.SaveChangesAsync();
        await RemoveCacheAsync(entity);
        return new DeleteUserBlacklistResponse { Success = true };
    }

    public override async Task<ReloadUserBlacklistCacheResponse> ReloadUserBlacklistCache(
        ReloadUserBlacklistCacheRequest request, ServerCallContext context)
    {
        await myCache.HSetDeleteByKeyAsync(_nameKey);
        await myCache.HSetDeleteByKeyAsync(_phoneKey);
        await myCache.HSetDeleteByKeyAsync(_emailKey);
        await myCache.HSetDeleteByKeyAsync(_idNumKey);

        var items = await centralDb.UserBlackLists
            .Select(x => new { x.Name, x.Phone, x.Email, x.IdNumber })
            .ToListAsync();

        foreach (var item in items)
        {
            await myCache.HSetStringAsync(_nameKey,  item.Name,     "1");
            await myCache.HSetStringAsync(_phoneKey, item.Phone,    "1");
            await myCache.HSetStringAsync(_emailKey, item.Email,    "1");
            await myCache.HSetStringAsync(_idNumKey, item.IdNumber, "1");
        }

        return new ReloadUserBlacklistCacheResponse { Success = true, Message = items.Count.ToString() };
    }

    private Task RemoveCacheAsync(UserBlackList e) => Task.WhenAll(
        myCache.HSetDeleteByKeyFieldAsync(_nameKey, e.Name),
        myCache.HSetDeleteByKeyFieldAsync(_phoneKey, e.Phone),
        myCache.HSetDeleteByKeyFieldAsync(_emailKey, e.Email),
        myCache.HSetDeleteByKeyFieldAsync(_idNumKey, e.IdNumber));

    private Task AddCacheAsync(UserBlackList e) => Task.WhenAll(
        myCache.HSetStringAsync(_nameKey, e.Name, "1"),
        myCache.HSetStringAsync(_phoneKey, e.Phone, "1"),
        myCache.HSetStringAsync(_emailKey, e.Email, "1"),
        myCache.HSetStringAsync(_idNumKey, e.IdNumber, "1"));

    private static UserBlacklistItem MapToProto(UserBlackList.TenantPageModel m) => new()
    {
        Id        = m.Id,
        Email     = m.Email,
        Reason    = m.OperatorName,
        Status    = 1,
        CreatedAt = m.CreatedOn.ToString("O"),
    };

    private async Task<string> GetOperatorNameAsync(ServerCallContext ctx)
    {
        var user = ctx.GetHttpContext().User;
        var tenantId = user.GetTenantId();
        var partyId  = user.GetPartyId();
        var u = await authDb.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == partyId)
            .ToUserNameModel()
            .SingleAsync();
        return u.GuessNativeName();
    }

    private static PaginationMeta BuildMeta(int page, int size, int total) => new()
    {
        Page      = page,
        Size      = size,
        Total     = total,
        PageCount = total > 0 ? (int)Math.Ceiling((double)total / size) : 0,
        HasMore   = page * size < total,
    };
}

// ─── API Log ──────────────────────────────────────────────────────────────────

/// <summary>
/// Replaces Areas/Tenant/Controllers/ApiLogController.cs
/// </summary>
public class TenantApiLogGrpcService(TenantDbContext tenantDb)
    : TenantApiLogService.TenantApiLogServiceBase
{
    public override async Task<ListApiLogsResponse> ListApiLogs(
        ListApiLogsRequest request, ServerCallContext context)
    {
        var criteria = new ApiLog.Criteria
        {
            Page       = request.Pagination?.Page > 0 ? request.Pagination.Page : request.HasPage && request.Page > 0 ? request.Page : 1,
            Size       = request.Pagination?.Size > 0 ? request.Pagination.Size : request.HasSize && request.Size > 0 ? request.Size : 20,
            Action     = request.HasPath       ? request.Path       : null,
            StatusCode = request.HasStatusCode ? (short)request.StatusCode : null,
        };

        var items = await tenantDb.ApiLogs
            .PagedFilterBy(criteria)
            .ToTenantPageModel(false)
            .ToListAsync();

        var partyIds  = items.Select(x => x.PartyId).Distinct().ToList();
        var partyEmails = await tenantDb.Parties
            .Where(x => partyIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Email);
        items.ForEach(x => x.Email = partyEmails.GetValueOrDefault(x.PartyId, string.Empty));

        var response = new ListApiLogsResponse
        {
            Criteria = BuildMeta(criteria.Page, criteria.Size, criteria.Total)
        };
        response.Data.AddRange(items.Select(l => new ProtoApiLog
        {
            Id         = l.Id ?? 0,
            Method     = l.Method ?? "",
            Path       = l.Action ?? "",
            StatusCode = l.StatusCode ?? 0,
            Duration   = l.DurationInSeconds ?? 0,
            CreatedAt  = l.CreatedOn?.ToString("O") ?? "",
        }));
        return response;
    }

    private static PaginationMeta BuildMeta(int page, int size, int total) => new()
    {
        Page      = page,
        Size      = size,
        Total     = total,
        PageCount = total > 0 ? (int)Math.Ceiling((double)total / size) : 0,
        HasMore   = page * size < total,
    };
}

// ─── Statistics V1 ────────────────────────────────────────────────────────────

/// <summary>
/// Online admin/user statistics.
/// Routes moved from api/v2 (StatisticControllerV2) to api/v1 as defined in admin.proto.
/// </summary>
public class TenantStatisticsGrpcService(MonitorService monitorSvc, IMyCache cache, MyDbContextPool pool)
    : TenantStatisticsService.TenantStatisticsServiceBase
{
    public override async Task<GetOnlineAdminsResponse> GetOnlineAdmins(
        GetOnlineAdminsRequest request, ServerCallContext context)
    {
        var rawInfos = await monitorSvc.GetOnlineAdminAsync();
        // Stored format: "email_partyId_tenantId_datetime"
        var users = rawInfos
            .Select(x =>
            {
                var raw      = x.ToString()!;
                var lastUs   = raw.LastIndexOf('_');
                var dateStr  = raw[(lastUs + 1)..];
                var rest     = raw[..lastUs];
                var secondUs = rest.LastIndexOf('_');
                var tenantId = rest[(secondUs + 1)..];   // extract tenantId
                rest         = rest[..secondUs];
                var thirdUs  = rest.LastIndexOf('_');
                var partyId  = rest[(thirdUs + 1)..];
                var email    = rest[..thirdUs];
                return new OnlineUser
                {
                    PartyId  = long.TryParse(partyId,  out var pid) ? pid : 0,
                    TenantId = long.TryParse(tenantId, out var tid) ? tid : 0,
                    Email    = email,
                    Since    = dateStr,
                };
            })
            .ToList();

        var inner = new OnlineAdminsResponse { Count = users.Count };
        inner.Users.AddRange(users);
        return new GetOnlineAdminsResponse { Data = inner };
    }

    public override async Task<GetOnlineUsersResponse> GetOnlineUsers(
        GetOnlineUsersRequest request, ServerCallContext context)
    {
        var db        = cache.GetDatabase();
        var tenantIds = pool.GetTenantIds();
        var inner     = new OnlineUsersResponse();

        foreach (var tenantId in tenantIds)
        {
            var deviceStat = new Dictionary<string, long>();
            var clientStat = new Dictionary<string, long>();
            var total      = 0;
            var pattern    = $"portal_online:{tenantId}:*";
            long cursor    = 0;

            do
            {
                var scan   = await db.ExecuteAsync("SCAN", cursor.ToString(), "MATCH", pattern, "COUNT", "100");
                var result = (RedisResult[])scan!;
                cursor = long.Parse((string)result[0]!);

                foreach (var key in (string[])result[1]!)
                {
                    total++;
                    var parts  = key.Split(':').TakeLast(3).ToArray();
                    var client = parts[1];
                    var device = parts[2];

                    deviceStat.TryAdd(device, 0);
                    clientStat.TryAdd(client, 0);
                    deviceStat[device]++;
                    clientStat[client]++;
                }
            } while (cursor != 0);

            var stat = new OnlineTenantStat { TenantId = tenantId, Total = total };
            foreach (var kv in deviceStat) stat.DeviceStat[kv.Key] = kv.Value;
            foreach (var kv in clientStat) stat.ClientStat[kv.Key] = kv.Value;
            inner.Items.Add(stat);
        }

        return new GetOnlineUsersResponse { Data = inner };
    }
}

// ─── Statistics V2 ────────────────────────────────────────────────────────────

/// <summary>
/// Server list and metrics (replaces StatisticControllerV2 server endpoints).
/// </summary>
public class TenantStatisticsV2GrpcService(MybcrDbContext bcrCtx, IHttpClientFactory httpFactory)
    : TenantStatisticsV2Service.TenantStatisticsV2ServiceBase
{
    public override async Task<GetServersResponse> GetServers(
        GetServersRequest request, ServerCallContext context)
    {
        var items = await bcrCtx.Servers
            .PagedFilterBy<Server, ulong>(new Server.Criteria())
            .ToTenantPageModel()
            .OrderBy(x => x.Stat)
            .ThenBy(x => x.Name)
            .ToListAsync();

        var inner = new ServersResponse();
        inner.Servers.AddRange(items.Select(s => new ServerItem
        {
            Id   = (int)s.Id,
            Name = s.Name,
            Host = s.Ip,
        }));
        return new GetServersResponse { Data = inner };
    }

    public override async Task<GetServerMetricsResponse> GetServerMetrics(
        GetServerMetricsRequest request, ServerCallContext context)
    {
        var servers = await bcrCtx.Servers
            .ToTenantPageModel()
            .ToListAsync();

        var metrics = new List<ServerMetric>();
        using var client = httpFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(5);

        await Task.WhenAll(servers.Select(async server =>
        {
            double cpu = 0, mem = 0;
            try
            {
                var baseUrl = $"http://{server.Ip}:60618/bcrstat/api/4";
                var cpuJson = await (await client.GetAsync(baseUrl + "/cpu")).Content.ReadAsStringAsync();
                var memJson = await (await client.GetAsync(baseUrl + "/mem")).Content.ReadAsStringAsync();
                // Parse simplified values from raw JSON
                using var cpuDoc = System.Text.Json.JsonDocument.Parse(cpuJson);
                using var memDoc = System.Text.Json.JsonDocument.Parse(memJson);
                if (cpuDoc.RootElement.TryGetProperty("usage", out var cpuVal)) cpu = cpuVal.GetDouble();
                if (memDoc.RootElement.TryGetProperty("usedPercent", out var memVal)) mem = memVal.GetDouble();
            }
            catch { /* timeout or unreachable — leave as 0 */ }

            metrics.Add(new ServerMetric
            {
                ServerId  = (int)server.Id,
                CpuUsage  = cpu,
                MemUsage  = mem,
                UpdatedAt = DateTime.UtcNow.ToString("O"),
            });
        }));

        var inner = new ServerMetricsResponse();
        inner.Metrics.AddRange(metrics);
        return new GetServerMetricsResponse { Data = inner };
    }
}

// ─── Admin RBAC ───────────────────────────────────────────────────────────────

/// <summary>
/// Replaces Areas/Tenant/Controllers/AdminController.cs (RBAC endpoints).
/// Routes defined in admin.proto via google.api.http annotations.
/// </summary>
public class TenantAdminGrpcService(
    AuthDbContext authDb,
    ITenantGetter tenantGetter,
    UserService userService,
    PermissionService permissionSvc)
    : TenantAdminService.TenantAdminServiceBase
{
    // ─── Users ────────────────────────────────────────────────────────────────

    public override async Task<ListAdminUsersResponse> ListAdminUsers(
        ListAdminUsersRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var items = await authDb.Users
            .Where(x => x.TenantId == tenantId)
            .Where(u => u.UserRoles.Any(ur => ur.RoleId < 100))
            .ToUserAdminViewModel()
            .ToListAsync();

        var response = new ListAdminUsersResponse();
        response.Data.AddRange(items.Select(MapToProto));
        return response;
    }

    public override async Task<GetAdminUserResponse> GetAdminUser(
        GetAdminUserRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var user = await authDb.Users
            .Where(x => x.TenantId == tenantId && x.PartyId == request.PartyId)
            .ToUserAdminViewModel()
            .SingleOrDefaultAsync();
        if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var roles = await userService.GetUserRolesAsync(user.PartyId);
        var userPermissions = await permissionSvc.GetUserPermissionIdsAsync(tenantId, user.PartyId);
        var rolePermissions = await permissionSvc.GetRolePermissionIdsAsync(roles.ToArray());
        var quizLockedOut   = await userService.IsUserLockedOutAsync(user.PartyId);

        var detail = new AdminUserDetail
        {
            User          = MapToProto(user),
            QuizLockedOut = quizLockedOut,
        };
        detail.UserPermissions.AddRange(userPermissions.Select(id =>
            new AdminPermission { Id = id }));
        detail.RolePermissions.AddRange(rolePermissions.Select(id =>
            new AdminPermission { Id = id }));
        return new GetAdminUserResponse { Data = detail };
    }

    // ─── Roles ────────────────────────────────────────────────────────────────

    public override async Task<ListAdminRolesResponse> ListAdminRoles(
        ListAdminRolesRequest request, ServerCallContext context)
    {
        var items = await userService.GetAllRolesAsync();
        var inner = new AdminRolesResponse();
        inner.Data.AddRange(items
            .Where(r => r.Id < 100)
            .Select(r => new AdminRole { Id = r.Id, Name = r.Name ?? "" }));
        return new ListAdminRolesResponse { Data = inner };
    }

    public override async Task<GetAdminRoleResponse> GetAdminRole(
        GetAdminRoleRequest request, ServerCallContext context)
    {
        var role = await authDb.ApplicationRoles
            .Where(r => r.Id == request.Id)
            .Select(r => new { r.Id, r.Name })
            .FirstOrDefaultAsync();
        if (role?.Name == null) throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));

        var permIds = await permissionSvc.GetRolePermissionIdsAsync(role.Name);
        var detail  = new AdminRoleDetail { Id = role.Id, Name = role.Name };
        detail.PermissionIds.AddRange(permIds);
        return new GetAdminRoleResponse { Data = detail };
    }

    // ─── Permissions ──────────────────────────────────────────────────────────

    public override async Task<ListAdminPermissionsResponse> ListAdminPermissions(
        ListAdminPermissionsRequest request, ServerCallContext context)
    {
        var items    = await permissionSvc.GetAllAsync();
        var inner = new AdminPermissionsResponse();
        inner.Data.AddRange(items.Select(MapPermToProto));
        return new ListAdminPermissionsResponse { Data = inner };
    }

    public override async Task<CreateAdminPermissionResponse> CreateAdminPermission(
        CreateAdminPermissionRequest request, ServerCallContext context)
    {
        var ok = await permissionSvc.CreateAsync(
            request.Auth, request.Action, request.Method, request.Category, request.Key);
        if (!ok) throw new RpcException(new Status(StatusCode.AlreadyExists, "Permission already exists"));

        var created = await authDb.Permissions
            .Where(x => x.Action == request.Action && x.Method == request.Method && x.Category == request.Category)
            .FirstOrDefaultAsync()
            ?? throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve created permission"));
        return new CreateAdminPermissionResponse { Data = MapPermToProto(created) };
    }

    public override async Task<TogglePermissionResponse> TogglePermission(
        TogglePermissionRequest request, ServerCallContext context)
    {
        var ok = await permissionSvc.ToggleAuthAsync(request.Id);
        return new TogglePermissionResponse { Success = ok };
    }

    // ─── User role / permission toggles ──────────────────────────────────────

    public override async Task<ToggleUserRoleResponse> ToggleUserRole(
        ToggleUserRoleRequest request, ServerCallContext context)
    {
        var partyId = await authDb.Users
            .Where(x => x.Id == request.Id)
            .Select(x => x.PartyId)
            .SingleOrDefaultAsync();
        if (partyId == 0) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var roleName = await authDb.ApplicationRoles
            .Where(x => x.Id == request.RoleId)
            .Select(x => x.Name)
            .SingleOrDefaultAsync();
        if (roleName == null) throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));

        var operatorPartyId = GetPartyId(context);
        var ok = await userService.HasRoleAsync(partyId, roleName)
            ? await userService.RemoveRoleAsync(partyId, roleName, operatorPartyId)
            : await userService.AddRoleAsync(partyId, roleName, operatorPartyId);

        return new ToggleUserRoleResponse { Success = ok };
    }

    public override async Task<ToggleUserPermissionResponse> ToggleUserPermission(
        ToggleUserPermissionRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var partyId  = await authDb.Users
            .Where(x => x.Id == request.Id)
            .Select(x => x.PartyId)
            .SingleOrDefaultAsync();
        if (partyId == 0) throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var ok = await permissionSvc.UserHasPermissionAsync(tenantId, partyId, request.PermissionId)
            ? await permissionSvc.RemoveUserPermissionAsync(tenantId, partyId, request.PermissionId)
            : await permissionSvc.AddUserPermissionAsync(tenantId, partyId, request.PermissionId);

        return new ToggleUserPermissionResponse { Success = ok };
    }

    public override async Task<ToggleRolePermissionResponse> ToggleRolePermission(
        ToggleRolePermissionRequest request, ServerCallContext context)
    {
        var roleName = await authDb.ApplicationRoles
            .Where(x => x.Id == request.Id)
            .Select(x => x.Name)
            .SingleOrDefaultAsync();
        if (roleName == null) throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));

        var ok = await permissionSvc.RoleHasPermissionAsync(roleName, request.PermissionId)
            ? await permissionSvc.RemoveRolePermissionAsync(roleName, request.PermissionId)
            : await permissionSvc.AddRolePermissionAsync(roleName, request.PermissionId);

        return new ToggleRolePermissionResponse { Success = ok };
    }

    // ─── User permissions by party ID (v2) ───────────────────────────────────

    public override async Task<GetUserPermissionsByPartyIdResponse> GetUserPermissionsByPartyId(
        GetUserPermissionsByPartyIdRequest request, ServerCallContext context)
    {
        var tenantId        = tenantGetter.GetTenantId();
        var webPermissions  = await permissionSvc.GetUserWebPermissions(tenantId, request.PartyId);
        var roles           = await userService.GetUserRolesAsync(request.PartyId);
        var rolePermIds     = await permissionSvc.GetRolePermissionIdsAsync(roles.ToArray());

        var allPerms    = await permissionSvc.GetAllAsync();
        var rolePermActions = allPerms
            .Where(p => rolePermIds.Contains(p.Id) && p.Category == "WEB")
            .Select(p => p.Action)
            .Distinct()
            .ToList();

        var inner = new UserPermissionsDetailResponse();
        inner.UserPermissions.AddRange(webPermissions);
        inner.RolePermissions.AddRange(rolePermActions);
        return new GetUserPermissionsByPartyIdResponse { Data = inner };
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static AdminUser MapToProto(Bacera.Gateway.ViewModels.Tenant.UserAdminViewModel u)
    {
        var proto = new AdminUser
        {
            Id         = u.Id,
            Uid        = u.Uid,
            PartyId    = u.PartyId,
            Email      = u.Email,
            Avatar     = u.Avatar,
            Language   = u.Language,
            FirstName  = u.FirstName ?? "",
            LastName   = u.LastName  ?? "",
            NativeName = u.NativeName ?? "",
        };
        proto.RoleIds.AddRange(u.Roles);
        return proto;
    }

    private static AdminPermission MapPermToProto(Bacera.Gateway.Permission p) => new()
    {
        Id        = p.Id,
        Action    = p.Action,
        Method    = p.Method,
        Category  = p.Category,
        Key       = p.Key,
        Auth      = p.Auth,
        CreatedAt = p.CreatedOn.ToString("O"),
        UpdatedAt = p.UpdatedOn.ToString("O"),
    };

    private static long GetPartyId(ServerCallContext ctx)
        => ctx.GetHttpContext().User.GetPartyId();
}
