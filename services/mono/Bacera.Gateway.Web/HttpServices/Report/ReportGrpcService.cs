using System.Collections.Concurrent;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.Services;
using Grpc.Core;
using Http.V1;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProtoReportRequest = Http.V1.ReportRequest;
using ProtoAccountReport = Http.V1.AccountReport;

namespace Bacera.Gateway.Web.HttpServices.Report;

/// <summary>
/// gRPC JSON Transcoding implementation of TenantReportService.
/// Replaces Areas/Tenant/Controllers/ReportController.cs.
/// Routes defined via google.api.http annotations in report.proto.
/// </summary>
public class TenantReportGrpcService(
    TenantDbContext tenantCtx,
    ITenantGetter tenantGetter,
    IReportServiceClient reportServiceClient,
    IReportJob reportJob,
    ReportService reportService)
    : TenantReportService.TenantReportServiceBase
{
    public override async Task<ListReportRequestsResponse> ListReportRequests(
        ListReportRequestsRequest request, ServerCallContext context)
    {
        var page = request.Pagination?.Page > 0 ? request.Pagination.Page : 1;
        var size = request.Pagination?.Size > 0 ? request.Pagination.Size : 20;

        var query = tenantCtx.ReportRequests.AsQueryable();
        if (request.HasType)   query = query.Where(x => x.Type == request.Type);

        query = query
            .OrderByDescending(x => x.GeneratedOn.HasValue)
            .ThenByDescending(x => x.GeneratedOn)
            .ThenByDescending(x => x.Id);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();

        var response = new ListReportRequestsResponse
        {
            Meta = new PaginationMeta
            {
                Page      = page,
                Size      = size,
                Total     = total,
                PageCount = (int)Math.Ceiling((double)total / size),
                HasMore   = page * size < total,
            }
        };
        response.Data.AddRange(items.Select(MapToProto));
        return response;
    }

    public override async Task<ProtoReportRequest> GetReportRequest(
        GetReportRequestRequest request, ServerCallContext context)
    {
        var item = await tenantCtx.ReportRequests.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Report not found"));
        return MapToProto(item);
    }

    public override Task<QuerySampleResponse> GetQuerySample(
        GetQuerySampleRequest request, ServerCallContext context)
    {
        var sample = (ReportRequestTypes)request.Type switch
        {
            ReportRequestTypes.Rebate => JsonConvert.SerializeObject(new Bacera.Gateway.Rebate.Criteria()),
            _ => "{}"
        };
        return Task.FromResult(new QuerySampleResponse { Sample = sample });
    }

    public override async Task<QueryPreviewResponse> PreviewQuery(
        PreviewQueryRequest request, ServerCallContext context)
    {
        var data = (ReportRequestTypes)request.Type switch
        {
            ReportRequestTypes.Rebate => await PreviewRebateAsync(request.Query),
            _ => "[]"
        };
        return new QueryPreviewResponse { Data = data };
    }

    public override async Task<ProtoReportRequest> CreateReportRequest(
        CreateReportRequestRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var spec = new Bacera.Gateway.ReportRequest.CreateSpec
        {
            Type      = (ReportRequestTypes)request.Type,
            Name      = $"{(ReportRequestTypes)request.Type} Report",
            Query     = request.Parameters,
            IsFromApi = 1,
        };

        var item = Bacera.Gateway.ReportRequest.Build(partyId, spec.Type, spec.Name, request.Parameters);
        item.IsFromApi = 1;

        if (!ReportService.ValidateQueryString(item))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "__INVALID_QUERY__"));

        tenantCtx.ReportRequests.Add(item);
        await tenantCtx.SaveChangesAsync();

        await reportServiceClient.EnqueueProcessReportRequestAsync(tenantGetter.GetTenantId(), item.Id);

        return MapToProto(item);
    }

    public override async Task<ProtoReportRequest> RegenReport(
        GetReportRequestRequest request, ServerCallContext context)
    {
        var item = await tenantCtx.ReportRequests.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Report not found"));

        item.PartyId     = GetPartyId(context);
        item.FileName    = "";
        item.GeneratedOn = null;

        await tenantCtx.SaveChangesAsync();
        await reportServiceClient.EnqueueProcessReportRequestAsync(tenantGetter.GetTenantId(), item.Id);

        return MapToProto(item);
    }

    public override async Task<DownloadReportResponse> DownloadReport(
        CreateReportRequestRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var item = Bacera.Gateway.ReportRequest.Build(
            partyId,
            (ReportRequestTypes)request.Type,
            $"{(ReportRequestTypes)request.Type} Download",
            request.Parameters);
        item.IsFromApi = 1;

        tenantCtx.ReportRequests.Add(item);
        await tenantCtx.SaveChangesAsync();

        // Async enqueue: report generated in background; client polls via GetReportRequest(request_id)
        await reportServiceClient.EnqueueProcessReportRequestAsync(tenantGetter.GetTenantId(), item.Id);

        return new DownloadReportResponse { RequestId = item.Id, Url = item.FileName ?? "" };
    }

    public override async Task<ProtoReportRequest> CreateEquityReport(
        CreateEquityReportRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var item = Bacera.Gateway.ReportRequest.Build(
            partyId,
            ReportRequestTypes.DailyEquity,
            "Daily Equity Report",
            request.Configuration);
        item.IsFromApi = 1;

        tenantCtx.ReportRequests.Add(item);
        await tenantCtx.SaveChangesAsync();

        await reportServiceClient.EnqueueProcessReportRequestAsync(tenantGetter.GetTenantId(), item.Id);

        return MapToProto(item);
    }

    public override async Task<ProtoReportRequest> SimulateJob(
        CreateReportRequestRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var item = Bacera.Gateway.ReportRequest.Build(
            partyId,
            (ReportRequestTypes)request.Type,
            $"{(ReportRequestTypes)request.Type} Report",
            request.Parameters);
        item.IsFromApi = 1;

        if (!ReportService.ValidateQueryString(item))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "__INVALID_QUERY__"));

        tenantCtx.ReportRequests.Add(item);
        await tenantCtx.SaveChangesAsync();

        await reportJob.ProcessReportRequest(tenantGetter.GetTenantId(), item.Id);

        var updated = await tenantCtx.ReportRequests.SingleOrDefaultAsync(x => x.Id == item.Id);
        return MapToProto(updated ?? item);
    }

    public override async Task<EquityReportHtmlResponse> GetEquityReportHtml(
        CreateEquityReportRequest request, ServerCallContext context)
    {
        var cfg = JsonConvert.DeserializeObject<ReportConfiguration>(request.Configuration)
                  ?? new ReportConfiguration();
        var html = await ReportJob.GenerateEquityReportHtmlAsync(reportService, cfg);
        return new EquityReportHtmlResponse { Html = html ?? "" };
    }

    public override async Task<EquityReportLoginsResponse> GetEquityReportLogins(
        CreateEquityReportRequest request, ServerCallContext context)
    {
        var cfg = JsonConvert.DeserializeObject<ReportConfiguration>(request.Configuration)
                  ?? new ReportConfiguration();

        var dict = new ConcurrentDictionary<string, ConcurrentDictionary<string, List<long>>>();
        await Task.WhenAll(cfg.Items.Select(async item =>
        {
            dict.GetOrAdd(item.Group, new ConcurrentDictionary<string, List<long>>())[item.Name] =
                await reportService.GetLogins(item, cfg.ServiceId);
        }));

        var ordered = dict
            .OrderBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value.OrderBy(y => y.Key).ToDictionary(y => y.Key, y => y.Value));

        return new EquityReportLoginsResponse { Data = JsonConvert.SerializeObject(ordered) };
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<string> PreviewRebateAsync(string queryJson)
    {
        var criteria = JsonConvert.DeserializeObject<Bacera.Gateway.Rebate.Criteria>(queryJson)
                       ?? new Bacera.Gateway.Rebate.Criteria();
        criteria.Page = 1;
        criteria.Size = 20;
        var items = await tenantCtx.Rebates.PagedFilterBy(criteria).ToListAsync();
        return JsonConvert.SerializeObject(items);
    }

    private static ProtoReportRequest MapToProto(Bacera.Gateway.ReportRequest r)
        => new ProtoReportRequest
        {
            Id        = r.Id,
            Type      = r.Type,
            Status    = r.IsGenerated ? 2 : 0,
            FileUrl   = r.FileName ?? "",
            CreatedAt = r.CreatedOn.ToString("O"),
        };

    private static long GetPartyId(ServerCallContext ctx)
    {
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("PartyId", out var v) && v is long id ? id : 0;
    }
}

