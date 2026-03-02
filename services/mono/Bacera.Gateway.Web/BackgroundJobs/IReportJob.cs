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
}