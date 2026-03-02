namespace Bacera.Gateway.Integration;

partial class Mt5Deals2025s
{
}

public static class Mt5DealsViewExtensions
{
    public static IQueryable<TradeViewModel> ToTradeViewModel(this IQueryable<Mt5Deals2025> query, long tenantId = 0,
    int serviceId = 0)
    => query.Select(x => new TradeViewModel
    {
        Id = (long)x.Deal,
        Position = (long)x.PositionId,
        Symbol = x.Symbol,
        Cmd = (int)x.Action,
        Digits = (int)x.Digits,
        Ticket = (long)x.Deal,
        Comment = x.Comment,
        Profit = Math.Round(x.Profit, 2),
        Reason = (int)x.Reason,
        Volume = x.VolumeClosed / 10000.0,
        AccountNumber = (long)x.Login,
        OpenAt = x.VolumeClosed == 0 ? DateTime.SpecifyKind(x.TimeMsc, DateTimeKind.Utc) : null,
        OpenPrice = x.VolumeClosed == 0 ? Math.Round(x.Price, (int)x.Digits) : 0,
        CloseAt = x.VolumeClosed > 0 ? DateTime.SpecifyKind(x.TimeMsc, DateTimeKind.Utc) : null,
        ClosePrice = x.VolumeClosed > 0 ? Math.Round(x.Price, (int)x.Digits) : 0,
        TenantId = tenantId,
        ServiceId = serviceId,
        TimeStamp = x.Timestamp,
        CreatedOn = x.Time,
        UpdatedOn = x.Time,
        Sl = x.PriceSl,
        Tp = x.PriceTp,
        Commission = x.Commission,
        Swaps = x.Storage,
    });

    public static IQueryable<TradeViewModel> ToTradeViewModel(this IQueryable<Mt5Deals2024> query, long tenantId = 0,
        int serviceId = 0)
        => query.Select(x => new TradeViewModel
        {
            Id = (long)x.Deal,
            Position = (long)x.PositionId,
            Symbol = x.Symbol,
            Cmd = (int)x.Action,
            Digits = (int)x.Digits,
            Ticket = (long)x.Deal,
            Comment = x.Comment,
            Profit = Math.Round(x.Profit, 2),
            Reason = (int)x.Reason,
            Volume = x.VolumeClosed / 10000.0,
            AccountNumber = (long)x.Login,
            OpenAt = x.VolumeClosed == 0 ? DateTime.SpecifyKind(x.TimeMsc, DateTimeKind.Utc) : null,
            OpenPrice = x.VolumeClosed == 0 ? Math.Round(x.Price, (int)x.Digits) : 0,
            CloseAt = x.VolumeClosed > 0 ? DateTime.SpecifyKind(x.TimeMsc, DateTimeKind.Utc) : null,
            ClosePrice = x.VolumeClosed > 0 ? Math.Round(x.Price, (int)x.Digits) : 0,
            TenantId = tenantId,
            ServiceId = serviceId,
            TimeStamp = x.Timestamp,
            CreatedOn = x.Time,
            UpdatedOn = x.Time,
            Sl = x.PriceSl,
            Tp = x.PriceTp,
            Commission = x.Commission,
            Swaps = x.Storage,
        });


