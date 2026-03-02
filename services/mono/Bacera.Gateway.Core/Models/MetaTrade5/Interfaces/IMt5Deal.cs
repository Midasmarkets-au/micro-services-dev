namespace Bacera.Gateway.Integration;

public interface IMt5Deal
{
    ulong Deal { get; set; }
    long Timestamp { get; set; }
    string ExternalId { get; set; }
    ulong Login { get; set; }
    ulong Dealer { get; set; }
    ulong Order { get; set; }
    uint Action { get; set; }
    uint Entry { get; set; }
    uint Reason { get; set; }
    uint Digits { get; set; }
    uint DigitsCurrency { get; set; }
    double ContractSize { get; set; }
    DateTime Time { get; set; }
    DateTime TimeMsc { get; set; }
    string Symbol { get; set; }
    double Price { get; set; }
    ulong VolumeExt { get; set; }
    double Profit { get; set; }
    double Storage { get; set; }
    double Commission { get; set; }
    double Fee { get; set; }
    double RateProfit { get; set; }
    double RateMargin { get; set; }
    ulong ExpertId { get; set; }
    ulong PositionId { get; set; }
    string Comment { get; set; }
    double ProfitRaw { get; set; }
    double PricePosition { get; set; }
    double PriceSl { get; set; }
    double PriceTp { get; set; }
    ulong VolumeClosedExt { get; set; }
    double TickValue { get; set; }
    double TickSize { get; set; }
    ulong Flags { get; set; }
    string Gateway { get; set; }
    double PriceGateway { get; set; }
    uint ModifyFlags { get; set; }
    double MarketBid { get; set; }
    double MarketAsk { get; set; }
    double MarketLast { get; set; }
    ulong Volume { get; set; }
    ulong VolumeClosed { get; set; }
    string ApiData { get; set; }
}