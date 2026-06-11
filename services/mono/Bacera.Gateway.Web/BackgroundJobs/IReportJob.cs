using Hangfire;

namespace Bacera.Gateway.Web.BackgroundJobs;

public interface IReportJob
{
    Task<List<Tuple<long, string>>> GenerateDailyEquityReport();
    
    Task GenerateAccountDailyConfirmationReport(CancellationToken cancellationToken);
    Task ProcessAccountDailyConfirmationReport(long tenantId, CancellationToken token,
        DateTime? date = null);

    Task<Tuple<long, string>> GenerateDailyEquityReportForTenant(long tenantId, long rowId,
        List<string>? mailTos = null, DateTime? date = null);

    Task<bool> ProcessReportRequest(long tenantId, long requestId);

    Task ExecuteCloseTradeJobAsync();

    // DST-guarded wrappers for the recurring schedules. The cron fires at both the
    // DST (21:xx) and non-DST (22:xx) UTC times; these run the real job only at the
    // hour that is correct for the *current* DST state (decided at fire time, so it
    // self-corrects across DST transitions without an app restart).
    Task ExecuteCloseTradeJobDstGuardedAsync();
    Task GenerateAccountDailyConfirmationReportDstGuardedAsync();
}