using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.Amazon;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.BackgroundJobs.GeneralJob;
using Grpc.Core;
using Hangfire;
using Http.V1;
using Microsoft.EntityFrameworkCore;
using ProtoDebugEmailRequest = Http.V1.DebugEmailRequest;
using ProtoMessage      = Http.V1.Message;
using ProtoTopic        = Http.V1.Topic;
using ProtoTopicContent = Http.V1.TopicContent;

namespace Bacera.Gateway.Web.HttpServices.Notification;

/// <summary>
/// gRPC JSON Transcoding implementation of TenantMessageService.
/// Replaces Areas/Tenant/Controllers/MessageController.cs.
/// Routes defined via google.api.http annotations in notification.proto.
/// </summary>
public class TenantMessageGrpcService(
    TenantDbContext tenantCtx,
    ITenantGetter tenantGetter,
    ISendMessageService sendMessageSvc)
    : TenantMessageService.TenantMessageServiceBase
{
    public override async Task<ListMessagesResponse> ListMessages(
        ListMessagesRequest request, ServerCallContext context)
    {
        var page = request.Pagination?.Page > 0 ? request.Pagination.Page : 1;
        var size = request.Pagination?.Size > 0 ? request.Pagination.Size : 20;

        var criteria = new Bacera.Gateway.Message.Criteria
        {
            Page    = page,
            Size    = size,
            PartyId = request.HasAccountId ? request.AccountId : null,
            Type    = request.HasType ? (MessageTypes)request.Type : null,
        };

        if (request.HasStatus)
            criteria.IsRead = request.Status == 1;

        var items = await tenantCtx.Messages
            .PagedFilterBy(criteria)
            .ToListAsync();

        var response = new ListMessagesResponse
        {
            Criteria = new PaginationMeta
            {
                Page      = page,
                Size      = size,
                Total     = criteria.Total,
                PageCount = criteria.Total > 0 ? (int)Math.Ceiling((double)criteria.Total / size) : 0,
                HasMore   = page * size < criteria.Total,
            }
        };
        response.Data.AddRange(items.Select(MapToProto));
        return response;
    }

    public override async Task<GetMessageResponse> GetMessage(
        GetMessageRequest request, ServerCallContext context)
    {
        var item = await tenantCtx.Messages.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Message not found"));
        return new GetMessageResponse { Data = MapToProto(item) };
    }

    public override async Task<CreateMessageResponse> CreateMessage(
        CreateMessageRequest request, ServerCallContext context)
    {
        var item = new Bacera.Gateway.Message
        {
            PartyId   = request.AccountId,
            Title     = request.Title,
            Content   = request.Body,
            Type      = request.Type,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };
        await tenantCtx.Messages.AddAsync(item);
        await tenantCtx.SaveChangesAsync();
        return new CreateMessageResponse { Data = MapToProto(item) };
    }

    public override async Task<UpdateMessageResponse> UpdateMessage(
        UpdateMessageRequest request, ServerCallContext context)
    {
        var item = await tenantCtx.Messages.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Message not found"));

        var spec = request.Spec;
        item.PartyId    = spec.AccountId;
        item.Title      = spec.Title;
        item.Content    = spec.Body;
        item.Type       = spec.Type;
        item.UpdatedOn  = DateTime.UtcNow;

        tenantCtx.Messages.Update(item);
        await tenantCtx.SaveChangesAsync();
        return new UpdateMessageResponse { Data = MapToProto(item) };
    }

    public override async Task<DeleteMessageResponse> DeleteMessage(
        DeleteMessageRequest request, ServerCallContext context)
    {
        var item = await tenantCtx.Messages.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "Message not found"));

        tenantCtx.Messages.Remove(item);
        await tenantCtx.SaveChangesAsync();
        return new DeleteMessageResponse { Success = true };
    }

    public override async Task<SendPopupMessageResponse> SendPopupMessage(
        SendPopupMessageRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var dto = MessagePopupDTO.BuildInfo(request.Title, request.Body);
        await sendMessageSvc.SendPopupToPartyAsync(tenantId, request.AccountId, dto);
        return new SendPopupMessageResponse { Success = true };
    }

    private static ProtoMessage MapToProto(Bacera.Gateway.Message m) => new ProtoMessage
    {
        Id        = m.Id,
        AccountId = m.PartyId,
        Title     = m.Title,
        Body      = m.Content,
        Type      = m.Type,
        Status    = m.ReadOn != null ? 1 : 0,
        CreatedAt = m.CreatedOn.ToString("O"),
    };
}

