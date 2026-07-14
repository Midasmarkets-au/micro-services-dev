using Bacera.Gateway.Core.Utility;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.Services;
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.PostgreSql;

namespace Bacera.Gateway.Web;

public partial class Startup
{
    public static void SetupHangFire(this IServiceCollection me)
    {
        me.AddHangfire(config => config.UsePostgreSqlStorage(GetHangfireDatabaseOptions().ConnectionString));
        me.AddScoped<IHangfireWrapper, HangfireWrapper>();
    }

    public static void SetupHangFireServer(this IServiceCollection me)
    {
        if (!IsHangFireWorkerEnable()) return;
        var guid = Guid.NewGuid().ToString();
        me.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
            options.ServerName = $"{Environment.MachineName}.default.{guid}";
            options.Queues = ["default"];
        });

        me.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
            options.ServerName = $"{Environment.MachineName}.intensive-job.{guid}";
            options.Queues = [HangFireQueues.IntensiveJob];
        });

        me.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
            options.ServerName = $"{Environment.MachineName}.trade.{guid}";
            options.Queues = [HangFireQueues.Trade];
        });

        me.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
            options.ServerName = $"{Environment.MachineName}.trade-rebate.{guid}";
            options.Queues = [HangFireQueues.TradeRebate];
        });

        me.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
            options.ServerName = $"{Environment.MachineName}.rebate.{guid}";
            options.Queues = [HangFireQueues.Rebate];
        });

        me.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
            options.ServerName = $"{Environment.MachineName}.crypto.{guid}";
            options.Queues = [HangFireQueues.CryptoMonitor];
        });

        me.AddHangfireServer(options =>
        {
            options.WorkerCount = 3;
            options.ServerName = $"{Environment.MachineName}.report.{guid}";
            options.CancellationCheckInterval = TimeSpan.FromSeconds(5);
            options.Queues = [HangFireQueues.AccountReport, HangFireQueues.AccountConfirmationReport];
        });
    }

    private static bool IsHangFireWorkerEnable()
    {
        return !string.IsNullOrEmpty(GetEnvValue("HANGFIRE_WORKER_ENABLE")) &&
               GetEnvValue("HANGFIRE_WORKER_ENABLE").Equals("true", StringComparison.InvariantCultureIgnoreCase);
    }

    public static void UseHangfireJobSetup(this IApplicationBuilder me)
    {
        me.UseHangfireDashboard("/hf_manage", new DashboardOptions
        {
            Authorization =
            [
                new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    RequireSsl = false,
                    SslRedirect = false,
                    LoginCaseSensitive = true,
                    Users =
                    [
                        new BasicAuthAuthorizationUser
                        {
                            Login = GetEnvValue("HANGFIRE_DASHBOARD_USER"),
                            PasswordClear = GetEnvValue("HANGFIRE_DASHBOARD_PASS")
                        }
                    ]
                })
            ]
        });

        if (!IsHangFireWorkerEnable()) return;

        // CalculateRebate / ReleaseRebate moved to the Rust scheduler (execute_calculate /
        // execute_release in services/scheduler/src/main.rs, same */2 * * * * cadence).
        // Originally disabled in 975c45b (2026-03-23) but the registrations below were
        // accidentally reintroduced by the 50ce57f MM-Back bulk sync (2026-04-13) and have
        // been firing as dead-letter no-ops since (no Status=Created backlog remains in
        // trd."_TradeRebate"). Re-disabling here to avoid double-processing risk if new
        // backlog ever appears.
        // RecurringJob.AddOrUpdate<IRebateJob>(
        //     "Calculate Rebate",
        //     "rebate",
        //     w => w.CalculateRebate(),
        //     "*/2 * * * *");
        //
        // RecurringJob.AddOrUpdate<IRebateJob>(
        //     "Release Rebate",
        //     "rebate",
        //     w => w.ReleaseRebateAsync(),
        //     "*/2 * * * *");

        RecurringJob.AddOrUpdate<CryptoJob>(
            "Crypto Wallet",
            "crypto-monitor",
            w => w.MonitorAsync(),
            "*/1 * * * *");

        // Fire at BOTH the DST (21:xx) and non-DST (22:xx) UTC times; the *DstGuarded
        // wrappers run the real job only at the hour correct for the current DST state,
        // decided at fire time. This self-corrects across DST transitions — the old
        // form baked the cron string once at startup and drifted 1h until restart.
        RecurringJob.AddOrUpdate<IReportJob>(
            "Close Trade Job",
            HangFireQueues.AccountReport,
            w => w.ExecuteCloseTradeJobDstGuardedAsync(),
            "30 21,22 * * *");

        RecurringJob.AddOrUpdate<IReportJob>(
            "Report Account Daily Confirmation",
            HangFireQueues.AccountConfirmationReport,
            w => w.GenerateAccountDailyConfirmationReportDstGuardedAsync(),
            "29 21,22 * * 1-5");
    }
}

// RecurringJob.AddOrUpdate<IRebateJob>(
//     "Generate Trade Rebate",
//     "trade-rebate",
//     w => w.GenerateTradeRebate(),
//     "*/2 * * * *");

// RecurringJob.AddOrUpdate<IReportJob>(
//     "Report Daily Equity",
//     "equity-report",
//     w => w.GenerateDailyEquityReport(),
//     "29 21 * * 1-5");