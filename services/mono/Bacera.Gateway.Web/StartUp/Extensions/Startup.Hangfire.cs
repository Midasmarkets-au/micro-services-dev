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

        // "Calculate Rebate", "Release Rebate", and "Crypto Wallet" recurring jobs
        // have been moved to the Rust scheduler service (services/scheduler).
        // The Rust scheduler calls back via gRPC (MonoCallbackService.TriggerXxx) to enqueue
        // the corresponding Hangfire job immediately.
        // TryAddOrUpdateRecurringJob(() => RecurringJob.AddOrUpdate<IRebateJob>(
        //     "Calculate Rebate",
        //     "rebate",
        //     w => w.CalculateRebate(),
        //     "*/2 * * * *"));
        //
        // TryAddOrUpdateRecurringJob(() => RecurringJob.AddOrUpdate<IRebateJob>(
        //     "Release Rebate",
        //     "rebate",
        //     w => w.ReleaseRebateAsync(),
        //     "*/2 * * * *"));
        //
        // TryAddOrUpdateRecurringJob(() => RecurringJob.AddOrUpdate<CryptoJob>(
        //     "Crypto Wallet",
        //     "crypto-monitor",
        //     w => w.MonitorAsync(),
        //     "*/1 * * * *"));

        // "Close Trade Job" and "Report Account Daily Confirmation" recurring jobs
        // have been moved to the Rust report service (services/report).
        // The Rust service runs its own cron scheduler and no longer needs Hangfire for these.
        // TryAddOrUpdateRecurringJob(() => RecurringJob.AddOrUpdate<IReportJob>(
        //     "Close Trade Job",
        //     HangFireQueues.AccountReport,
        //     w => w.ExecuteCloseTradeJobAsync(),
        //     "30 22 * * *"));
        //
        // TryAddOrUpdateRecurringJob(() => RecurringJob.AddOrUpdate<IReportJob>(
        //     "Report Account Daily Confirmation",
        //     HangFireQueues.AccountConfirmationReport,
        //     w => w.GenerateAccountDailyConfirmationReport(CancellationToken.None),
        //     Utils.IsCurrentDSTLosAngeles(DateTime.UtcNow) ? "29 21 * * 1-5" : "29 22 * * 1-5"));
    }

    private static void TryAddOrUpdateRecurringJob(System.Action register)
    {
        try
        {
            register();
        }
        catch (Hangfire.PostgreSql.PostgreSqlDistributedLockException ex)
        {
            Console.WriteLine($"[WARN] Could not acquire distributed lock during Hangfire job registration: {ex.Message}. The job will be registered on next startup.");
        }
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