/// <summary>
/// gRPC JSON Transcoding implementation of TenantEmailService.
/// Replaces Areas/Tenant/Controllers/EmailController.cs.
/// Routes defined via google.api.http annotations in notification.proto.
/// </summary>
public class TenantEmailGrpcService(
    TenantDbContext tenantCtx,
    ITenantGetter tenantGetter,
    IBackgroundJobClient backgroundJobClient,
    IServiceProvider serviceProvider,
    BatchSendEmailService batchSendEmailSvc,
    ConfigService cfgSvc,
    AwsEmailClientV2 awsEmailClientV2,
    ISendMailService sendMailService)
    : TenantEmailService.TenantEmailServiceBase
{
    public override async Task<DebugEmailResponse> DebugEmail(
        ProtoDebugEmailRequest request, ServerCallContext context)
    {
        var domainReq = new Bacera.Gateway.DebugEmailRequest
        {
            To    = request.To,
            Title = request.Subject,
        };
        var (ok, msg) = await sendMailService.DebugAsync(domainReq);
        return new DebugEmailResponse { Success = ok, Message = msg ?? "" };
    }

    public override async Task<SendToUserResponse> SendToUser(
        SendToUserRequest request, ServerCallContext context)
    {
        var domainReq = new SendToPartyRequest
        {
            PartyId = request.PartyId,
            Title   = request.Subject,
            Content = request.Body,
        };
        var (ok, msg) = await batchSendEmailSvc.SendEmailToPartyAsync(domainReq);
        return new SendToUserResponse { Success = ok, Message = msg ?? "" };
    }

    public override async Task<CreateBatchResponse> CreateBatch(
        CreateBatchRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var topicExists = await tenantCtx.Topics.AnyAsync(x => x.Id == request.TopicContentId);
        if (!topicExists) throw new RpcException(new Status(StatusCode.NotFound, "Topic not found"));

        var spec = new CreateSendTopicContentSpec { TopicId = (long)request.TopicContentId };
        var info = await batchSendEmailSvc.InitSendBatchEmailInfoAsync(spec, partyId);

        return new CreateBatchResponse
        {
            Data = new EmailBatch
            {
                Id        = info.TopicId,
                Subject   = info.TopicKey,
                Status    = info.Status switch
                {
                    "pending"   => 1,
                    "sending"   => 2,
                    "completed" => 3,
                    "failed"    => 4,
                    _           => 0,
                },
                Total     = (int)info.Total,
                CreatedAt = DateTime.UtcNow.ToString("O"),
            },
        };
    }

    public override async Task<GetBatchInfoResponse> GetBatchInfo(
        GetBatchInfoRequest request, ServerCallContext context)
    {
        var info = await batchSendEmailSvc.GetRealTimeInfoAsync();
        return new GetBatchInfoResponse { RecipientCount = (int)(info?.Total ?? 0) };
    }

    public override async Task<GetBatchDetailResponse> GetBatchDetail(
        GetBatchDetailRequest request, ServerCallContext context)
    {
        var items = await batchSendEmailSvc.GetBatchEmailDetail();
        var response = new GetBatchDetailResponse();
        response.Emails.AddRange(items.Select(x => x.Email));
        return response;
    }

    public override async Task<TestBatchResponse> TestBatch(
        TestBatchRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var info = await cfgSvc.GetAsync<SendBatchEmailInfo>(
            nameof(Bacera.Gateway.Core.Types.Public), 0, ConfigKeys.SendBatchEmailSpecKey);
        if (info == null) throw new RpcException(new Status(StatusCode.NotFound, "No batch email info found"));

        var languages = info.Contents.Keys.ToList();
        var dtos = languages
            .Select(lang => SendBatchEmailDTO.Build(tenantId, 0, request.TestEmail, lang, info.TopicId, info.TopicKey))
            .ToList();

        _ = Task.Run(async () =>
        {
            using var scope = serviceProvider.CreateTenantScope(tenantId);
            var batchEmailSvc = scope.ServiceProvider.GetRequiredService<BatchSendEmailService>();
            foreach (var dto in dtos)
                await batchEmailSvc.SendEmailByTopicIdWithContent(dto, true);
        });

        return new TestBatchResponse { Success = true };
    }

    public override async Task<ConfirmBatchResponse> ConfirmBatch(
        ConfirmBatchRequest request, ServerCallContext context)
    {
        var tenantId = tenantGetter.GetTenantId();
        var info = await batchSendEmailSvc.GetRealTimeInfoAsync();
        if (info == null) throw new RpcException(new Status(StatusCode.NotFound, "No batch email info found"));

        backgroundJobClient.Enqueue<IGeneralJob>(x => x.SendEmailByTopicIdWithContent(tenantId, info.Uuid));
        return new ConfirmBatchResponse { Success = true };
    }

    public override async Task<CheckSuppressionResponse> CheckSuppression(
        CheckSuppressionRequest request, ServerCallContext context)
    {
        var result = await awsEmailClientV2.EmailInSuppressedDestinationAsync(request.Email);
        return new CheckSuppressionResponse { Suppressed = result, Reason = "" };
    }

    public override async Task<RemoveSuppressionResponse> RemoveSuppression(
        RemoveSuppressionRequest request, ServerCallContext context)
    {
        var result = await awsEmailClientV2.DeleteEmailFromSuppressedDestinationAsync(request.Email);
        return new RemoveSuppressionResponse { Success = result };
    }

    public override async Task<AddSuppressionResponse> AddSuppression(
        AddSuppressionRequest request, ServerCallContext context)
    {
        var result = await awsEmailClientV2.PutEmailInSuppressedDestinationAsync(request.Email);
        return new AddSuppressionResponse { Success = result };
    }

    public override async Task<ListEmailTemplatesResponse> ListEmailTemplates(
        ListEmailTemplatesRequest request, ServerCallContext context)
    {
        var page = request.Pagination?.Page > 0 ? request.Pagination.Page : 1;
        var size = request.Pagination?.Size > 0 ? request.Pagination.Size : 20;

        var query = tenantCtx.Topics
            .Where(x => x.Type == (int)TopicTypes.EmailTemplate);

        if (request.HasKeywords)
            query = query.Where(x => x.Title.Contains(request.Keywords));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var response = new ListEmailTemplatesResponse
        {
            Criteria = new PaginationMeta
            {
                Page      = page,
                Size      = size,
                Total     = total,
                PageCount = total > 0 ? (int)Math.Ceiling((double)total / size) : 0,
                HasMore   = page * size < total,
            }
        };
        response.Data.AddRange(items.Select(t => new ProtoTopic
        {
            Id        = t.Id,
            Title     = t.Title ?? "",
            Type      = t.Type,
            CreatedAt = t.CreatedOn.ToString("O"),
        }));
        return response;
    }

    public override async Task<GetBatchReceiverEmailsResponse> GetBatchReceiverEmails(
        GetBatchReceiverEmailsRequest request, ServerCallContext context)
    {
        var spec = Newtonsoft.Json.JsonConvert.DeserializeObject<CreateSendTopicContentSpec>(request.Spec)
                   ?? new CreateSendTopicContentSpec();
        var info  = spec.ToSendBatchEmailInfo();
        var query = batchSendEmailSvc.GetSendBatchEmailQuery(info);

        var emails = await query
            .OrderBy(x => x.UserId)
            .Skip(request.Page * 100)
            .Take(100)
            .Select(x => x.Email)
            .ToListAsync();

        var total = await query.CountAsync();

        var response = new GetBatchReceiverEmailsResponse { Total = total, Page = request.Page };
        response.Emails.AddRange(emails);
        return response;
    }

    private static long GetPartyId(ServerCallContext ctx)
        => ctx.GetHttpContext().User.GetPartyId();
}

