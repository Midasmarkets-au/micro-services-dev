// using Bacera.Gateway;
//
// namespace Bacera.Gateway;
//
// partial class TradeTransaction
// {
//     public bool RebateQualified()
//         => Status == 0
//            && CloseAt > new DateTime(1970, 1, 1) // greater then 1970
//            && Cmd is >= 0 and <= 5 // CMD between 0 and 5
//     ;
//
//     public bool RebateUnqualified()
//         => Cmd is < 0 or > 5 // CMD between 0 and 5
//     ;
//
//     public class ClientResponseModel
//     {
//         public long Id { get; set; }
//         public long AccountNumber { get; set; }
//
//         public int ServiceId { get; set; }
//
//         public long Ticket { get; set; }
//
//         public string Symbol { get; set; } = null!;
//
//         public int Digits { get; set; }
//
//         public int Cmd { get; set; }
//
//         public int Volume { get; set; }
//
//         public DateTime OpenAt { get; set; }
//
//         public double OpenPrice { get; set; }
//
//         public double Sl { get; set; }
//
//         public double Tp { get; set; }
//
//         public DateTime? CloseAt { get; set; }
//
//         public double ClosePrice { get; set; }
//
//         public double Swaps { get; set; }
//
//         public double Profit { get; set; }
//
//         public double Taxes { get; set; }
//         public double Commission { get; set; }
//
//         public string Comment { get; set; } = null!;
//
//         public CurrencyTypes CurrencyId { get; set; } = CurrencyTypes.Invalid;
//
//         public DateTime ModifiedAt { get; set; }
//     }
//
//     public class SummaryResponseModel
//     {
//         public long Id { get; set; }
//         public long AccountNumber { get; set; }
//         public long AccountUid { get; set; }
//         public string AccountName { get; set; } = string.Empty;
//         public long Ticket { get; set; }
//         public string Symbol { get; set; } = null!;
//         public int Volume { get; set; }
//         public DateTime? CloseAt { get; set; }
//     }
// }
//
// public static class TradeTransactionExit
// {
//     public static IQueryable<TradeTransaction.ClientResponseModel> ToClientResponseModels(
//         this IQueryable<TradeTransaction> query)
//         => query
//             .Select(x => new TradeTransaction.ClientResponseModel
//             {
//                 Id = x.Id,
//                 AccountNumber = x.AccountNumber,
//                 CloseAt = x.CloseAt,
//                 ClosePrice = x.ClosePrice,
//                 Cmd = x.Cmd,
//                 Comment = x.Comment,
//                 Commission = x.Commission,
//                 CurrencyId = (CurrencyTypes)x.TradeAccount.CurrencyId,
//                 Digits = x.Digits,
//                 ModifiedAt = x.ModifiedAt,
//                 OpenAt = x.OpenAt,
//                 OpenPrice = x.OpenPrice,
//                 Profit = x.Profit,
//                 ServiceId = x.ServiceId,
//                 Sl = x.Sl,
//                 Swaps = x.Swaps,
//                 Symbol = x.Symbol,
//                 Taxes = x.Taxes,
//                 Ticket = x.Ticket,
//                 Tp = x.Tp,
//                 Volume = x.Volume,
//             });
//
//     public static IQueryable<TradeTransaction.SummaryResponseModel> ToSummaryResponseModels(
//         this IQueryable<TradeTransaction> query)
//         => query
//             .Select(x => new TradeTransaction.SummaryResponseModel
//             {
//                 Id = x.Id,
//                 AccountNumber = x.AccountNumber,
//                 AccountUid = x.TradeAccount.IdNavigation.Uid,
//                 AccountName = x.TradeAccount.IdNavigation.Name,
//                 CloseAt = x.CloseAt,
//                 Symbol = x.Symbol,
//                 Ticket = x.Ticket,
//                 Volume = x.Volume,
//             });
// }