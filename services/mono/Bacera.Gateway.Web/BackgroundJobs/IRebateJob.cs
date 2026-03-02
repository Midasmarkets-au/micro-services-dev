namespace Bacera.Gateway.Web.BackgroundJobs;

public interface IRebateJob
{
    // Task<Dictionary<long, int>> GenerateTradeRebate();
    Task<(bool, string)> CalculateRebate();
    Task ReleaseRebateAsync();
    
}