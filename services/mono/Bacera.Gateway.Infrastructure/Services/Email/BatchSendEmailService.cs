using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services;

public class BatchSendEmailService(
    TenantDbContext tenantCtx,
    AuthDbContext authCtx,
    IMyCache cache,
    ILogger<BatchSendEmailService> logger,
    MyDbContextPool pool,
    IEmailSender emailSender,
    IServiceProvider provider,
    ITenantGetter tenantGetter,
    ConfigService cfgSvc)
{
    private readonly long _tenantId = tenantGetter.GetTenantId();
    private readonly string _contentTenantHKey = CacheKeys.GetSendBatchEmailContentHKey(tenantGetter.GetTenantId());
    private readonly string _sendEmailsTenantHKey = CacheKeys.GetSendBatchEmailsHKey(tenantGetter.GetTenantId());
    private readonly string _sendEmailFailedHKey = CacheKeys.GetSendBatchEmailFailedHKey(tenantGetter.GetTenantId());
    private const int MaxTryTime = 3;

    public async Task<(bool, string)> SendEmailByTopicIdWithContent(SendBatchEmailDTO dto, bool isTest = false)
    {
        var hasSent = await EmailHasSentAsync(dto.TopicKey, dto.Email);
        var noPromotion = await EmailNoPromotionAsync(dto.Email);
        if (noPromotion) await cache.HIncrementAsync(_sendEmailsTenantHKey, nameof(SendBatchEmailInfo.NoPromotion));

        if (isTest || (!hasSent && !noPromotion))
        {
            var senderEmail = await GetSenderEmailAsync();
            var senderDisplayName = await GetSenderDisplayNameAsync();
            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderDisplayName))
                return (false, "Sender email or display name is empty");

            var topicContent = await GetTopicContentAsync(dto.TopicId, dto.Language);
            if (topicContent == null) return (false, "No template found");

            var content = await GetContentAsync(dto.Language);
            if (content == null) return (false, "No content found");

            var appliedTemplate = topicContent.Content
                .Replace("{{Title}}", content.Title)
                .Replace("{{Subtitle}}", content.SubTitle)
                .Replace("{{Content}}", content.Content);

            if (string.IsNullOrWhiteSpace(appliedTemplate)) return (false, "Template is empty");

            try
            {
                var (result, msg) = await emailSender.SendEmailAsync(dto.Email
                    , content.Title
                    , appliedTemplate
                    , senderEmail
                    , senderDisplayName);

                if (isTest) return (result, msg);
                if (!result) throw new Exception(msg);

                await cache.AddToSetAsync(CacheKeys.GetSendBatchEmailHasSentHKey(dto.TopicKey), dto.Email,
                    TimeSpan.FromDays(2));
            }
            catch (Exception e)
            {
                var tryTime = await GetEmailTryTime(dto.Email);
                logger.LogError(e, "SendEmailByTopicIdWithContent_Email_failed: {dto}", dto);
                if (tryTime >= MaxTryTime)
                {
                    logger.LogError("SendEmailByTopicIdWithContent_Email_TryTimeExceed: {dto}", dto);
                    await cache.HIncrementAsync(_sendEmailsTenantHKey, nameof(EmailBatchStatInfo.Failed));
                }
                else
                {
                    await IncrementEmailTryTime(dto.Email);
                    return (false, e.Message);
                }
            }
        }

        await cache.HIncrementAsync(_sendEmailsTenantHKey, nameof(EmailBatchStatInfo.Sent));
        var sent = await cache.HGetStringAsync(_sendEmailsTenantHKey, nameof(EmailBatchStatInfo.Sent));
        var total = await cache.HGetStringAsync(_sendEmailsTenantHKey, nameof(EmailBatchStatInfo.Total));
        var failed = await cache.HGetStringAsync(_sendEmailsTenantHKey, nameof(EmailBatchStatInfo.Failed));
        if (sent + failed == total)
        {
            await UpdateRealTimeStatusAsync("Job Finished");
            await PersistDataAsync();
            await ClearCache(dto.TopicId);
            BcrLog.Slack($"SendEmailByTopicIdWithContent_Email_AllEmailsSent: {dto.ToJson()}");
        }
        logger.LogInformation("SendEmailByTopicIdWithContent_Email_Email sent to {Email}", dto.Email);
        return (true, "Email sent");
    }

    public async Task<(bool, string)> SendEmailToPartyAsync(SendToPartyRequest request)
    {
        var userEmail = await tenantCtx.Parties
            .Where(x => x.Id == request.PartyId)
            .Select(x => x.Email)
            .SingleOrDefaultAsync();
        if (userEmail == null) return (false, "No email found");

        var senderEmail = await GetSenderEmailAsync();
        var senderDisplayName = await GetSenderDisplayNameAsync();
        if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderDisplayName))
            return (false, "Sender email or display name is empty");

        var topicContent = await GetTopicContentAsync(request.TopicId, request.Language);
        if (topicContent == null) return (false, "No template found");

        var appliedTemplate = topicContent.Content
            .Replace("{{Title}}", request.Title)
            .Replace("{{Subtitle}}", request.SubTitle)
            .Replace("{{Content}}", request.Content);

        if (string.IsNullOrWhiteSpace(appliedTemplate)) return (false, "Template is empty");

        try
        {
            var (result, msg) = await emailSender.SendEmailAsync([userEmail]
                , request.Title
                , appliedTemplate
                , senderEmail
                , senderDisplayName
                , request.BCC
                , request.CC);

            if (!result) throw new Exception(msg);
            return (true, msg);
        }
        catch (Exception e)
        {
            logger.LogError(e, "SendEmailToParty_Email_failed: {topicId}, {partyId}", request.TopicId, request.PartyId);
            return (false, e.Message);
        }
    }

    private async Task<TopicContent?> GetTopicContentAsync(long topicId, string language)
    {
        var hkey = GetTopicContentHKey(_tenantId, topicId);
        var templateJson = await cache.HGetStringAsync(hkey, language);
        if (templateJson != null && TopicContent.TryParse(templateJson, out var template))
            return template;

        var items = await tenantCtx.TopicContents
            .Where(x => x.TopicId == topicId)
            .Where(x => x.Language == language || x.Language == LanguageTypes.English)
            .ToListAsync();

        var item = items.FirstOrDefault(x => x.Language == language)
                   ?? items.FirstOrDefault(x => x.Language == LanguageTypes.English);

        if (item == null) return null;

        await cache.HSetStringAsync(hkey, language, JsonConvert.SerializeObject(item), TimeSpan.FromDays(2));
        return item;
    }

    private async Task<SendLanguageSpec?> GetContentAsync(string language)
    {
        var content = await cache.HGetStringAsync(_contentTenantHKey, language);
        if (content != null) return JsonConvert.DeserializeObject<SendLanguageSpec>(content);

        var info = await cfgSvc.GetAsync<SendBatchEmailInfo>(nameof(Public), 0, ConfigKeys.SendBatchEmailSpecKey);
        if (info == null) return null;

        var contents = info.Contents;

        foreach (var (lang, cont) in contents)
        {
            await cache.HSetStringAsync(_contentTenantHKey, lang, cont.ToJson(), TimeSpan.FromDays(2));
        }

        if (contents.TryGetValue(language, out var value))
            return value;

        if (contents.TryGetValue(LanguageTypes.English, out var engContent))
        {
            await cache.HSetStringAsync(_contentTenantHKey, language, engContent.ToJson(), TimeSpan.FromDays(2));
            return engContent;
        }

        return null;
    }

    public async Task<SendBatchEmailInfo> InitSendBatchEmailInfoAsync(CreateSendTopicContentSpec spec, long partyId = 1)
    {
        await cache.KeyDeleteAsync(_contentTenantHKey);
        await cache.KeyDeleteAsync(GetTopicContentHKey(_tenantId, spec.TopicId));
        
        var info = spec.ToSendBatchEmailInfo();

        var total = await GetSendBatchEmailQuery(info).CountAsync();
        info.Total = total;
        info.Uuid = Guid.NewGuid().ToString();
        if (string.IsNullOrEmpty(info.TopicKey))
            info.TopicKey = info.Uuid;
        
        info.Status = "Job Created";
        await cfgSvc.SetAsync(nameof(Public), 0, ConfigKeys.SendBatchEmailSpecKey, info, partyId);

        await cache.HSetStringAsync(_sendEmailsTenantHKey, nameof(SendBatchEmailInfo.Uuid), info.Uuid);
        await cache.HSetStringAsync(_sendEmailsTenantHKey, nameof(SendBatchEmailInfo.TopicKey), info.TopicKey);
        await cache.HSetStringAsync(_sendEmailsTenantHKey, nameof(SendBatchEmailInfo.Total), total.ToString());
        await cache.HSetStringAsync(_sendEmailsTenantHKey, nameof(SendBatchEmailInfo.Status), info.Status);
        await cache.HSetStringAsync(_sendEmailsTenantHKey, nameof(SendBatchEmailInfo.NoPromotion), "0");
        await cache.HSetStringAsync(_sendEmailsTenantHKey, nameof(SendBatchEmailInfo.Sent), "0");
        await cache.HSetStringAsync(_sendEmailsTenantHKey, nameof(SendBatchEmailInfo.Failed), "0");

        return info;
    }

    public async Task<SendBatchEmailInfo?> GetRealTimeInfoAsync()
    {
        var info = await cfgSvc.GetAsync<SendBatchEmailInfo>(nameof(Public), 0, ConfigKeys.SendBatchEmailSpecKey);
        if (info == null) return null;

        var emailsKey = CacheKeys.GetSendBatchEmailsHKey(_tenantId);
        var failedHKey = CacheKeys.GetSendBatchEmailFailedHKey(_tenantId);
        var totalRaw = await cache.HGetStringAsync(emailsKey, nameof(SendBatchEmailInfo.Total));
        if (totalRaw == null) return info;
        
        var sentRaw = await cache.HGetStringAsync(emailsKey, nameof(SendBatchEmailInfo.Sent));
        var failedRaw = await cache.HGetStringAsync(emailsKey, nameof(SendBatchEmailInfo.Failed));
        var statusRaw = await cache.HGetStringAsync(emailsKey, nameof(SendBatchEmailInfo.Status));
        var noPromotionRaw = await cache.HGetStringAsync(emailsKey, nameof(SendBatchEmailInfo.NoPromotion));
        info.Total = long.TryParse(totalRaw, out var total) ? total : -1;
        info.Sent = long.TryParse(sentRaw, out var sent) ? sent : -1;
        info.Failed = long.TryParse(failedRaw, out var failed) ? failed : -1;
        info.NoPromotion = long.TryParse(noPromotionRaw, out var noPromotion) ? noPromotion : -1;
        info.Status = statusRaw ?? "Unknown";

        var db = cache.GetDatabase();

        await foreach (var entry in db.HashScanAsync(emailsKey))
        {
            info.RKeys.Add([entry.Name!, entry.Value!]);
        }

        await foreach (var entry in db.HashScanAsync(failedHKey))
        {
            info.FailedEmails.Add([entry.Name!, entry.Value!]);
        }

        // Read succeed emails from Set
        var succeedKey = CacheKeys.GetSendBatchEmailHasSentHKey(info.TopicKey);
        var succeedEmails = await cache.GetAllMembersAsync(succeedKey);
        info.SucceedEmails = succeedEmails.ToList();

        return info;
    }

    private string? _senderEmail;
    private string? _senderDisplayName;

    private async Task<string?> GetSenderEmailAsync()
    {
        if (_senderEmail != null) return _senderEmail;
        var value = await cfgSvc.GetAsync<ApplicationConfigure.StringValue>(nameof(Public), 0,
            ConfigKeys.DefaultEmailAddress);
        _senderEmail = value!.Value;
        return _senderEmail;
    }

    private async Task<string?> GetSenderDisplayNameAsync()
    {
        if (_senderDisplayName != null) return _senderDisplayName;
        var value = await cfgSvc.GetAsync<ApplicationConfigure.StringValue>(nameof(Public), 0,
            ConfigKeys.DefaultEmailDisplayName);
        _senderDisplayName = value!.Value;
        return _senderDisplayName;
    }

    private async Task<int> GetEmailTryTime(string email)
    {
        var timeStr = await cache.HGetStringAsync(_sendEmailFailedHKey, email);
        if (timeStr != null) return int.Parse(timeStr);

        await cache.HSetStringAsync(_sendEmailFailedHKey, email, "0", TimeSpan.FromDays(2));
        return 0;
    }

    private async Task IncrementEmailTryTime(string email)
    {
        var timeStr = await cache.HGetStringAsync(_sendEmailFailedHKey, email);
        if (timeStr == null) await cache.HSetStringAsync(_sendEmailFailedHKey, email, "0", TimeSpan.FromDays(2));
        await cache.HIncrementAsync(_sendEmailFailedHKey, email);
    }

    private static string GetTopicContentHKey(long tenantId, long topicId) =>
        $"BatchSendEmailService.GetTemplateRequest_TenantId{tenantId}_TopicId{topicId}";


    public async Task UpdateRealTimeStatusAsync(string status)
        => await cache.HSetStringAsync(_sendEmailsTenantHKey, nameof(SendBatchEmailInfo.Status), status);

    public IQueryable<UserSendBatchEmailQueryBasic> GetSendBatchEmailQuery(SendBatchEmailInfo info)
        => tenantCtx.Parties
            .Where(x => x.Status == 0)
            .Where(x => info.SiteId == null || x.SiteId == (int)info.SiteId)
            .Where(x => info.ReceiverEmails == null || info.ReceiverEmails.Count == 0 ||
                        info.ReceiverEmails.Contains(x.Email!))
            .Select(x => new UserSendBatchEmailQueryBasic
            {
                Email = x.Email,
                Language = x.Language,
                UserId = x.Id,
            });

    private Task<bool> EmailNoPromotionAsync(string email)
        => cache.IsMemberOfSetAsync(NoPromotionEmailCacheKey, email);

    private Task<bool> EmailHasSentAsync(string topicKey, string email)
        => cache.IsMemberOfSetAsync(CacheKeys.GetSendBatchEmailHasSentHKey(topicKey), email);

    public Task WarmNoPromotionEmailCache() => Task.WhenAll(pool.GetTenantIds().Select(async tenantId =>
    {
        using var scope = provider.CreateTenantScope(tenantId);
        var ctx = scope.ServiceProvider.GetTenantDbContext();
        var query = ctx.Parties
            .Where(x => x.PartyTags.Any(t => t.Tag.Name == "NoPromotionEmail"))
            .Select(x => x.Email)
            .Distinct()
            .OrderBy(x => x);

        const int size = 100;
        var page = 0;

        while (true)
        {
            var emails = await query.Skip(page * size).Take(size).ToListAsync();

            foreach (var email in emails)
            {
                await cache.AddToSetAsync(NoPromotionEmailCacheKey, email, TimeSpan.FromDays(1));
            }

            if (emails.Count < size)
                break;

            page += 1;
        }

        await Task.Delay(0);
    }));

    public Task DeletePromotionEmailCacheKey() => cache.KeyDeleteAsync(NoPromotionEmailCacheKey);

    private const string NoPromotionEmailCacheKey = "GeneralJob_NoPromotionEmailCacheKey";

    private async Task PersistDataAsync()
    {
        var info = await GetRealTimeInfoAsync();
        if (info == null) return;

        var persistedInfo = await cfgSvc.GetAsync<SendBatchEmailInfo>(nameof(Public)
            , 0, ConfigKeys.SendBatchEmailSpecKey);
        if (persistedInfo == null) return;

        persistedInfo.Failed = info.Failed;
        persistedInfo.Sent = info.Sent;
        persistedInfo.Status = info.Status;
        persistedInfo.FailedEmails = info.FailedEmails;
        persistedInfo.SucceedEmails = info.SucceedEmails;
        persistedInfo.RKeys = info.RKeys;
        await cfgSvc.SetAsync(nameof(Public), 0, ConfigKeys.SendBatchEmailSpecKey, persistedInfo);
    }

    private async Task ClearCache(long topicId)
    {
        var hkey = GetTopicContentHKey(_tenantId, topicId);
        await cache.KeyDeleteAsync(hkey);
        await cache.KeyDeleteAsync(_contentTenantHKey);
        await cache.KeyDeleteAsync(_sendEmailsTenantHKey);
    }

    /// <summary>
    /// Get batch email detail with success/fail status
    /// Returns list of {email, isSuccess, isFail}
    /// Priority: Redis cache first, then Config table (PersistData)
    /// </summary>
    public async Task<List<BatchEmailDetailItem>> GetBatchEmailDetail()
    {
        var result = new List<BatchEmailDetailItem>();
        var succeedEmails = new HashSet<string>();
        var failedEmails = new HashSet<string>();

        // Get info from Config to get TopicKey
        var info = await cfgSvc.GetAsync<SendBatchEmailInfo>(nameof(Public), 0, ConfigKeys.SendBatchEmailSpecKey);
        if (info == null) return result;

        // Try to read from Redis cache first
        var succeedKey = CacheKeys.GetSendBatchEmailHasSentHKey(info.TopicKey);
        var failedKey = CacheKeys.GetSendBatchEmailFailedHKey(_tenantId);

        var cachedSucceedEmails = await cache.GetAllMembersAsync(succeedKey);
        var db = cache.GetDatabase();
        
        // Check if cache has data
        var hasCacheData = cachedSucceedEmails.Length > 0;

        if (hasCacheData)
        {
            // Read from Redis cache
            foreach (var email in cachedSucceedEmails)
            {
                succeedEmails.Add(email);
            }

            await foreach (var entry in db.HashScanAsync(failedKey))
            {
                failedEmails.Add(entry.Name!);
            }
        }
        else
        {
            // Read from PersistData (Config table)
            if (info.SucceedEmails != null)
            {
                foreach (var email in info.SucceedEmails)
                {
                    succeedEmails.Add(email);
                }
            }

            if (info.FailedEmails != null)
            {
                foreach (var emailArr in info.FailedEmails)
                {
                    if (emailArr.Length > 0)
                    {
                        failedEmails.Add(emailArr[0]); // First element is email
                    }
                }
            }
        }

        // Build result: succeed emails
        foreach (var email in succeedEmails)
        {
            result.Add(new BatchEmailDetailItem
            {
                Email = email,
                IsSuccess = "1",
                IsFail = "0"
            });
        }

        // Build result: failed emails (exclude those already in succeed)
        foreach (var email in failedEmails)
        {
            if (!succeedEmails.Contains(email))
            {
                result.Add(new BatchEmailDetailItem
                {
                    Email = email,
                    IsSuccess = "0",
                    IsFail = "1"
                });
            }
        }

        return result;
    }
}
