using Bacera.Gateway.Context;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.BackgroundJobs;

public class SuperAdminJob(
    IServiceProvider serviceProvider,
    CentralDbContext centralDbContext,
    MyDbContextPool pool,
    AuthDbContext authDbContext)
{

    public async Task UpdatePartyNativeNameAndEmailForAllTenants()
    {
        var tenants = await centralDbContext.Tenants
            .Select(x => new { x.Id })
            .ToListAsync();
        foreach (var tenant in tenants)
        {
            using var scope = serviceProvider.CreateScope();
            var tenancyResolver = scope.ServiceProvider.GetRequiredService<Tenancy>();
            tenancyResolver.SetTenantId(tenant.Id);
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            var page = 1;
            const int pageSize = 500;
            while (true)
            {
                var parties = await ctx.Parties
                    .OrderBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
                    .ToListAsync();
                if (parties.Count == 0)
                    break;
                var users = await authDbContext.Users
                    .Where(x => parties.Select(p => p.Id).Contains(x.PartyId) && x.TenantId == tenant.Id)
                    .ToListAsync();
                foreach (var party in parties)
                {
                    var user = users.FirstOrDefault(x => x.PartyId == party.Id);
                    if (user == null)
                        continue;
                    party.NativeName = user.GuessUserNativeName();
                    party.Email = user.Email ?? "";
                }

                ctx.UpdateRange(parties);
                await ctx.SaveChangesAsync();
                page++;
            }
        }
    }

    public async Task TJob()
    {
        var date = Utils.ParseToUTC("2024-10-08");
        foreach (var tenantId in pool.GetTenantIds())
        {
            // BcrLog.Slack($"tenantId: {tenantId} resend daily report start");
            using var scope = serviceProvider.CreateTenantScope(tenantId);
            var reportSvc = scope.ServiceProvider.GetRequiredService<ReportService>();
            long remains;
            do
            {
                remains = await reportSvc.ProcessSendAccountDailyReportEmail(date);
                // BcrLog.Slack($"Remains: {remains}");
            } while (remains > 0);

            BcrLog.Slack($"tenantId: {tenantId} resend daily report done");
        }
    }

    public async Task SendDailyConfirmationEmailJob(long tenantId, DateTime date)
    {
        using var scope = serviceProvider.CreateScope();
        var tenancyResolver = scope.ServiceProvider.GetRequiredService<Tenancy>();
        tenancyResolver.SetTenantId(tenantId);
        var reportSvc = scope.ServiceProvider.GetRequiredService<ReportService>();
        do
        {
            var remains = await reportSvc.ProcessSendAccountDailyReportEmail(date);
            if (remains == 0) break;
            await Task.Delay(1000);
        } while (true);
    }

    public async Task ProcessActionAsync(Task task)
    {
        await task;
    }
}