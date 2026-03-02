namespace Bacera.Gateway.Web.BackgroundJobs;

public interface ITradeAccountJob
{
    Task<Dictionary<long, double>> CheckTradeAccountBalanceAsync(long tenantId, long partyId);
    Task<bool> AdjustCreditOrBalanceByBatchId(long tenantId, long adjustBatchId);
}