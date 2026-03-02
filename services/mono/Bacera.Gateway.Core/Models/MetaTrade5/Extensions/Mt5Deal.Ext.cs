namespace Bacera.Gateway.Integration;

partial class Mt5Deal
{
}

public static class Mt5DealExtensions
{
    public static IQueryable<TradeRebate> ToTradeRebate(this IQueryable<IMt5Deal> query, int tradeServiceId = 0,
        CurrencyTypes currencyId = CurrencyTypes.Invalid)
        => query
            .Select(x => new TradeRebate
            {
                TradeServiceId = tradeServiceId,
                AccountNumber = (long)x.Login,
                Symbol = x.Symbol,


                // TODO: Ticket needs to be further discussed, currently it is the orderId of the closed deal
                Ticket = (long)x.Deal,
                // Ticket = (long)x.Order,

                // For TradeRebate import only!!!
                // MT4: Volume = x.Volume; MT5: Volume = x.VolumeClosed / 100.0;
                Volume = (int)x.VolumeClosed / 100,

                Action = (int)x.Action,
                TimeStamp = x.Timestamp,
                CurrencyId = (int)currencyId,
                ClosedOn = DateTime.SpecifyKind(x.TimeMsc, DateTimeKind.Utc),
                OpenedOn = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
                Commission = x.Commission,
                Swaps = x.Storage,
                OpenPrice = x.VolumeClosed == 0 ? x.Price : 0,
                ClosePrice = x.VolumeClosed > 0 ? x.Price : 0,
                Profit = x.Profit,
                Status = (int)TradeRebateStatusTypes.Created,
                DealId = (long)x.Deal,
            });

    public static IQueryable<Account.SQSModel> ToSQSModel(this IQueryable<Mt5Deals2025s> query, long tenantId)
        => query.Select(x => new Account.SQSModel
        {
            AccountNumber = (long)x.Login,
            Ticket = (long)x.Deal,
            CloseTime = DateTime.SpecifyKind(x.TimeMsc, DateTimeKind.Utc),
            Volume = (int)x.VolumeClosed / 100,
            Symbol = x.Symbol,
            TenantId = tenantId
        });

    public static IQueryable<TradeViewModel> ToViewModel(this IQueryable<IMt5Deal> query, int? serviceId = null)
        => query.Select(x => new TradeViewModel
        {
            Id = (long)x.Deal,
            Position = (long)x.PositionId,
            Symbol = x.Symbol,
            Cmd = (int)x.Action,
            Digits = (int)x.Digits,
            Ticket = (long)x.Deal,
            Comment = x.Comment,
            Profit = x.Profit,
            Reason = (int)x.Reason,
            Volume = x.VolumeClosed / 10000.0,
            AccountNumber = (long)x.Login,
            OpenAt = x.VolumeClosed == 0 ? x.Time : null,
            OpenPrice = x.VolumeClosed == 0 ? x.Price : 0,
            CloseAt = x.VolumeClosed > 0 ? x.Time : null,
            ClosePrice = x.VolumeClosed > 0 ? x.Price : 0,
            ServiceId = serviceId ?? 0,
            TimeStamp = x.Timestamp,
            CreatedOn = x.Time,
            UpdatedOn = x.Time,
            Sl = x.PriceSl,
            Tp = x.PriceTp,
            Commission = x.Commission,
            Swaps = x.Storage,
        });

    public static IQueryable<TradeViewModel> ToReportViewModel(this IQueryable<IMt5Deal> query, int? serviceId = null)
        => query.Select(x => new TradeViewModel
        {
            Id = (long)x.Deal,
            Position = (long)x.PositionId,
            Symbol = x.Symbol,
            Cmd = (int)x.Action,
            Ticket = (long)x.Deal,
            Profit = x.Profit,
            Volume = x.Volume / 10000.0,
            AccountNumber = (long)x.Login,
            OpenAt = x.VolumeClosed == 0 ? x.Time : null,
            OpenPrice = x.VolumeClosed == 0 ? x.Price : null,
            CloseAt = x.VolumeClosed > 0 ? x.Time : null,
            ClosePrice = x.VolumeClosed > 0 ? x.Price : null,
            ServiceId = serviceId ?? 0,
            TimeStamp = x.Timestamp,
            CreatedOn = x.Time,
            UpdatedOn = x.Time,
            Swaps = x.Storage,
        });
}