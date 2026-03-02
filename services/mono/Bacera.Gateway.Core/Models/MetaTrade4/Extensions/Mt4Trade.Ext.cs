namespace Bacera.Gateway.Integration;

partial class Mt4Trade
{
}

public static partial class Mt4TradeExtensions
{
    public static IQueryable<TradeRebate> ToTradeRebate(this IQueryable<Mt4Trade> query, int tradeServiceId = 0,
        CurrencyTypes currencyId = CurrencyTypes.Invalid)
        => query
            .Select(x => new TradeRebate
            {
                TradeServiceId = tradeServiceId,
                Ticket = x.Ticket,
                AccountNumber = x.Login,
                Symbol = x.Symbol,

                // For TradeRebate import only!!!
                // MT4: Volume = x.Volume; MT5: Volume = x.VolumeClosed / 100.0;
                Volume = x.Volume,
                Commission = x.Commission,
                Swaps = x.Swaps,
                OpenPrice = Math.Round(x.OpenPrice, x.Digits),
                ClosePrice = Math.Round(x.ClosePrice, x.Digits),
                Profit = x.Profit,
                Reason = x.Reason,
                Action = x.Cmd,
                TimeStamp = x.Timestamp,
                ClosedOn = DateTime.SpecifyKind(x.CloseTime, DateTimeKind.Utc),
                OpenedOn = DateTime.SpecifyKind(x.OpenTime, DateTimeKind.Utc),
                CurrencyId = (int)currencyId,
                Status = (int)TradeRebateStatusTypes.Created,
            });

    public static IQueryable<TradeViewModel> ToTradeViewModel(this IQueryable<Mt4Trade> query, long tenantId = 0,
        int serviceId = 0)
        => query.Select(x => new TradeViewModel
        {
            Id = x.Ticket,
            Position = null,
            Symbol = x.Symbol,
            Cmd = x.Cmd,
            Digits = x.Digits,
            Ticket = x.Ticket,
            Comment = x.Comment,
            Profit = Math.Round(x.Profit, 2),
            Reason = x.Reason,
            Volume = x.Volume / 100.0,
            AccountNumber = x.Login,
            OpenAt = x.OpenTime,
            OpenPrice = Math.Round(x.OpenPrice, x.Digits),
            CloseAt = x.CloseTime,
            ClosePrice = x.ClosePrice,
            TenantId = tenantId,
            ServiceId = serviceId,
            TimeStamp = x.Timestamp,
            CreatedOn = x.OpenTime,
            UpdatedOn = x.ModifyTime,
            Sl = x.Sl,
            Tp = x.Tp,
            Commission = x.Commission,
            Swaps = x.Swaps
        });

    public static IQueryable<TradeViewModel> ToTradeViewModel(this IQueryable<Mt4OpenTrade> query, long tenantId = 0,
        int serviceId = 0)
        => query.Select(x => new TradeViewModel
        {
            Id = x.Ticket,
            Position = null,
            Symbol = x.Symbol,
            Cmd = x.Cmd,
            Digits = x.Digits,
            Ticket = x.Ticket,
            Comment = x.Comment,
            Profit = Math.Round(x.Profit, 2),
            Reason = x.Reason,
            Volume = x.Volume / 100.0,
            AccountNumber = x.Login,
            OpenAt = x.OpenTime,
            OpenPrice = Math.Round(x.OpenPrice, x.Digits),
            CloseAt = x.CloseTime,
            ClosePrice = x.ClosePrice,
            TenantId = tenantId,
            ServiceId = serviceId,
            TimeStamp = x.Timestamp,
            CreatedOn = x.OpenTime,
            UpdatedOn = x.ModifyTime,
            Sl = x.Sl,
            Tp = x.Tp,
            Commission = x.Commission,
            Swaps = x.Swaps
        });

    public static IQueryable<MetaTrade> ToMetaTrade(this IQueryable<Mt4Trade> query, long tenantId = 0,
        int serviceId = 0)
        => query.Select(x => new MetaTrade
        {
            // Id = x.Ticket,
            Position = null,
            Symbol = x.Symbol,
            Cmd = x.Cmd,
            Digits = x.Digits,
            Ticket = x.Ticket,
            Comment = x.Comment,
            Profit = Math.Round(x.Profit, 2),
            Reason = x.Reason,
            Volume = x.Volume / 100.0,
            VolumeOriginal = x.Volume,
            AccountNumber = x.Login,
            OpenAt = DateTime.SpecifyKind(x.OpenTime, DateTimeKind.Utc),
            OpenPrice = Math.Round(x.OpenPrice, x.Digits),
            CloseAt = DateTime.SpecifyKind(x.CloseTime, DateTimeKind.Utc),
            ClosePrice = Math.Round(x.ClosePrice, x.Digits),
            TenantId = tenantId,
            ServiceId = serviceId,
            TimeStamp = x.Timestamp,
            CreatedOn = x.OpenTime,
            UpdatedOn = x.ModifyTime,
            Sl = x.Sl,
            Tp = x.Tp,
            Commission = x.Commission,
            Swaps = x.Swaps
        });
}