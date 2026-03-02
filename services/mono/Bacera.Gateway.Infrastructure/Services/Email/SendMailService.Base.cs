using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bacera.Gateway.Services;

public partial class SendMailService : ISendMailService
{
    private readonly TenantDbContext _ctx;
    private readonly IEmailSender _sendMailSvc;
    private readonly ConfigurationService _cfgSvc;
    private readonly ILogger<SendMailService> _logger;

    public SendMailService(
        IEmailSender sendmailService
        , TenantDbContext ctx
        , ConfigurationService configurationService
        , ILogger<SendMailService>? logger
    )
    {
        _ctx = ctx;
        _sendMailSvc = sendmailService;
        _cfgSvc = configurationService;
        _logger = logger ?? new NullLogger<SendMailService>();
    }

    public async Task<TopicContent> GetTemplate(string title, string language)
    {
        return await _ctx.TopicContents
                   .Where(x => x.Topic.Title == title)
                   .Where(x => x.Topic.Type == (short)TopicTypes.EmailTemplate)
                   .Where(x => x.Language == language)
                   .FirstOrDefaultAsync()
               ?? await _ctx.TopicContents
                   .Where(x => x.Topic.Type == (short)TopicTypes.EmailTemplate)
                   .Where(x => x.Topic.Title == title)
                   .Where(x => x.Language == LanguageTypes.English)
                   .FirstOrDefaultAsync()
               ?? new TopicContent();
    }

    // format id with current tenant for cache
    // added more parameters to template key
    private static string FormatTemplateIdForTenant(long tenantId, int id, string language = LanguageTypes.English)
        => id + "|" + tenantId + "|" + language.Trim();
}