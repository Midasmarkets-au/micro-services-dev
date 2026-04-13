using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.Amazon;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.BackgroundJobs.GeneralJob;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Bacera.Gateway.Topic;

[Tags("Tenant/Email")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class EmailController(
    TenantDbContext tenantCtx,
    IBackgroundJobClient client,
    IServiceProvider serviceProvider,
    BatchSendEmailService batchSendEmailSvc,
    ConfigService cfgSvc,
    AwsEmailClientV2 awsEmailClientV2,
    ISendMailService sendMailService)
    : TenantBaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria = null)
    {
        criteria ??= new M.Criteria();
        criteria.Type = TopicTypes.EmailTemplate;
        var items = await tenantCtx.Topics
            .PagedFilterBy<M, int>(criteria)
            .ToListAsync();
        return Ok(Result.Of(items, criteria));
    }

    [HttpPost("debug")]
    public async Task<IActionResult> Send([FromBody] DebugEmailRequest request)
    {
        var result = await sendMailService.DebugAsync(request);
        if (result.Item1 == false) return BadRequest(result.Item2);
        return Ok(result);
    }

    [HttpPost("send-to-user")]
    public async Task<IActionResult> SendToUser([FromBody] SendToPartyRequest request)
    {
        var (res, msg) = await batchSendEmailSvc.SendEmailToPartyAsync(request);
        return res ? Ok(msg) : BadRequest(msg);
    }

    [HttpPost("batch")]
    public async Task<IActionResult> SendTopic([FromBody] CreateSendTopicContentSpec spec)
    {
        var partyId = GetPartyId();
        var topicExists = await tenantCtx.Topics.AnyAsync(x => x.Id == spec.TopicId);
        if (!topicExists) return NotFound("Topic not found");

        var info = await batchSendEmailSvc.InitSendBatchEmailInfoAsync(spec, partyId);
        return Ok(new { uuid = info.Uuid, total = info.Total, topicKey = info.TopicKey });
    }

    [HttpGet("batch/info")]
    public async Task<IActionResult> GetRealTimeInfo()
    {
        var info = await batchSendEmailSvc.GetRealTimeInfoAsync();
        return info != null ? Ok(info) : NotFound("No batch email info found");
    }

    [HttpGet("batch/detail")]
    public async Task<IActionResult> GetBatchEmailDetail()
    {
        var result = await batchSendEmailSvc.GetBatchEmailDetail();
        return Ok(result);
    }

    [HttpGet("batch/receiver-emails")]
    public async Task<IActionResult> ReceiverEmails([FromBody] CreateSendTopicContentSpec spec, [FromQuery] int page)
    {
        var info = spec.ToSendBatchEmailInfo();
        var query = batchSendEmailSvc.GetSendBatchEmailQuery(info);
        var emails = await query
            .OrderBy(x => x.UserId)
            .Skip(page * 100)
            .Take(100)
            .Select(x => x.Email)
            .ToListAsync();

        var total = await query.CountAsync();
        return Ok(new { emails, total, page });
    }

    [HttpPost("batch/test")]
    public async Task<IActionResult> TestSendTopic([FromBody] SendBatchEmailTestSpec spec)
    {
        var info = await cfgSvc.GetAsync<SendBatchEmailInfo>(nameof(Public), 0, ConfigKeys.SendBatchEmailSpecKey);
        if (info == null) return NotFound("No batch email info found");
        if (info.Uuid != spec.Uuid) return BadRequest("Invalid uuid");

        var tenantId = GetTenantId();
        var languages = info.Contents.Keys.ToList();
        var dtos = spec.TestEmails
            .SelectMany(email =>
                languages.Select(language =>
                    SendBatchEmailDTO.Build(tenantId, 0, email, language, info.TopicId, info.TopicKey)))
            .ToList();

        _ = Task.Run(async () =>
        {
            using var scope = serviceProvider.CreateTenantScope(tenantId);
            var batchEmailSvc = scope.ServiceProvider.GetRequiredService<BatchSendEmailService>();
            foreach (var dto in dtos)
                await batchEmailSvc.SendEmailByTopicIdWithContent(dto, true);
        });

        return Ok();
    }

    [HttpPost("batch/confirm")]
    public async Task<IActionResult> ConfirmSendTopic([FromBody] SendBatchEmailConfirmSpec spec)
    {
        var info = await batchSendEmailSvc.GetRealTimeInfoAsync();
        if (info == null) return NotFound("No batch email info found");
        if (info.Uuid != spec.Uuid) return BadRequest("Invalid uuid");
        if (info.Total != spec.Total) return BadRequest("Invalid total");

        var tenantId = GetTenantId();
        client.Enqueue<IGeneralJob>(x => x.SendEmailByTopicIdWithContent(tenantId, spec.Uuid));
        return Ok();
    }

    [HttpGet("suppression-check")]
    public async Task<IActionResult> SuppressionCheck([FromQuery] string email)
    {
        var result = await awsEmailClientV2.EmailInSuppressedDestinationAsync(email);
        return Ok(result);
    }

    [HttpPut("suppression-remove")]
    public async Task<IActionResult> SuppressionRemove([FromQuery] string email)
    {
        var result = await awsEmailClientV2.DeleteEmailFromSuppressedDestinationAsync(email);
        return Ok(result);
    }

    [HttpPut("suppression-add")]
    public async Task<IActionResult> SuppressionAdd([FromQuery] string email)
    {
        var result = await awsEmailClientV2.PutEmailInSuppressedDestinationAsync(email);
        return Ok(result);
    }
}
