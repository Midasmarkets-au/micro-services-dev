using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bacera.Gateway.Web.BackgroundJobs.GeneralJob;

public partial class GeneralJob
{
    public async Task<(bool, string)> SendEmailByTopicIdWithContent(long tenantId, string uuid)
    {
        using var scope = serviceProvider.CreateTenantScope(tenantId);
        var cfgSvc = scope.ServiceProvider.GetRequiredService<ConfigService>();
        var info = await cfgSvc.GetAsync<SendBatchEmailInfo>(nameof(Public), 0, ConfigKeys.SendBatchEmailSpecKey);
        if (info == null) return (false, "No batch email info found");
        if (info.Uuid != uuid) return (false, "Invalid uuid");
        
        var mqOptions = scope.ServiceProvider.GetRequiredService<IOptions<AmazonSQSOptions>>();
        var mqSvc = scope.ServiceProvider.GetRequiredService<IMessageQueueService>();
        var sendBatchEmailSvc = scope.ServiceProvider.GetRequiredService<BatchSendEmailService>();

        const string lastEnqueuedUserIdKey = "SendEmailByTopicIdWithContent_lastEnqueuedUserIdKey";
        var emailKey = CacheKeys.GetSendBatchEmailsHKey(tenantId);
        var lastEnqueuedUserIdString = await cache.GetStringAsync(lastEnqueuedUserIdKey);
        if (!long.TryParse(lastEnqueuedUserIdString, out var lastUserId))
            lastUserId = 0;

        if (info.ReceiverEmails != null)
            await sendBatchEmailSvc.WarmNoPromotionEmailCache();

        await sendBatchEmailSvc.UpdateRealTimeStatusAsync("Processing");
        var query = sendBatchEmailSvc.GetSendBatchEmailQuery(info);

        if (lastUserId != 0)
            query = query.Where(x => x.UserId > lastUserId);
        
        const int size = 100;
        var page = 0;
        var total = 0;
        while (true)
        {
            var users = await query.Skip(page * size).Take(size).ToListAsync();
            foreach (var user in users)
            {
                if (!info.Contents.ContainsKey(user.Language))
                    user.Language = LanguageTypes.English;

                var dto = SendBatchEmailDTO
                    .Build(tenantId, user.UserId, user.Email, user.Language, info.TopicId, info.TopicKey)
                    .ToSendMessageMqDTO()
                    .ToJson();

                await  mqSvc.SendAsync(dto, mqOptions.Value.BCRSendMessage, messageGroupId: tenantId.ToString());
                await cache.SetStringAsync(lastEnqueuedUserIdKey, user.UserId.ToString());
                total += 1;
            }

            if (users.Count < size)
            {
                await cache.KeyDeleteAsync(lastEnqueuedUserIdKey);
                await cache.HSetStringAsync(emailKey, "Total", total.ToString());
                break;
            }

            page += 1;
        }

        await sendBatchEmailSvc.DeletePromotionEmailCacheKey();
        return (true, "Emails have been sent");
    }
}