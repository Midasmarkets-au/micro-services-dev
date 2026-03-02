using Bacera.Gateway.Integration;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services.Email.ViewModel;

public class AccountDailyConfirmationViewModel : EmailViewModel, IRazorModel
{
    public bool IsEmpty() => PartyId == 0;
    public override string TemplateTitle { get; } = EmailTemplateTypes.ClientDailyConfirmation;

    public long TenantId { get; set; }
    public long PartyId { get; set; }
    public long AccountId { get; set; }
    public long AccountNumber { get; set; }
    public string NativeName { get; set; } = string.Empty;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime ReportDate { get; set; } = DateTime.Now;
    public long DailyDateTime { get; set; } = 0;

    public List<TradeViewModel> OpenTrades { get; set; } = new();
    public List<TradeViewModel> ClosedTrades { get; set; } = new();

    public TradeSummary OpenTradeSummary { get; set; } = new();
    public TradeSummary ClosedTradeSummary { get; set; } = new();

    public List<TradeViewModel> PendingTrades { get; set; } = new();

    public Summary Summary { get; set; } = new();

    public static AccountDailyConfirmationViewModel Build(
        DateTime reportDate,
        long dailyDateTime,
        long tenantId,
        long partyId,
        long accountNumber,
        long accountId
        , List<TradeViewModel> openTrades
        , List<TradeViewModel> closedTrades
        , TradeSummary openTradeSummary
        , TradeSummary closedTradeSummary
        , List<TradeViewModel> pendingTrades
        , Summary summary)
        => new()
        {
            ReportDate = reportDate,
            DailyDateTime = dailyDateTime,
            TenantId = tenantId,
            PartyId = partyId, AccountNumber = accountNumber, AccountId = accountId,
            OpenTrades = openTrades,
            ClosedTrades = closedTrades,
            OpenTradeSummary = openTradeSummary,
            ClosedTradeSummary = closedTradeSummary,
            PendingTrades = pendingTrades,
            Summary = summary,
            CreatedOn = DateTime.Now,
        };

    public override string ToString() => JsonConvert.SerializeObject(this);
}

public class TradeSummary
{
    public double Deposit { get; set; }
    public double Credit { get; set; }
    public double Commission { get; set; }
    public double Swaps { get; set; }
    public double Profit { get; set; }
    public double TotalProfit { get; set; }
}

public class Summary
{
    public double PrevBalance { get; set; }
    public double Credit { get; set; }
    public double Deposit { get; set; }
    public double Equity { get; set; }
    public double Balance { get; set; }
    public double Margin { get; set; }
    public double Profit { get; set; }
    public double AvailableMargin { get; set; }
}

public static class SummaryExtension
{
    public static IQueryable<Summary> ToMt4Summary(this IQueryable<Mt4User> query)
        => query.Select(x => new Summary
        {
            PrevBalance = x.Prevbalance,
            Balance = x.Balance,
            Credit = x.Credit,
            Equity = x.Equity,
            Margin = x.Margin,
            AvailableMargin = x.MarginFree,
        });

    public static IQueryable<Summary> ToMt5Summary(this IQueryable<Mt5User> query)
        => query.Select(x => new Summary
        {
            PrevBalance = x.BalancePrevDay,
            Balance = x.Balance,
            Credit = x.Credit,
        });
}


