using Bacera.Gateway.Context;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bacera.Gateway.Web.BackgroundJobs.GeneralJob;

public partial class GeneralJob(
    IServiceProvider serviceProvider,
    AuthDbContext authDbContext,
    IMyCache cache,
    IMessageQueueService mqService,
    IOptions<AmazonSQSOptions> sqsOptions,
    ILogger<GeneralJob> logger,
    MyDbContextPool myDbContextPool)
    : IGeneralJob
{
    public async Task<Tuple<bool, string>> ReadOnlyCodeNoticeAsync(long tenantId, ReadOnlyCodeNoticeViewModel model,
        string language = LanguageTypes.English)
        => await ProcessEmailAsync(tenantId, async svc => await svc.ReadOnlyCodeNoticeAsync(model, language));

    public async Task<Tuple<bool, string>> ConfirmEmailAsync(long tenantId, ConfirmEmailViewModel model,
        string language = LanguageTypes.English)
        => await ProcessEmailAsync(tenantId, async svc => await svc.ConfirmEmailAsync(model, language));

    public async Task<Tuple<bool, string>> ResetTradeAccountPasswordAsync(long tenantId,
        ResetTradeAccountPasswordViewModel model,
        string language = LanguageTypes.English) =>
        await ProcessEmailAsync(tenantId,
            async svc =>
                await svc.ResetTradeAccountPasswordAsync(model, language));

    public async Task<Tuple<bool, string>> TradeDemoAccountCreatedAsync(long tenantId,
        TradeDemoAccountCreatedViewModel model,
        string language = LanguageTypes.English) =>
        await ProcessEmailAsync(tenantId,
            async svc =>
                await svc.TradeDemoAccountCreatedAsync(model, language));

    public async Task<Tuple<bool, string>> ResetPasswordAsync(long tenantId, ResetPasswordViewModel model,
        string language = LanguageTypes.English)
        => await ProcessEmailAsync(tenantId, async svc => await svc.ResetPasswordAsync(model, language));

    public async Task<Tuple<bool, string>> TradeAccountLeverageChangedAsync(long tenantId,
        TradeAccountLeverageChangedViewModel model,
        string language = LanguageTypes.English)
        => await ProcessEmailAsync(tenantId,
            async svc => await svc.TradeAccountLeverageChangedAsync(model, language));

    public async Task<Tuple<bool, string>> ApplicationForWholesaleCreatedAsync(long tenantId,
        ApplicationForWholesaleCreatedViewModel model,
        string language = LanguageTypes.English)
        => await ProcessEmailAsync(tenantId,
            async svc => await svc.ApplicationForWholesaleCreatedAsync(model, language));

    public async Task<Tuple<bool, string>> ApplicationForWholesaleApprovedAsync(long tenantId,
        ApplicationForWholesaleApprovedViewModel model,
        string language = LanguageTypes.English)
        => await ProcessEmailAsync(tenantId,
            async svc => await svc.ApplicationForWholesaleApprovedAsync(model, language));

    public async Task<Tuple<bool, string>> ApplicationForWholesaleRejectedAsync(long tenantId,
        ApplicationForWholesaleRejectedViewModel model,
        string language = LanguageTypes.English)
        => await ProcessEmailAsync(tenantId,
            async svc => await svc.ApplicationForWholesaleRejectedAsync(model, language));

    public async Task<Tuple<bool, string>> ApplicationForWholesaleEvidenceNeededAsync(long tenantId,
        ApplicationForWholesaleEvidenceNeededViewModel model,
        string language = LanguageTypes.English)
        => await ProcessEmailAsync(tenantId,
            async svc => await svc.ApplicationForWholesaleEvidenceNeededAsync(model, language));

    public async Task<Tuple<bool, string>> ApplicationForWholesaleEvidenceForSophisticatedInvestorNeededAsync(
        long tenantId,
        ApplicationForWholesaleEvidenceForSophisticatedInvestorNeededViewModel model,
        string language = LanguageTypes.English)
        => await ProcessEmailAsync(tenantId,
            async svc => await svc.ApplicationForWholesaleEvidenceForSophisticatedInvestorNeededAsync(model, language));

    private IServiceScope CreateTenantScopeByTenantIdAsync(long tenantId) =>
        serviceProvider.CreateTenantScope(tenantId);

    private async Task<Tuple<bool, string>> ProcessEmailAsync(long tenantId,
        Func<ISendMailService, Task<Tuple<bool, string>>> process)
    {
        // var tenant = await centralDbContext.Tenants.SingleOrDefaultAsync(x => x.Id == tenantId);
        // if (tenant == null)
        //     return Tuple.Create(false, "__TENANT_NOT_FOUND__");

        using var scope = serviceProvider.CreateScope();
        var s = scope.ServiceProvider.GetRequiredService<Tenancy>();
        s.SetTenantId(tenantId);
        var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();

        return await process(sendMailService);
    }
}