    public static IQueryable<TradeViewModel> ToTradeViewModel(this IQueryable<Mt5Deals2025s> query, long tenantId = 0,
        int serviceId = 0)
        => query.Select(x => new TradeViewModel
        {
            Id = (long)x.Deal,
            Position = (long)x.PositionId,
            Symbol = x.Symbol,
            Cmd = (int)x.Action,
            Digits = (int)x.Digits,
            Ticket = (long)x.Deal,
            Comment = x.Comment,
            Profit = Math.Round(x.Profit, 2),
            Reason = (int)x.Reason,
            Volume = x.VolumeClosed / 10000.0,
            AccountNumber = (long)x.Login,
            OpenAt = x.VolumeClosed == 0 ? DateTime.SpecifyKind(x.TimeMsc, DateTimeKind.Utc) : null,
            OpenPrice = x.VolumeClosed == 0 ? Math.Round(x.Price, (int)x.Digits) : 0,
            CloseAt = x.VolumeClosed > 0 ? DateTime.SpecifyKind(x.TimeMsc, DateTimeKind.Utc) : null,
            ClosePrice = x.VolumeClosed > 0 ? Math.Round(x.Price, (int)x.Digits) : 0,
            TenantId = tenantId,
            ServiceId = serviceId,
            TimeStamp = x.Timestamp,
            CreatedOn = x.Time,
            UpdatedOn = x.Time,
            Sl = x.PriceSl,
            Tp = x.PriceTp,
            Commission = x.Commission,
            Swaps = x.Storage,
        });


    public static IQueryable<MetaTrade> ToMetaTrade(this IQueryable<Mt5Deals2025s> query, long tenantId = 0,
        int serviceId = 0)
        => query.Select(x => new MetaTrade
        {
            // Id = (long)x.Deal,
            Position = (long)x.PositionId,
            Symbol = x.Symbol,
            Cmd = (int)x.Action,
            Digits = (int)x.Digits,
            Ticket = (long)x.Deal,
            Comment = x.Comment,
            Profit = x.Profit,
            Reason = (int)x.Reason,
            Volume = x.VolumeClosed / 10000.0,
            VolumeOriginal = (int)(x.VolumeClosed / 100),
            AccountNumber = (long)x.Login,
            OpenAt = x.VolumeClosed == 0 ? DateTime.SpecifyKind(x.TimeMsc, DateTimeKind.Utc) : null,
            OpenPrice = x.VolumeClosed == 0 ? Math.Round(x.Price, (int)x.Digits) : 0,
            CloseAt = x.VolumeClosed > 0 ? DateTime.SpecifyKind(x.TimeMsc, DateTimeKind.Utc) : null,
            ClosePrice = x.VolumeClosed > 0 ? Math.Round(x.Price, (int)x.Digits) : 0,
            TenantId = tenantId,
            ServiceId = serviceId,
            TimeStamp = x.Timestamp,
            CreatedOn = x.Time,
            UpdatedOn = x.Time,
            Sl = x.PriceSl,
            Tp = x.PriceTp,
            Commission = x.Commission,
            Swaps = x.Storage,
        });

    public static IQueryable<MetaTrade> ToMetaTrade(this IQueryable<Mt5Deals2025> query, long tenantId = 0,
    int serviceId = 0)
    => query.Select(x => new MetaTrade
    {
        // Id = (long)x.Deal,
        Position = (long)x.PositionId,
        Symbol = x.Symbol,
        Cmd = (int)x.Action,
        Digits = (int)x.Digits,
        Ticket = (long)x.Deal,
        Comment = x.Comment,
        Profit = x.Profit,
        Reason = (int)x.Reason,
        Volume = x.VolumeClosed / 10000.0,
        VolumeOriginal = (int)(x.VolumeClosed / 100),
        AccountNumber = (long)x.Login,
        OpenAt = x.VolumeClosed == 0 ? DateTime.SpecifyKind(x.TimeMsc, DateTimeKind.Utc) : null,
        OpenPrice = x.VolumeClosed == 0 ? Math.Round(x.Price, (int)x.Digits) : 0,
        CloseAt = x.VolumeClosed > 0 ? DateTime.SpecifyKind(x.TimeMsc, DateTimeKind.Utc) : null,
        ClosePrice = x.VolumeClosed > 0 ? Math.Round(x.Price, (int)x.Digits) : 0,
        TenantId = tenantId,
        ServiceId = serviceId,
        TimeStamp = x.Timestamp,
        CreatedOn = x.Time,
        UpdatedOn = x.Time,
        Sl = x.PriceSl,
        Tp = x.PriceTp,
        Commission = x.Commission,
        Swaps = x.Storage,
    });
}