/// <summary>
/// gRPC JSON Transcoding implementation of TenantAccountReportService.
/// Replaces Areas/Tenant/Controllers/AccountReportController.cs.
/// Routes defined via google.api.http annotations in report.proto.
/// </summary>
public class TenantAccountReportGrpcService(
    TenantDbContext tenantCtx,
    ReportService reportService,
    IMyCache myCache)
    : TenantAccountReportService.TenantAccountReportServiceBase
{
    public override async Task<ListAccountReportsResponse> ListAccountReports(
        ListAccountReportsRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.AccountReport.Criteria
        {
            Page      = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size      = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            AccountId = request.HasAccountId ? request.AccountId : null,
        };

        var items = await tenantCtx.AccountReports
            .PagedFilterBy(criteria)
            .ToListAsync();

        var response = new ListAccountReportsResponse
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
        response.Data.AddRange(items.Select(r => new ProtoAccountReport
        {
            Id        = r.Id,
            AccountId = r.AccountId,
            Status    = r.Status,
            CreatedAt = r.CreatedOn.ToString("O"),
        }));
        return response;
    }

    public override async Task<AccountReportPreviewResponse> PreviewAccountReport(
        GetAccountReportRequest request, ServerCallContext context)
    {
        var html = await reportService.TryGetAccountDailyReportModelPreviewHtml(request.Id);
        return new AccountReportPreviewResponse { HtmlContent = html ?? "" };
    }

    public override async Task<OperationResponse> SendAccountReport(
        SendAccountReportRequest request, ServerCallContext context)
    {
        var (result, msg) = await reportService.SendAccountReportEmailById(request.Id, receiverEmail: request.Spec?.Email);
        return new OperationResponse { Success = result, Message = msg ?? "" };
    }

    public override async Task<AccountReportTodayStatusResponse> GetTodayStatus(
        EmptyRequest request, ServerCallContext context)
    {
        var today = DateTime.UtcNow.Date;
        var reports = await tenantCtx.AccountReports
            .Where(x => x.CreatedOn.Date == today)
            .GroupBy(_ => 1)
            .Select(g => new { Total = g.Count(), Sent = g.Count(x => x.Status == 1) })
            .FirstOrDefaultAsync();

        return new AccountReportTodayStatusResponse
        {
            Total   = reports?.Total ?? 0,
            Sent    = reports?.Sent  ?? 0,
            Pending = (reports?.Total ?? 0) - (reports?.Sent ?? 0),
        };
    }
}
