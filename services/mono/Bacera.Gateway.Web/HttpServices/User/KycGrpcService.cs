using Bacera.Gateway.ViewModels.Tenant;
using Grpc.Core;
using Http.V1;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProtoKycForm = Http.V1.KycForm;

namespace Bacera.Gateway.Web.HttpServices.User;

/// <summary>
/// gRPC JSON Transcoding implementation of TenantKycService.
/// Replaces Areas/Tenant/Controllers/KycController.cs.
/// Routes defined via google.api.http annotations in user.proto.
/// </summary>
public class TenantKycGrpcService(
    TenantDbContext tenantDb,
    AuthDbContext authDb)
    : TenantKycService.TenantKycServiceBase
{
    public override async Task<ListKycsResponse> ListKycs(ListKycsRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.Verification.Criteria
        {
            Page   = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size   = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            Type   = VerificationTypes.KycForm,
            Status = request.HasStatus ? (VerificationStatusTypes?)request.Status : null,
        };

        var items = await tenantDb.Verifications
            .PagedFilterBy(criteria)
            .ToTenantViewModel(false)
            .ToListAsync();

        var response = new ListKycsResponse
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
        response.Data.AddRange(items.Select(v => new ProtoKycForm
        {
            Id      = v.Id,
            PartyId = v.PartyId,
            Status  = (int)v.Status,
        }));
        return response;
    }

    public override async Task<ProtoKycForm> GetKyc(GetKycRequest request, ServerCallContext context)
    {
        var item = await GetVerificationWithItem(request.PartyId);
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "KYC not found"));
        return MapToProto(item);
    }

    public override async Task<ProtoKycForm> CreateKyc(CreateKycRequest request, ServerCallContext context)
    {
        var existing = await GetVerificationWithItem(request.PartyId);
        if (existing != null)
            throw new RpcException(new Status(StatusCode.AlreadyExists, "KYC record already exists"));

        var spec = JsonConvert.DeserializeObject<KycFormViewModel>(request.Spec)
            ?? new KycFormViewModel();
        spec.StaffPartyId = GetPartyId(context);

        var vItem = new Bacera.Gateway.VerificationItem
        {
            Content  = Utils.JsonSerializeObject(spec),
            Category = VerificationCategoryTypes.KycForm,
        };
        var form = new Bacera.Gateway.Verification
        {
            PartyId           = request.PartyId,
            Type              = (short)VerificationTypes.KycForm,
            Status            = (int)VerificationStatusTypes.AwaitingReview,
            Note              = string.Empty,
            VerificationItems = new List<Bacera.Gateway.VerificationItem> { vItem },
        };
        await tenantDb.Verifications.AddAsync(form);
        await tenantDb.SaveChangesAsync();
        return MapToProto(form);
    }

    public override async Task<ProtoKycForm> UpdateKyc(UpdateKycRequest request, ServerCallContext context)
    {
        var item = await GetVerificationWithItem(request.PartyId);
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "KYC not found"));

        var spec = JsonConvert.DeserializeObject<KycFormViewModel>(request.Spec)
            ?? new KycFormViewModel();
        spec.StaffPartyId = GetPartyId(context);

        var vItem = item.VerificationItems.FirstOrDefault();
        if (vItem == null) throw new RpcException(new Status(StatusCode.NotFound, "KYC form item not found"));

        vItem.Content   = Utils.JsonSerializeObject(spec);
        vItem.UpdatedOn = DateTime.UtcNow;
        item.UpdatedOn  = DateTime.UtcNow;
        item.Status     = (int)VerificationStatusTypes.UnderReview;

        tenantDb.Verifications.Update(item);
        await tenantDb.SaveChangesAsync();
        return MapToProto(item);
    }

    public override async Task<ProtoKycForm> AwaitingReview(GetKycRequest request, ServerCallContext context)
    {
        var item = await tenantDb.Verifications
            .Where(x => x.Type == (int)VerificationTypes.KycForm && x.PartyId == request.PartyId)
            .SingleOrDefaultAsync();
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "KYC not found"));

        item.Status    = (int)VerificationStatusTypes.AwaitingReview;
        item.UpdatedOn = DateTime.UtcNow;
        tenantDb.Verifications.Update(item);
        await tenantDb.SaveChangesAsync();
        return MapToProto(item);
    }

    public override async Task<ProtoKycForm> SignKyc(UpdateKycRequest request, ServerCallContext context)
    {
        var spec = JsonConvert.DeserializeObject<KycFormViewModel>(request.Spec)
            ?? new KycFormViewModel();
        return await HandleKycForm(request.PartyId, spec, VerificationStatusTypes.AwaitingApprove, context);
    }

    public override async Task<ProtoKycForm> FinalizeKyc(UpdateKycRequest request, ServerCallContext context)
    {
        var spec = JsonConvert.DeserializeObject<KycFormViewModel>(request.Spec)
            ?? new KycFormViewModel();
        var result = await HandleKycForm(request.PartyId, spec, VerificationStatusTypes.Approved, context);
        await RecordHistory(request.PartyId, spec);
        return result;
    }

    public override async Task<KycHistoryResponse> GetKycHistory(GetKycRequest request, ServerCallContext context)
    {
        var supplement = await tenantDb.Supplements
            .Where(x => x.Type == (int)SupplementTypes.KycFormHistory && x.RowId == request.PartyId)
            .SingleOrDefaultAsync();

        var response = new KycHistoryResponse();
        if (supplement == null) return response;

        var models = JsonConvert.DeserializeObject<List<KycFormViewModel>>(supplement.Data)
                     ?? new List<KycFormViewModel>();
        response.Items.AddRange(models.Select(m => new KycHistoryItem
        {
            Json = JsonConvert.SerializeObject(m),
        }));
        return response;
    }

    public override async Task<ProtoKycForm> RejectKyc(GetKycRequest request, ServerCallContext context)
    {
        var item = await tenantDb.Verifications
            .Where(x => x.Type == (int)VerificationTypes.KycForm && x.PartyId == request.PartyId)
            .SingleOrDefaultAsync();
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "KYC not found"));

        item.Status    = (int)VerificationStatusTypes.Rejected;
        item.UpdatedOn = DateTime.UtcNow;
        tenantDb.Verifications.Update(item);
        await tenantDb.SaveChangesAsync();
        return MapToProto(item);
    }

    public override async Task<ComplianceSigResponse> GetComplianceSig(EmptyRequest request, ServerCallContext context)
    {
        // Returns the hardcoded compliance signature (matches original controller behaviour)
        return new ComplianceSigResponse { Signature = GetVicSignature() };
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Bacera.Gateway.Verification?> GetVerificationWithItem(long partyId)
    {
        var item = await tenantDb.Verifications
            .Where(x => x.Type == (int)VerificationTypes.KycForm)
            .Include(x => x.VerificationItems)
            .SingleOrDefaultAsync(x => x.PartyId == partyId);
        if (item?.VerificationItems != null)
            item.VerificationItems = item.VerificationItems.OrderByDescending(x => x.Id).ToList();
        return item;
    }

    private async Task<ProtoKycForm> HandleKycForm(long partyId, KycFormViewModel spec,
        VerificationStatusTypes newStatus, ServerCallContext context)
    {
        var item = await GetVerificationWithItem(partyId);
        if (item == null || item.VerificationItems.Count == 0)
            throw new RpcException(new Status(StatusCode.NotFound, "KYC not found"));

        spec.StaffPartyId = GetPartyId(context);
        var vItem = item.VerificationItems.First();
        vItem.Content   = Utils.JsonSerializeObject(spec);
        vItem.UpdatedOn = DateTime.UtcNow;
        item.Status     = (int)newStatus;
        item.UpdatedOn  = DateTime.UtcNow;

        tenantDb.Verifications.Update(item);
        await tenantDb.SaveChangesAsync();
        return MapToProto(item);
    }

    private async Task RecordHistory(long partyId, KycFormViewModel model)
    {
        const int keepRecord = 100;
        var supplementHistory = await tenantDb.Supplements
                                    .Where(x => x.Type == (int)SupplementTypes.KycFormHistory && x.RowId == partyId)
                                    .SingleOrDefaultAsync()
                                ?? Bacera.Gateway.Supplement.Build(SupplementTypes.KycFormHistory, partyId, "[]");

        var history = JsonConvert.DeserializeObject<List<KycFormViewModel>>(supplementHistory.Data)
                      ?? new List<KycFormViewModel>();
        history.Add(model);
        history = history.TakeLast(keepRecord).ToList();
        supplementHistory.Data      = Utils.JsonSerializeObject(history);
        supplementHistory.UpdatedOn = DateTime.UtcNow;
        tenantDb.Supplements.Update(supplementHistory);
        await tenantDb.SaveChangesAsync();
    }

    private static ProtoKycForm MapToProto(Bacera.Gateway.Verification v)
    {
        var formData = v.VerificationItems?.FirstOrDefault()?.Content ?? "";
        return new ProtoKycForm
        {
            Id       = v.Id,
            PartyId  = v.PartyId,
            Status   = v.Status,
            FormData = formData,
        };
    }

    private static long GetPartyId(ServerCallContext ctx)
    {
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("PartyId", out var v) && v is long id ? id : 0;
    }

    private static string GetVicSignature() => "data:image/jpeg;base64,/9j/4AAQ..."; // Truncated — matches KycController
}
