using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.ViewModels.Tenant;
using Grpc.Core;
using Http.V1;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.HttpServices.Lead;

using M = global::Bacera.Gateway.Lead;

/// <summary>
/// gRPC JSON Transcoding implementation of TenantLeadService.
/// Replaces Areas/Tenant/Controllers/LeadController.cs.
/// Routes defined via google.api.http annotations in lead.proto.
/// </summary>
public class TenantLeadGrpcService(
    TenantDbContext tenantDb,
    ILeadService leadSvc,
    ConfigService configSvc)
    : TenantLeadService.TenantLeadServiceBase
{
    // ─── List ─────────────────────────────────────────────────────────────────

    public override async Task<ListLeadsResponse> ListLeads(
        ListLeadsRequest request, ServerCallContext context)
    {
        var utmsParam = request.Utms.Count > 0 ? request.Utms.ToArray() : [];

        var hasUtm    = request.HasHasUtm ? (bool?)request.HasUtm : null;
        var utmsFilter = request.Utms.Count > 0 ? request.Utms.ToList() : null;

        var criteria = new M.Criteria
        {
            Page                = request.Pagination?.Page > 0 ? request.Pagination.Page : request.HasPage && request.Page > 0 ? request.Page : 1,
            Size                = request.Pagination?.Size > 0 ? request.Pagination.Size : request.HasSize && request.Size > 0 ? request.Size : 20,
            Keyword             = request.HasKeyword            ? request.Keyword            : null,
            AssignedAccountUid  = request.HasAssignedAccountUid ? request.AssignedAccountUid : null,
            IsAssigned          = request.HasIsAssigned         ? (bool?)request.IsAssigned  : null,
            HasReferCode        = request.HasHasReferCode       ? (bool?)request.HasReferCode : null,
            IsArchived          = request.HasIsArchived         ? (LeadIsArchivedTypes?)request.IsArchived : null,
            Status              = request.HasStatus             ? (LeadStatusTypes?)request.Status        : null,
            SourceType          = request.HasSourceType         ? (int?)request.SourceType   : null,
            From                = request.HasDateFrom           ? DateTime.Parse(request.DateFrom)        : null,
            To                  = request.HasDateTo             ? DateTime.Parse(request.DateTo)          : null,
            HasTradeAccount     = request.HasHasTradeAccount    ? (bool?)request.HasTradeAccount          : null,
            HasDeposit          = request.HasHasDeposit         ? (bool?)request.HasDeposit  : null,
            HasUtm              = hasUtm,
            Utms                = utmsFilter,
        };

        // UTM filter uses raw SQL (same as existing LeadController.Index)
        IQueryable<global::Bacera.Gateway.Lead> query;
        if (hasUtm.HasValue || utmsFilter != null)
        {
            query = tenantDb.Leads.FromSqlInterpolated($"""
                SELECT t.*
                FROM trd."_Lead" t
                WHERE
                    (
                    {hasUtm == null} or "Supplement"::jsonb ->> 'utm' <> ''
                    )
                    AND
                    (
                        ({utmsFilter == null}) OR
                        ("Supplement"::jsonb -> 'utm' ->> 'data' = ANY({utmsParam}))
                    )
                """);
        }
        else
        {
            query = tenantDb.Leads;
        }

        var items = await query
            .PagedFilterBy(criteria)
            .ToResponseModel()
            .ToListAsync();

        var response = new ListLeadsResponse
        {
            Criteria = BuildMeta(criteria.Page, criteria.Size, criteria.Total)
        };
        response.Data.AddRange(items.Select(MapToProto));
        return response;
    }

    // ─── Get detail ───────────────────────────────────────────────────────────

    public override async Task<GetLeadResponse> GetLead(GetLeadRequest request, ServerCallContext context)
    {
        var item = await leadSvc.GetAsync(request.Id);
        if (item.IsEmpty())
            throw new RpcException(new Status(StatusCode.NotFound, "Lead not found"));

        return new GetLeadResponse { Data = MapToDetailProto(item) };
    }

    // ─── Create ───────────────────────────────────────────────────────────────

    public override async Task<CreateLeadResponse> CreateLead(CreateLeadRequest request, ServerCallContext context)
    {
        var spec = new M.CreateSpec
        {
            Name         = request.Spec.Name,
            Email        = request.Spec.Email,
            PhoneNumber  = request.Spec.PhoneNumber,
            PartyId      = request.Spec.HasPartyId     ? request.Spec.PartyId                       : 0,
            Status       = request.Spec.HasStatus      ? (LeadStatusTypes)request.Spec.Status       : LeadStatusTypes.UserNotRegistered,
            SourceType   = request.Spec.HasSourceType  ? (LeadSourceTypes)request.Spec.SourceType   : LeadSourceTypes.ManuallyAdd,
            SourceComment = request.Spec.HasSourceComment ? request.Spec.SourceComment              : null,
        };

        var entity = await leadSvc.CreateAsync(spec);
        return new CreateLeadResponse { Data = MapEntityToProto(entity) };
    }

    // ─── Archive / Unarchive ──────────────────────────────────────────────────

    public override async Task<ArchiveLeadResponse> ArchiveLead(ArchiveLeadRequest request, ServerCallContext context)
    {
        var ok = await leadSvc.Archive(request.Id, LeadIsArchivedTypes.Archived);
        if (!ok) throw new RpcException(new Status(StatusCode.Internal, "__LEAD_ARCHIVE_FAILED__"));
        return new ArchiveLeadResponse { Data = await GetLeadEntityProto(request.Id) };
    }

    public override async Task<UnarchiveLeadResponse> UnarchiveLead(UnarchiveLeadRequest request, ServerCallContext context)
    {
        var ok = await leadSvc.Archive(request.Id, LeadIsArchivedTypes.Unarchived);
        if (!ok) throw new RpcException(new Status(StatusCode.Internal, "__LEAD_ARCHIVE_FAILED__"));
        return new UnarchiveLeadResponse { Data = await GetLeadEntityProto(request.Id) };
    }

    // ─── Comment ──────────────────────────────────────────────────────────────

    public override async Task<AddLeadCommentResponse> AddLeadComment(AddLeadCommentRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var operatorName = await tenantDb.Parties
            .Where(x => x.Id == partyId)
            .Select(x => x.NativeName)
            .SingleOrDefaultAsync();

        var content = $"Tenant-{operatorName}-Comment: " + request.Spec.Content;
        var ok = await leadSvc.AddComment(request.Id, content, partyId);
        if (!ok) throw new RpcException(new Status(StatusCode.Internal, "__LEAD_COMMENT_FAILED__"));
        return new AddLeadCommentResponse { Data = await GetLeadEntityProto(request.Id) };
    }

    // ─── Assign / Unassign ────────────────────────────────────────────────────

    public override async Task<AssignLeadResponse> AssignLead(AssignLeadRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var ok = await leadSvc.AssignOwnerAccount(request.Id, request.AssignedAccountUid, partyId);
        if (!ok) throw new RpcException(new Status(StatusCode.Internal, "__LEAD_ASSIGN_FAILED__"));
        return new AssignLeadResponse { Data = await GetLeadEntityProto(request.Id) };
    }

    public override async Task<UnassignLeadResponse> UnassignLead(UnassignLeadRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var ok = await leadSvc.UnAssignOwnerAccount(request.Id, request.AssignedAccountUid, partyId);
        if (!ok) throw new RpcException(new Status(StatusCode.Internal, "__LEAD_ASSIGN_FAILED__"));
        return new UnassignLeadResponse { Data = await GetLeadEntityProto(request.Id) };
    }

    public override async Task<BulkAssignLeadsResponse> BulkAssignLeads(
        BulkAssignLeadsRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var results = await Task.WhenAll(
            request.LeadIds.Select(id => leadSvc.AssignOwnerAccount(id, request.AssignedAccountUid, partyId)));
        return new BulkAssignLeadsResponse { Success = results.All(r => r), Message = $"{results.Count(r => r)}/{results.Length}" };
    }

    // ─── Auto-assign ──────────────────────────────────────────────────────────

    public override async Task<GetAutoAssignInfoResponse> GetAutoAssignInfo(GetAutoAssignInfoRequest request, ServerCallContext context)
    {
        var info = await configSvc.GetAsync<M.AutoAssignInfo>(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo)
                   ?? new M.AutoAssignInfo();
        return new GetAutoAssignInfoResponse { Data = MapAutoAssign(info) };
    }

    public override async Task<EnableAutoAssignResponse> EnableAutoAssign(EnableAutoAssignRequest request, ServerCallContext context)
    {
        var info = await configSvc.GetAsync<M.AutoAssignInfo>(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo)
                   ?? new M.AutoAssignInfo();
        info.Enabled = true;
        await configSvc.SetAsync(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo, info);
        return new EnableAutoAssignResponse { Data = MapAutoAssign(info) };
    }

    public override async Task<DisableAutoAssignResponse> DisableAutoAssign(DisableAutoAssignRequest request, ServerCallContext context)
    {
        var info = await configSvc.GetAsync<M.AutoAssignInfo>(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo)
                   ?? new M.AutoAssignInfo();
        info.Enabled = false;
        await configSvc.SetAsync(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo, info);
        return new DisableAutoAssignResponse { Data = MapAutoAssign(info) };
    }

    public override async Task<SetAutoAssigneeResponse> SetAutoAssignee(SetAutoAssigneeRequest request, ServerCallContext context)
    {
        var info = await configSvc.GetAsync<M.AutoAssignInfo>(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo)
                   ?? new M.AutoAssignInfo();
        info.AutoAssignAccountUid = request.AutoAssignAccountUid;
        info.Enabled = request.Enabled;
        await configSvc.SetAsync(nameof(Public), 0, ConfigKeys.AutoAssignLeadInfo, info);
        return new SetAutoAssigneeResponse { Data = MapAutoAssign(info) };
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Http.V1.Lead> GetLeadEntityProto(long id)
    {
        var item = await tenantDb.Leads
            .Where(x => x.Id == id)
            .ToResponseModel()
            .SingleOrDefaultAsync();
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Lead not found"));
        return MapToProto(item);
    }

    private static Http.V1.Lead MapToProto(LeadBasicViewModel m) => new()
    {
        Id                 = m.Id,
        PartyId            = m.PartyId,
        Name               = m.Name,
        Email              = m.Email,
        PhoneNumber        = m.PhoneNumber,
        SourceType         = (int)m.SourceType,
        Status             = (int)m.Status,
        HasAssignedToSales = m.HasAssignedToSales,
        Utm                = m.Utm,
        CreatedAt          = m.CreatedOn.ToString("O"),
        UpdatedAt          = m.UpdatedOn.ToString("O"),
    };

    private static Http.V1.Lead MapEntityToProto(global::Bacera.Gateway.Lead e) => new()
    {
        Id          = e.Id,
        PartyId     = e.PartyId ?? 0,
        Name        = e.Name,
        Email       = e.Email,
        PhoneNumber = e.PhoneNumber,
        SourceType  = e.SourceType,
        Status      = e.Status,
        IsArchived  = e.IsArchived,
        CreatedAt   = e.CreatedOn.ToString("O"),
        UpdatedAt   = e.UpdatedOn.ToString("O"),
    };

    private static LeadDetail MapToDetailProto(LeadDetailViewModel m)
    {
        var detail = new LeadDetail
        {
            Id                 = m.Id,
            PartyId            = m.PartyId,
            Name               = m.Name,
            Email              = m.Email,
            PhoneNumber        = m.PhoneNumber,
            SourceType         = (int)m.SourceType,
            Status             = (int)m.Status,
            HasAssignedToSales = m.HasAssignedToSales,
            CreatedAt          = m.CreatedOn.ToString("O"),
            UpdatedAt          = m.UpdatedOn.ToString("O"),
        };
        detail.AssignedAccounts.AddRange(m.AssignedAccounts.Select(a => new AccountBasic
        {
            Id   = a.Id,
            Uid  = a.Uid,
            Name = a.Name,
            Role = (int)a.Role,
        }));
        detail.Comments.AddRange(m.UpdateLogs.Select(c => new LeadComment
        {
            Id        = c.Id,
            Content   = c.Content,
            CreatedAt = c.CreatedOn.ToString("O"),
        }));
        return detail;
    }

    private static AutoAssignInfo MapAutoAssign(M.AutoAssignInfo info) => new()
    {
        AutoAssignAccountUid = info.AutoAssignAccountUid,
        Enabled              = info.Enabled,
    };

    private static long GetPartyId(ServerCallContext ctx)
    {
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("PartyId", out var v) && v is long id ? id : 0;
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
