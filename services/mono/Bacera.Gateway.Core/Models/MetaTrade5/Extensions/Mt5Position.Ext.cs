namespace Bacera.Gateway.Integration;

partial class Mt5Position
{
}

public static class Mt5PositionExtensions
{
    public static IQueryable<TradeViewModel> ToTradeViewModel(this IQueryable<Mt5Position> query, int? serviceId = null)
        => query.Select(x => new TradeViewModel
        {
            Id = (long)x.PositionId,
            Position = (long)x.Position,
            Symbol = x.Symbol,
            Cmd = (int)x.Action,
            Digits = (int)x.Digits,
            Ticket = (long)x.Position,
            Comment = x.Comment,
            Profit = x.Profit,
            Reason = (int)x.Reason,
            Volume = x.Volume / 10000.0,
            AccountNumber = (long)x.Login,
            OpenAt = x.TimeCreateMsc,
            OpenPrice = Math.Round(x.PriceOpen, (int)x.Digits),
            CurrentPrice = Math.Round(x.PriceCurrent, (int)x.Digits),
            CloseAt = null,
            ClosePrice = null,
            ServiceId = serviceId ?? 0,
            TimeStamp = x.Timestamp,
            CreatedOn = x.TimeCreateMsc,
            UpdatedOn = x.TimeUpdateMsc,
            Sl = x.PriceSl,
            Tp = x.PriceTp,
            Commission = 0,
            Swaps = x.Storage,
        });
}