/// <summary>
/// gRPC JSON Transcoding implementation of TenantTopicService.
/// Replaces Areas/Tenant/Controllers/TopicController.cs.
/// Routes defined via google.api.http annotations in notification.proto.
/// </summary>
public class TenantTopicGrpcService(ITopicService topicSvc)
    : TenantTopicService.TenantTopicServiceBase
{
    public override async Task<ListTopicsResponse> ListTopics(
        ListTopicsRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.Topic.Criteria
        {
            Page  = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size  = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            Type  = request.HasType ? (TopicTypes)request.Type : null,
            Title = request.HasKeywords ? request.Keywords : null,
        };

        var result = await topicSvc.QueryAsync(criteria);

        var response = new ListTopicsResponse
        {
            Criteria = new PaginationMeta
            {
                Page      = result.Criteria.Page,
                Size      = result.Criteria.Size,
                Total     = result.Criteria.Total,
                PageCount = result.Criteria.Total > 0
                    ? (int)Math.Ceiling((double)result.Criteria.Total / result.Criteria.Size)
                    : 0,
                HasMore   = result.Criteria.Page * result.Criteria.Size < result.Criteria.Total,
            }
        };
        response.Data.AddRange(result.Data.Select(MapToProto));
        return response;
    }

    public override async Task<GetTopicResponse> GetTopic(
        GetTopicRequest request, ServerCallContext context)
    {
        Bacera.Gateway.Topic item;
        if (request.HasLanguage && !string.IsNullOrEmpty(request.Language))
            item = await topicSvc.GetWithLanguageAsync(request.Id, request.Language);
        else
            item = await topicSvc.GetAsync(request.Id);

        if (item.Id == 0) throw new RpcException(new Status(StatusCode.NotFound, "Topic not found"));
        return new GetTopicResponse { Data = MapToProto(item) };
    }

    public override async Task<GetTopicLanguagesResponse> GetTopicLanguages(
        GetTopicLanguagesRequest request, ServerCallContext context)
    {
        if (!await topicSvc.ExistsAsync(request.Id))
            throw new RpcException(new Status(StatusCode.NotFound, "Topic not found"));

        var languages = await topicSvc.GetLanguagesAsync(request.Id);
        var response = new GetTopicLanguagesResponse();
        response.Languages.AddRange(languages);
        return response;
    }

    public override async Task<CreateTopicResponse> CreateTopic(
        CreateTopicRequest request, ServerCallContext context)
    {
        var spec = new Bacera.Gateway.Topic.CreateSpec
        {
            Title    = request.Title,
            Type     = (TopicTypes)request.Type,
            Language = LanguageTypes.English,
            Content  = request.Title,
        };
        var item = await topicSvc.CreateAsync(spec);
        if (item.Id == 0) throw new RpcException(new Status(StatusCode.Internal, "Failed to create topic"));
        return new CreateTopicResponse { Data = MapToProto(item) };
    }

    public override async Task<UpdateTopicResponse> UpdateTopic(
        UpdateTopicRequest request, ServerCallContext context)
    {
        var spec = new Bacera.Gateway.Topic.UpdateSpec
        {
            Title = request.Spec.Title,
            Type  = (TopicTypes)request.Spec.Type,
        };
        var item = await topicSvc.UpdateAsync(request.Id, spec);
        if (item.Id == 0) throw new RpcException(new Status(StatusCode.NotFound, "Topic not found"));
        return new UpdateTopicResponse { Data = MapToProto(item) };
    }

    public override async Task<MoveToTrashResponse> MoveToTrash(
        MoveToTrashRequest request, ServerCallContext context)
    {
        var item = await topicSvc.GetAsync(request.Id);
        if (item.Id == 0) throw new RpcException(new Status(StatusCode.NotFound, "Topic not found"));
        await topicSvc.MoveToTrash(request.Id);
        item = await topicSvc.GetAsync(request.Id);
        return new MoveToTrashResponse { Data = MapToProto(item) };
    }

    public override async Task<DeleteTopicResponse> DeleteTopic(
        DeleteTopicRequest request, ServerCallContext context)
    {
        if (!await topicSvc.ExistsAsync(request.Id))
            throw new RpcException(new Status(StatusCode.NotFound, "Topic not found"));
        await topicSvc.DeleteAsync(request.Id);
        return new DeleteTopicResponse { Success = true };
    }

    public override async Task<CreateTopicContentResponse> CreateTopicContent(
        CreateTopicContentRequest request, ServerCallContext context)
    {
        var spec = new TopicContent.Spec
        {
            Language = request.Language,
            Content  = request.Content,
            Title    = "",
        };
        var item = await topicSvc.CreateContentAsync(request.Id, spec);
        if (item.Id == 0) throw new RpcException(new Status(StatusCode.NotFound, "Topic not found"));
        return new CreateTopicContentResponse { Data = MapContentToProto(item) };
    }

    public override async Task<UpdateTopicContentResponse> UpdateTopicContent(
        UpdateTopicContentRequest request, ServerCallContext context)
    {
        var spec = new TopicContent.Spec
        {
            Language = request.Spec.Language,
            Content  = request.Spec.Content,
            Title    = "",
        };
        var item = await topicSvc.UpdateContentAsync(request.ContentId, spec);
        if (item.Id == 0) throw new RpcException(new Status(StatusCode.NotFound, "TopicContent not found"));
        return new UpdateTopicContentResponse { Data = MapContentToProto(item) };
    }

    public override async Task<DeleteTopicContentResponse> DeleteTopicContent(
        DeleteTopicContentRequest request, ServerCallContext context)
    {
        if (!await topicSvc.ExistsAsync(request.Id))
            throw new RpcException(new Status(StatusCode.NotFound, "Topic not found"));
        await topicSvc.DeleteContentAsync(request.ContentId);
        return new DeleteTopicContentResponse { Success = true };
    }

    private static ProtoTopic MapToProto(Bacera.Gateway.Topic t) => new ProtoTopic
    {
        Id        = t.Id,
        Title     = t.Title,
        Type      = t.Type,
        Status    = t.EffectiveTo < DateTime.UtcNow ? 1 : 0,
        CreatedAt = t.CreatedOn.ToString("O"),
    };

    private static ProtoTopic MapToProto(Bacera.Gateway.Topic.ResponseModel r) => new ProtoTopic
    {
        Id        = r.Id,
        Title     = r.Title,
        Type      = r.Type,
        Status    = r.EffectiveTo < DateTime.UtcNow ? 1 : 0,
        CreatedAt = r.CreatedOn.ToString("O"),
    };

    private static ProtoTopicContent MapContentToProto(TopicContent c) => new ProtoTopicContent
    {
        Id       = c.Id,
        TopicId  = c.TopicId,
        Language = c.Language,
        Content  = c.Content,
    };
}