/*
public string ToWorkingOrders()
   {
   var color = new[] { "#FFFFFF;", "#EEEEEE;" };
   if (PendingTrades.Count == 0)
   return
   "<tr style=\"background:#EEEEEE; text-align:center\"><td style=\"border:1px solid #cccccc\" colspan=\"10\">無待发单</td></tr>";

   var counter = 0;
   var result = new StringBuilder();
   foreach (var trade in PendingTrades)
   {
   result.Append($"<tr style=\"background:{color[counter++ % 2]}\">");
   result.Append(generateTd(trade.Ticket.ToString()));
   result.Append(generateTd(trade.OpenAt.ToString() ?? string.Empty));
   result.Append(generateTd(trade.Cmd.ToString()));
   result.Append(generateTd(trade.Volume.ToString(CultureInfo.CurrentCulture)));
   result.Append(generateTd(trade.Symbol));
   result.Append(generateTd(trade.OpenPrice?.ToString("F" + trade.Digits) ?? string.Empty));
   result.Append(generateTd(trade.Sl.ToString("F" + trade.Digits)));
   result.Append(generateTd(trade.Tp.ToString("F" + trade.Digits)));
   result.Append(generateTd(trade.ClosePrice?.ToString("F" + trade.Digits) ?? string.Empty));
   result.Append(generateTd(trade.Comment));
   result.Append("</tr>");
   }

   return result.ToString();
   }

   public string ToOpenTrades()
   {
   if (OpenTrades.Count == 0)
   return
   "<tr style=\"background:#FFFFFF; text-align:center\"><td style=\"border:1px solid #cccccc\" colspan=\"13\">无平仓单</td></tr>";

   var color = new[] { "#FFFFFF;", "#EEEEEE;" };
   var counter = 0;
   var result = new StringBuilder();

   foreach (var trade in OpenTrades)
   {
   result.Append($"<tr style=\"background:{color[counter++ % 2]}\">");
   result.Append(generateTd(""));
   result.Append(generateTd(trade.OpenAt.ToString() ?? string.Empty));
   result.Append(generateTd(trade.Cmd.ToString()));

   switch (trade.Cmd)
   {
   case < 2:
   result.Append(generateTd(trade.Volume.ToString(CultureInfo.CurrentCulture)));
   result.Append(generateTd(trade.Symbol));
   result.Append(generateTd(trade.OpenPrice?.ToString("F" + trade.Digits) ?? string.Empty));
   result.Append(generateTd(trade.Sl.ToString("F" + trade.Digits)));
   result.Append(generateTd(trade.Tp.ToString("F" + trade.Digits)));
   result.Append(generateTd(trade.CloseAt?.ToString() ?? string.Empty));
   result.Append(generateTd(trade.ClosePrice?.ToString("F" + trade.Digits) ?? string.Empty));
   result.Append(generateTd(trade.Commission.ToString("F2")));
   result.Append(generateTd(trade.Swaps.ToString("F2")));
   break;
   case > 5:
   result.Append(
   $"<td style=\"border:1px solid #cccccc\" colspan=\"9\" align=\"right\">{trade.Comment}</td>");
   break;
   }

   result.Append(generateTd(trade.Profit.ToString("F2")));
   result.Append("</tr>");
   }

   result.Append("<tr style=\"background:#EEEEEE;\">");
   result.Append("<td style=\"border:1px solid #cccccc\" colspan=\"10\"></td>");
   result.Append(generateTd(OpenTradeSummary.Commission.ToString("F2")));
   result.Append(generateTd(OpenTradeSummary.Swaps.ToString("F2")));
   result.Append(generateTd(OpenTradeSummary.Profit.ToString("F2")));
   result.Append("</tr>");

   return result.ToString();
   }

   public string ToOpenTradeSummaries()
   {
   var result = new StringBuilder();
   foreach (var summary in OpenTradeSummaries)
   {
   result.Append("<tr>");
   result.Append("<td><strong>&nbsp;</strong></td>");
   result.Append($"<td><strong>信用:{summary.Credit.ToString("F2")}</strong></td>");
   result.Append(
   $"<td align=\"right\">  <strong>浮动盈亏:&nbsp;&nbsp;&nbsp;&nbsp; {summary.Profit.ToString("F2")}</strong></td>");
   result.Append("</tr>");
   }

   return result.ToString();
   }

   public string ToClosedTrades()
   {
   if (ClosedTrades.Count == 0)
   return
   "<tr style=\"background:#FFFFFF; text-align:center\"><td style=\"border:1px solid #cccccc\" colspan=\"13\">无平仓单</td></tr>";

   var color = new[] { "#FFFFFF;", "#EEEEEE;" };
   var counter = 0;
   var result = new StringBuilder();

   foreach (var trade in ClosedTrades)
   {
   result.Append($"<tr style=\"background:{color[counter++ % 2]}\">");
   result.Append(generateTd(trade.Ticket.ToString()));
   result.Append(generateTd(trade.OpenAt.ToString() ?? string.Empty));
   result.Append(generateTd(trade.Cmd.ToString()));

   switch (trade.Cmd)
   {
   case < 2:
   result.Append(generateTd(trade.Volume.ToString(CultureInfo.CurrentCulture)));
   result.Append(generateTd(trade.Symbol));
   result.Append(generateTd(trade.OpenPrice?.ToString("F" + trade.Digits) ?? string.Empty));
   result.Append(generateTd(trade.Sl.ToString("F" + trade.Digits)));
   result.Append(generateTd(trade.Tp.ToString("F" + trade.Digits)));
   result.Append(generateTd(trade.CloseAt?.ToString() ?? string.Empty));
   result.Append(generateTd(trade.ClosePrice?.ToString("F" + trade.Digits) ?? string.Empty));
   result.Append(generateTd(trade.Commission.ToString("F2")));
   result.Append(generateTd(trade.Swaps.ToString("F2")));
   break;
   case > 5:
   result.Append(
   $"<td style=\"border:1px solid #cccccc\" colspan=\"9\" align=\"right\">{trade.Comment}</td>");
   break;
   }

   result.Append(generateTd(trade.Profit.ToString("F2")));
   result.Append("</tr>");
   }

   result.Append("<tr style=\"background:#EEEEEE;\">");
   result.Append("<td style=\"border:1px solid #cccccc\" colspan=\"10\"></td>");
   result.Append(generateTd(ClosedTradeSummary.Commission.ToString("F2")));
   result.Append(generateTd(ClosedTradeSummary.Swaps.ToString("F2")));
   result.Append(generateTd(ClosedTradeSummary.Profit.ToString("F2")));
   result.Append("</tr>");

   return result.ToString();
   }

   public string ToClosedTradeSummaries()
   {
   var result = new StringBuilder();
   foreach (var summary in ClosedTradeSummaries)
   {
   result.Append("<tr>");
   result.Append($"<td><strong>出入金:{summary.Deposit.ToString("F2")}</strong></td>");
   result.Append($"<td><strong>信用:{summary.Credit.ToString("F2")}</strong></td>");
   result.Append(
   $"<td align=\"right\"><strong>平仓盈亏:&nbsp;&nbsp;&nbsp;&nbsp;{summary.Profit.ToString("F2")}</strong></td>");
   result.Append("</tr>");
   }

   return result.ToString();
   }

   private static string generateTd(string value) => $"<td style=\"border:1px solid #cccccc\">{value}</td>";
*/