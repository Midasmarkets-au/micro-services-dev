using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.EventHandlers;
using Grpc.Core;
using Http.V1;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProtoVerification     = Http.V1.Verification;
using ProtoVerificationItem = Http.V1.VerificationItem;

namespace Bacera.Gateway.Web.HttpServices.User;

/// <summary>
/// gRPC JSON Transcoding implementation of TenantVerificationService.
/// Replaces Areas/Tenant/Controllers/VerificationController.cs.
/// Routes defined via google.api.http annotations in user.proto.
/// </summary>
public class TenantVerificationGrpcService(
    TenantDbContext tenantCtx,
    IMediator mediator,
    ITenantGetter tenantGetter,
    UserService userSvc,
    TagService tagSvc)
    : TenantVerificationService.TenantVerificationServiceBase
{
    private long TenantId => tenantGetter.GetTenantId();

    // ─── List / Get ───────────────────────────────────────────────────────────

    public override async Task<ListVerificationsResponse> ListVerifications(
        ListVerificationsRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.Verification.Criteria
        {
            Page    = request.Pagination?.Page > 0 ? request.Pagination.Page : request.HasPage && request.Page > 0 ? request.Page : 1,
            Size    = request.Pagination?.Size > 0 ? request.Pagination.Size : request.HasSize && request.Size > 0 ? request.Size : 20,
            Type    = VerificationTypes.Verification,
            PartyId = request.HasPartyId ? request.PartyId : null,
            Status  = request.HasStatus  ? (VerificationStatusTypes?)request.Status : null,
        };

        var items = await tenantCtx.Verifications
            .PagedFilterBy(criteria)
            .ToTenantPageModel(false)
            .ToListAsync();

        var response = new ListVerificationsResponse
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
        response.Data.AddRange(items.Select(v => new ProtoVerification
        {
            Id        = v.Id,
            PartyId   = v.PartyId,
            Status    = v.Status,
            CreatedAt = v.CreatedOn?.ToString("O") ?? "",
        }));
        return response;
    }

    public override async Task<GetVerificationResponse> GetVerification(
        GetVerificationRequest request, ServerCallContext context)
    {
        var item = await tenantCtx.Verifications
            .Include(x => x.VerificationItems)
            .Include(x => x.Party)
            .SingleOrDefaultAsync(x => x.Id == request.Id);
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Verification not found"));
        return new GetVerificationResponse { Data = MapToProto(item) };
    }

    public override async Task<GetVerificationItemResponse> GetVerificationItem(
        GetVerificationItemRequest request, ServerCallContext context)
    {
        var item = await tenantCtx.VerificationItems
            .Where(x => x.VerificationId == request.Id && x.Id == request.ItemId)
            .SingleOrDefaultAsync();
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Verification item not found"));
        return new GetVerificationItemResponse { Data = MapItemToProto(item) };
    }

    public override async Task<GetVerificationItemListResponse> GetVerificationItemList(
        GetVerificationItemListRequest request, ServerCallContext context)
    {
        var items = await tenantCtx.VerificationItems
            .Where(x => x.VerificationId == request.Id)
            .OrderBy(x => x.Id)
            .ToListAsync();

        var response = new GetVerificationItemListResponse();
        response.Items.AddRange(items.Select(MapItemToProto));
        return response;
    }

    // ─── Status transitions ───────────────────────────────────────────────────

    public override async Task<SetUnderReviewResponse> SetUnderReview(
        SetUnderReviewRequest request, ServerCallContext context)
    {
        var result = await ChangeStatus(request.Id, VerificationStatusTypes.UnderReview, context);
        return new SetUnderReviewResponse { Data = result };
    }

    public override async Task<SetAwaitingApproveResponse> SetAwaitingApprove(
        SetAwaitingApproveRequest request, ServerCallContext context)
    {
        var result = await ChangeStatus(request.Id, VerificationStatusTypes.AwaitingApprove, context);
        return new SetAwaitingApproveResponse { Data = result };
    }

    public override async Task<ApproveVerificationResponse> ApproveVerification(
        ApproveVerificationRequest request, ServerCallContext context)
    {
        var result = await ApproveWithTag(request.Id, false, context);
        return new ApproveVerificationResponse { Data = result };
    }

    public override async Task<DelayedApproveResponse> DelayedApprove(
        DelayedApproveRequest request, ServerCallContext context)
    {
        var result = await ApproveWithTag(request.Id, true, context);
        return new DelayedApproveResponse { Data = result };
    }

    public override async Task<SetAwaitingReviewResponse> SetAwaitingReview(
        SetAwaitingReviewRequest request, ServerCallContext context)
    {
        var result = await ChangeStatus(request.Id, VerificationStatusTypes.AwaitingReview, context);
        return new SetAwaitingReviewResponse { Data = result };
    }

    public override async Task<RejectVerificationResponse> RejectVerification(
        RejectVerificationRequest request, ServerCallContext context)
    {
        var item = await tenantCtx.Verifications.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Verification not found"));

        await UpdateStatus(item, VerificationStatusTypes.Rejected, context);
        await mediator.Publish(new VerificationRejectedEvent(item));
        return new RejectVerificationResponse { Data = MapToProto(item) };
    }

    public override async Task<SetAwaitingAddressVerifyResponse> SetAwaitingAddressVerify(
        SetAwaitingAddressVerifyRequest request, ServerCallContext context)
    {
        var result = await ChangeStatus(request.Id, VerificationStatusTypes.AwaitingAddressVerify, context);
        return new SetAwaitingAddressVerifyResponse { Data = result };
    }

    public override async Task<SetAwaitingCodeVerifyResponse> SetAwaitingCodeVerify(
        SetAwaitingCodeVerifyRequest request, ServerCallContext context)
    {
        var result = await ChangeStatus(request.Id, VerificationStatusTypes.AwaitingCodeVerify, context);
        return new SetAwaitingCodeVerifyResponse { Data = result };
    }

    // ─── Document operations ──────────────────────────────────────────────────

    public override async Task<RejectDocumentNoticeResponse> RejectDocumentNotice(
        RejectDocumentNoticeRequest request, ServerCallContext context)
    {
        var partyId = await tenantCtx.Verifications
            .Where(x => x.Id == request.Id)
            .Select(x => x.PartyId)
            .SingleAsync();

        await mediator.Publish(new VerificationDocumentRejectedEvent(partyId));
        return new RejectDocumentNoticeResponse { Success = true };
    }

    public override async Task<RejectDocumentResponse> RejectDocument(
        RejectDocumentRequest request, ServerCallContext context)
    {
        var verificationItem = tenantCtx.VerificationItems
            .Include(x => x.Verification)
            .Where(x => x.VerificationId == request.Id)
            .Where(x => x.Verification.Type == (int)VerificationTypes.Verification)
            .FirstOrDefault(x => x.Category == VerificationCategoryTypes.Document);
        if (verificationItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Verification item not found"));

        var docs = JsonConvert.DeserializeObject<List<VerificationDocumentMedium>>(verificationItem.Content)
                   ?? new List<VerificationDocumentMedium>();
        var rejectItem = docs.SingleOrDefault(x => x.Id == request.MediumId);
        if (rejectItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Document not found"));
        if (rejectItem.Status == VerificationDocumentStatusTypes.Rejected)
            throw new RpcException(new Status(StatusCode.AlreadyExists, "Document already rejected"));

        rejectItem.RejectedOn     = DateTime.UtcNow;
        rejectItem.RejectedReason = request.Spec;
        rejectItem.Status         = VerificationDocumentStatusTypes.Rejected;

        verificationItem.Content   = Utils.JsonSerializeObject(docs);
        verificationItem.UpdatedOn = DateTime.UtcNow;
        tenantCtx.VerificationItems.Update(verificationItem);
        await tenantCtx.SaveChangesAsync();

        if (docs.CanAwaitingApprove())
            verificationItem.Verification.TryChangeToAwaitingApprove();
        else if ((int)VerificationStatusTypes.AwaitingReview == verificationItem.Verification.Status)
            verificationItem.Verification.TryChangeToUnderReview();

        tenantCtx.Verifications.Update(verificationItem.Verification);
        await tenantCtx.SaveChangesAsync();

        return new RejectDocumentResponse { Data = MapItemToProto(verificationItem) };
    }

    public override async Task<ApproveDocumentResponse> ApproveDocument(
        ApproveDocumentRequest request, ServerCallContext context)
    {
        var supplement = tenantCtx.VerificationItems
            .Include(x => x.Verification)
            .Where(x => x.VerificationId == request.Id)
            .Where(x => x.Verification.Type == (int)VerificationTypes.Verification)
            .FirstOrDefault(x => x.Category == VerificationCategoryTypes.Document);
        if (supplement == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Verification item not found"));

        var docs = VerificationDocumentMedium.FromJson(supplement.Content);
        var approveItem = docs.SingleOrDefault(x => x.Id == request.MediumId);
        if (approveItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Document not found"));
        if (approveItem.Status == VerificationDocumentStatusTypes.Approved)
            throw new RpcException(new Status(StatusCode.AlreadyExists, "Document already approved"));

        approveItem.RejectedOn = DateTime.UtcNow;
        approveItem.Status     = VerificationDocumentStatusTypes.Approved;

        supplement.Content   = Utils.JsonSerializeObject(docs);
        supplement.UpdatedOn = DateTime.UtcNow;
        tenantCtx.VerificationItems.Update(supplement);
        await tenantCtx.SaveChangesAsync();

        if (docs.CanAwaitingApprove())
            supplement.Verification.TryChangeToAwaitingApprove();
        else if ((int)VerificationStatusTypes.AwaitingReview == supplement.Verification.Status)
            supplement.Verification.TryChangeToUnderReview();

        tenantCtx.Verifications.Update(supplement.Verification);
        await tenantCtx.SaveChangesAsync();

        return new ApproveDocumentResponse { Data = MapItemToProto(supplement) };
    }

    public override async Task<DeleteDocumentResponse> DeleteDocument(
        DeleteDocumentRequest request, ServerCallContext context)
    {
        var supplement = tenantCtx.VerificationItems
            .Include(x => x.Verification)
            .Where(x => x.VerificationId == request.Id)
            .Where(x => x.Verification.Type == (int)VerificationTypes.Verification)
            .FirstOrDefault(x => x.Category == VerificationCategoryTypes.Document);
        if (supplement == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Verification item not found"));

        var docs = VerificationDocumentMedium.FromJson(supplement.Content);
        var deleteItem = docs.SingleOrDefault(x => x.Id == request.MediumId);
        if (deleteItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Document not found"));

        var medium = await tenantCtx.Media.FindAsync(request.MediumId);
        if (medium != null)
        {
            medium.DeletedOn = DateTime.UtcNow;
            tenantCtx.Media.Update(medium);
            await tenantCtx.SaveChangesAsync();
        }

        docs.Remove(deleteItem);
        supplement.Content   = Utils.JsonSerializeObject(docs);
        supplement.UpdatedOn = DateTime.UtcNow;
        tenantCtx.VerificationItems.Update(supplement);
        await tenantCtx.SaveChangesAsync();

        if (supplement.Verification.Status == (int)VerificationStatusTypes.UnderReview && docs.CanAwaitingApprove())
        {
            supplement.Verification.Status    = (int)VerificationStatusTypes.AwaitingApprove;
            supplement.Verification.UpdatedOn = DateTime.UtcNow;
            tenantCtx.Verifications.Update(supplement.Verification);
            await tenantCtx.SaveChangesAsync();
        }

        return new DeleteDocumentResponse { Success = true };
    }

    // ─── Mail code ────────────────────────────────────────────────────────────

    public override async Task<GetMailCodeResponse> GetMailCode(
        GetMailCodeRequest request, ServerCallContext context)
    {
        var verification = await tenantCtx.Verifications
            .Select(x => new { x.Id, x.PartyId })
            .SingleOrDefaultAsync(x => x.Id == request.Id);
        if (verification == null) throw new RpcException(new Status(StatusCode.NotFound, "Verification not found"));

        var item = await tenantCtx.AuthCodes
            .Where(x => x.PartyId == verification.PartyId)
            .Where(x => x.Method == (short)AuthCodeMethodTypes.PaperMail)
            .Where(x => x.MethodValue == verification.Id.ToString())
            .OrderByDescending(x => x.CreatedOn)
            .FirstOrDefaultAsync();
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "No mail code found"));

        return new GetMailCodeResponse
        {
            Code      = item.Code ?? "",
            ExpiresAt = item.ExpireOn?.ToString("O") ?? "",
        };
    }

    public override async Task<SendMailCodeResponse> SendMailCode(
        SendMailCodeRequest request, ServerCallContext context)
    {
        var verification = await tenantCtx.Verifications.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (verification == null) throw new RpcException(new Status(StatusCode.NotFound, "Verification not found"));

        var item = AuthCode.Build(verification.PartyId, AuthCode.EventLabel.PaperVerification,
            AuthCodeMethodTypes.PaperMail, verification.Id.ToString());

        tenantCtx.AuthCodes.Add(item);
        await tenantCtx.SaveChangesAsync();
        return new SendMailCodeResponse { Success = true, Message = item.Code ?? "" };
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<ProtoVerification> ChangeStatus(
        long id, VerificationStatusTypes status, ServerCallContext context)
    {
        var item = await tenantCtx.Verifications.SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Verification not found"));

        await UpdateStatus(item, status, context);
        return MapToProto(item);
    }

    private async Task<ProtoVerification> ApproveWithTag(
        long id, bool delayedReview, ServerCallContext context)
    {
        var item = await tenantCtx.Verifications
            .Include(x => x.Party)
            .Include(x => x.VerificationItems)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Verification not found"));

        if (delayedReview)
            await tagSvc.AddPartyTagAsync(item.PartyId, "DelayedReview");

        await UpdateStatus(item, VerificationStatusTypes.Approved, context);

        if (TenantId == 10005)
            return MapToProto(item);

        if (await tenantCtx.Applications.AnyAsync(x => x.PartyId == item.PartyId))
            return MapToProto(item);

        var verificationDTO = item.ToDTO();
        var accountType = (AccountTypes)(verificationDTO.Started?.AccountType ?? 0);
        var serviceId   = verificationDTO.Started?.ServiceId ?? 0;
        var platform    = (PlatformTypes)(verificationDTO.Started?.Platform ?? 0);
        var currency    = (CurrencyTypes)(verificationDTO.Started?.Currency ?? 0);

        var supplement = ApplicationSupplement.Build(
            AccountRoleTypes.Client, FundTypes.Wire,
            accountType: accountType, leverage: verificationDTO.Started?.Leverage ?? 0,
            currency: currency, platform: platform, serviceId: serviceId);
        supplement.ReferCode = verificationDTO.Started?.Referral ?? "";

        var application = new Application
        {
            PartyId    = item.PartyId,
            Type       = (short)ApplicationTypes.TradeAccount,
            Status     = (short)ApplicationStatusTypes.AwaitingApproval,
            UpdatedOn  = DateTime.UtcNow,
            CreatedOn  = DateTime.UtcNow,
            Supplement = supplement.ToJson(),
        };
        var entity = await tenantCtx.Applications.AddAsync(application);
        await tenantCtx.SaveChangesAsync();
        await mediator.Publish(new VerificationApprovedEvent(item, entity.Entity));

        return MapToProto(item);
    }

    private async Task UpdateStatus(
        Bacera.Gateway.Verification item, VerificationStatusTypes status, ServerCallContext context)
    {
        var party = await userSvc.GetPartyAsync(GetPartyId(context));
        item.Note      = Utils.JsonSerializeObject(new { @operator = party });
        item.Status    = (int)status;
        item.UpdatedOn = DateTime.UtcNow;
        tenantCtx.Verifications.Update(item);
        await tenantCtx.SaveChangesAsync();
    }

    private static ProtoVerification MapToProto(Bacera.Gateway.Verification v)
        => new ProtoVerification
        {
            Id        = v.Id,
            PartyId   = v.PartyId,
            Status    = v.Status,
            CreatedAt = v.CreatedOn.ToString("O"),
        };

    private static ProtoVerificationItem MapItemToProto(Bacera.Gateway.VerificationItem i)
        => new ProtoVerificationItem
        {
            Id             = i.Id,
            VerificationId = i.VerificationId,
            Category       = i.Category ?? "",
            Content        = i.Content  ?? "",
        };

    private static long GetPartyId(ServerCallContext ctx)
    {
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("PartyId", out var v) && v is long id ? id : 0;
    }
}
