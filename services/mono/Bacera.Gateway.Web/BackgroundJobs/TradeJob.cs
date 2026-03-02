using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Web.EventHandlers.Trade;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.BackgroundJobs;

public class TradeJob(IServiceProvider serviceProvider, IMediator mediator, CentralDbContext centralDbContext)
    : ITradeJob
{
    public async Task<Dictionary<long, int>> CheckOpenTrade()
    {
        var result = new Dictionary<long, int>();

        var tenants = await centralDbContext.Tenants
            .Select(x => new { x.Id })
            .ToListAsync();
        foreach (var tenant in tenants)
        {
            using var scope = serviceProvider.CreateScope();
            var s = scope.ServiceProvider.GetRequiredService<Tenancy>();
            s.SetTenantId(tenant.Id);

            var tradingSvc = scope.ServiceProvider.GetRequiredService<TradingService>();
            var configurationSvc = scope.ServiceProvider.GetRequiredService<ConfigurationService>();

            var offsetAccountNumbers = await GetOffsetCheckAccountNumbers(configurationSvc);

            var count = 0;
            var trades = await TrackTrades(configurationSvc, tradingSvc);

            foreach (var trade in trades)
            {
                count += await CheckOffsetAccountOpenTrade(trade, offsetAccountNumbers);

                // Other track trade logic
                // ...
            }

            result.Add(tenant.Id, count);
        }

        return result;
    }

    private static async Task<List<TradeViewModel>> TrackTrades(ConfigurationService configSvc,
        TradingService tradingSvc)
    {
        var mt4Criteria = new TradeViewModel.Criteria
        {
            Page = 1,
            Size = 100,
            IsClosed = false,
            SortField = "Id",
            SortFlag = false,
            From = await configSvc.GetTrackMt4TradeOpenFromAsync(),
        };

        var mt5Criteria = new TradeViewModel.Criteria
        {
            Page = 1,
            Size = 100,
            IsClosed = false,
            SortField = "Id",
            SortFlag = false,
            From = await configSvc.GetTrackMt5TradeOpenFromAsync(),
        };

        var tradeServices = await tradingSvc.GetTradeServicesAsync();
        var mt4Trades = new List<TradeViewModel>();
        var mt5Trades = new List<TradeViewModel>();

        foreach (var service in tradeServices)
        {
            switch (service.Platform)
            {
                case (int)PlatformTypes.MetaTrader4:
                    mt4Trades.AddRange(await tradingSvc.QueryMt4(mt4Criteria));
                    break;
                case (int)PlatformTypes.MetaTrader5:
                    mt5Trades.AddRange(await tradingSvc.QueryMt5(mt5Criteria));
                    break;
            }
        }

        var newMt4TrackStartTime =
            (mt4Trades.Any() ? mt4Trades.Max(x => x.OpenAt) : mt4Criteria.From) ?? DateTime.UtcNow;
        var newMt5TrackStartTime =
            (mt5Trades.Any() ? mt5Trades.Max(x => x.OpenAt) : mt5Criteria.From) ?? DateTime.UtcNow;

        await configSvc.SetTrackMt4TradeOpenFromAsync(newMt4TrackStartTime, 0);
        await configSvc.SetTrackMt5TradeOpenFromAsync(newMt5TrackStartTime, 0);

        // merge mt4 and mt5
        return mt4Trades.Concat(mt5Trades).ToList();
    }

    private async Task<int> CheckOffsetAccountOpenTrade(TradeViewModel trade,
        ICollection<long> accountNumbers)
    {
        await mediator.Publish(new OffsetAccountOpenTradeEvent(trade));
        if (!accountNumbers.Contains(trade.AccountNumber)) return 0;
        await mediator.Publish(new OffsetAccountOpenTradeEvent(trade));
        return 1;
    }

    private static async Task<List<long>> GetOffsetCheckAccountNumbers(ConfigurationService configSvc)
    {
        var offsetChecks = await configSvc.GetOffsetCheckListAsync();
        var offsetAccountNumbers = offsetChecks.SelectMany(x => x.AccountNumbers).ToList();
        return offsetAccountNumbers;
    }
}