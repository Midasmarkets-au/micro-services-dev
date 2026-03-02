// namespace Bacera.Gateway;
//
// partial class TradeTransaction : IEntity
// {
//     public sealed class Criteria : EntityCriteria<TradeTransaction>
//     {
//         public Criteria()
//         {
//             SortField = nameof(TradeTransaction.Ticket);
//             DefaultDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
//         }
//
//         private DateTime DefaultDateTime { get; }
//         public long? Uid { get; set; }
//         public long? PartyId { get; set; }
//         public long? AccountNumber { get; set; }
//         public long? AccountId { get; set; }
//         public int? ServiceId { get; set; }
//         public long? Ticket { get; set; }
//         public string? Symbol { get; set; }
//         public TradeCommandTypes? Command { get; set; }
//         public List<int>? Commands { get; set; }
//         public bool? IsClosed { get; set; }
//
//         public DateTime? From { get; set; }
//         public DateTime? To { get; set; }
//
//         public DateTime? OpenedFrom { get; set; }
//         public DateTime? OpenedTo { get; set; }
//
//         public DateTime? ClosedFrom { get; set; }
//         public DateTime? ClosedTo { get; set; }
//
//         public long? SalesUid { get; set; }
//         public long? AgentUid { get; set; }
//         public long? RepUid { get; set; }
//         public List<long>? GroupIds { get; set; }
//
//         protected override void OnCollect(ICriteriaPool<TradeTransaction> pool)
//         {
//             pool.Add(x => x.TradeAccount.IdNavigation.Uid.Equals(Uid!), Uid is not null);
//             pool.Add(x => x.TradeAccount.IdNavigation.PartyId == PartyId, PartyId.HasValue && PartyId.IsTangible());
//
//             pool.Add(x => x.Cmd == (int)Command!, Command.HasValue);
//             pool.Add(x => Commands!.Contains(x.Cmd), Commands != null && Commands.Any());
//             pool.Add(x => x.ServiceId == ServiceId, ServiceId.HasValue && ServiceId.IsTangible());
//             pool.Add(x => x.Ticket == Ticket, Ticket.HasValue && Ticket.IsTangible());
//             pool.Add(x => x.TradeAccountId == AccountId, AccountId.HasValue && AccountId.IsTangible());
//             pool.Add(x => x.AccountNumber == AccountNumber, AccountNumber.HasValue && AccountNumber.IsTangible());
//             pool.Add(x => x.Symbol.ToUpper().Equals(Symbol!.ToUpper()),
//                 !string.IsNullOrEmpty(Symbol) && Symbol.IsTangible());
//
//             pool.Add(x => x.ModifiedAt >= From!.Value.ToUniversalTime(), From != null && From.IsTangible());
//             pool.Add(x => x.ModifiedAt <= To!.Value.ToUniversalTime(), To != null && To.IsTangible());
//
//             pool.Add(x => x.OpenAt >= OpenedFrom!.Value.ToUniversalTime(),
//                 OpenedFrom != null && OpenedFrom.IsTangible());
//             pool.Add(x => x.OpenAt <= OpenedTo!.Value.ToUniversalTime(), OpenedTo != null && OpenedTo.IsTangible());
//
//             pool.Add(x => x.CloseAt >= From!.Value.ToUniversalTime(), ClosedFrom != null && ClosedFrom.IsTangible());
//             pool.Add(x => x.CloseAt <= To!.Value.ToUniversalTime(), ClosedTo != null && ClosedTo.IsTangible());
//
//             pool.Add(x => x.CloseAt > DefaultDateTime, IsClosed is true);
//             pool.Add(x => x.CloseAt <= DefaultDateTime, IsClosed is false);
//
//             pool.Add(x => x.TradeAccount.IdNavigation
//                     .Groups.Any(g => g.OwnerAccount.Uid == AgentUid
//                                      && g.Type == (int)AccountGroupTypes.Agent),
//                 AgentUid.IsTangible());
//             pool.Add(x => x.TradeAccount.IdNavigation
//                     .Groups.Any(g => g.OwnerAccount.Uid == RepUid
//                                      && g.Type == (int)AccountGroupTypes.Rep),
//                 RepUid.IsTangible());
//             pool.Add(x => x.TradeAccount.IdNavigation
//                     .Groups.Any(g => g.OwnerAccount.Uid == SalesUid
//                                      && g.Type == (int)AccountGroupTypes.Sales),
//                 SalesUid.IsTangible());
//             pool.Add(
//                 x => GroupIds!.Contains(
//                     x.TradeAccount.IdNavigation.Groups.Any(g => g.Type == (int)AccountGroupTypes.Agent)
//                         ? x.TradeAccount.IdNavigation.Groups.First(g => g.Type == (int)AccountGroupTypes.Agent).Id
//                         : 0), GroupIds.IsTangible());
//
//             //pool.Add(x => IncludeCodes!.Any(like => EF.Functions.ILike(x.TradeAccount.IdNavigation.Code, like)), IncludeCodes != null && IncludeCodes.Any());
//         }
//     }
//
//     public sealed class ReportCriteria : EntityCriteria<TradeTransaction>
//     {
//         public DateTime From { get; set; }
//         public DateTime To { get; set; }
//         public CurrencyTypes? CurrencyId { get; set; }
//         public double TimeZoneOffset { get; set; }
//
//         public ReportPeriodTypes PeriodType { get; set; } =
//             ReportPeriodTypes.Hourly;
//
//         public long? SalesUid { get; set; }
//         public long? AgentUid { get; set; }
//         public long? RepUid { get; set; }
//
//         protected override void OnCollect(ICriteriaPool<TradeTransaction> pool)
//         {
//             var offsetMinutes = (int)TimeSpan.FromHours(TimeZoneOffset).TotalMinutes;
//             var fromDt = new DateTime(From.Year,
//                 From.Month,
//                 From.Day,
//                 From.Hour,
//                 0, 0, DateTimeKind.Utc).AddMinutes(offsetMinutes);
//             var toDt = new DateTime(To.Year,
//                 To.Month,
//                 To.Day,
//                 To.Hour,
//                 0, 0, DateTimeKind.Utc).AddMinutes(offsetMinutes);
//             pool.Add(x => x.TradeAccount.CurrencyId == (int)CurrencyId!, CurrencyId.HasValue);
//             pool.Add(x => x.ModifiedAt >= fromDt);
//             pool.Add(x => x.ModifiedAt <= toDt);
//
//             pool.Add(x => x.TradeAccount.IdNavigation
//                     .Groups.Any(g => g.OwnerAccount.Uid == AgentUid
//                                      && g.Type == (int)AccountGroupTypes.Agent),
//                 AgentUid.IsTangible());
//
//             pool.Add(x => x.TradeAccount.IdNavigation
//                     .Groups.Any(g => g.OwnerAccount.Uid == RepUid
//                                      && g.Type == (int)AccountGroupTypes.Rep),
//                 RepUid.IsTangible());
//
//             pool.Add(x => x.TradeAccount.IdNavigation
//                     .Groups.Any(g => g.OwnerAccount.Uid == SalesUid
//                                      && g.Type == (int)AccountGroupTypes.Sales),
//                 SalesUid.IsTangible());
//         }
//
//         public static ReportCriteria Build(DateTime from, DateTime to, ReportPeriodTypes periodType,
//             double timeZoneOffset = 0d, CurrencyTypes? currencyId = null)
//             => new()
//             {
//                 From = from,
//                 To = to,
//                 CurrencyId = currencyId,
//                 PeriodType = periodType,
//                 TimeZoneOffset = timeZoneOffset,
//                 Size = 500,
//             };
//     }
// }