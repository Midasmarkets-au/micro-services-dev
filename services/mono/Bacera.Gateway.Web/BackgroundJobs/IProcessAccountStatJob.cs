namespace Bacera.Gateway.Web.BackgroundJobs;

public interface IProcessAccountStatJob
{
    Task<(bool, string)> ClientAccountAddedAsync(long tenantId, long accountId);
    Task<(bool, string)> AgentAccountAddedAsync(long tenantId, long accountId);
    Task<(bool, string)> DepositApprovedAsync(long tenantId, long depositId);
    Task<(bool, string)> WithdrawalApprovedAsync(long tenantId, long withdrawalId);
    Task<(bool, string)> RebateReleasedAsync(long tenantId, long rebateId);
    Task<(bool, string)> TradeClosedAsync(long tenantId, long tradeRebateId);
}