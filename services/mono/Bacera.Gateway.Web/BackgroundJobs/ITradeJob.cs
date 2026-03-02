namespace Bacera.Gateway.Web.BackgroundJobs;

public interface ITradeJob
{
    Task<Dictionary<long, int>> CheckOpenTrade();
}