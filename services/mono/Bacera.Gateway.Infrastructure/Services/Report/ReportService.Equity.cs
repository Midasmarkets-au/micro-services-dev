using System.Linq.Expressions;
using Bacera.Gateway.Integration;
using Bacera.Gateway.Services.Report.Models;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services;

partial class ReportService
{
    private async Task<Expression<Func<Account, bool>>> BuildExpressionTreeEquityReport(
        ReportConfigurationItem item)
    {
        if (item.AccountNumbers != null && item.AccountNumbers.Any())
            return PredicateBuilder.New<Account>(false).And(x => item.AccountNumbers.Contains(x.AccountNumber));

        var includeEndWithCodes = item.IncludeSalesCode
            .Where(x => x.EndsWith("%"))
            .Select(x => x.Replace("%", "").Trim().ToUpper())
            .ToList();
        var includeCodes = item.IncludeSalesCode
            .Where(x => !x.EndsWith("%"))
            .Select(x => x.Trim().ToUpper())
            .ToList();

        foreach (var subCode in includeEndWithCodes)
        {
            var li = await tenantDbContext.Accounts
                .Where(x => x.Role == (short)AccountRoleTypes.Sales)
                .Where(x => x.Code.StartsWith(subCode))
                .Select(x => x.Code)
                .ToListAsync();
            includeCodes.AddRange(li);
        }

        var excludeEndWithReferCodes = item.ExcludeSalesCode
            .Where(x => x.EndsWith("%"))
            .Select(x => x.Replace("%", "").Trim().ToUpper())
            .ToList();

        var excludeReferCodes = item.ExcludeSalesCode
            .Where(x => !x.EndsWith("%"))
            .Select(x => x.Trim().ToUpper())
            .ToList();

        foreach (var subCode in excludeEndWithReferCodes)
        {
            var li = await tenantDbContext.Accounts
                .Where(x => x.Role == (short)AccountRoleTypes.Sales)
                .Where(x => x.Code.StartsWith(subCode))
                .Select(x => x.Code)
                .ToListAsync();
            excludeReferCodes.AddRange(li);
        }

        includeCodes = includeCodes.Distinct().ToList();
        excludeReferCodes = excludeReferCodes.Distinct().ToList();

        var salesReferPaths = await tenantDbContext.Accounts
            .Where(x => x.Role == (short)AccountRoleTypes.Sales)
            .Where(x => includeCodes.Contains(x.Code.ToUpper()))
            .Where(x => !excludeReferCodes.Contains(x.Code.ToUpper()))
            .Select(x => x.ReferPath)
            .ToListAsync();
        if (salesReferPaths.Count == 0)
        {
            salesReferPaths.Add(".Unknown.Unknown");
        }

        var referPathPredict = PredicateBuilder.New<Account>(false);
        referPathPredict = salesReferPaths.Aggregate(referPathPredict,
            (current, code) => current.Or(x => x.ReferPath.ToUpper().StartsWith(code)));

        var includeGroup = PredicateBuilder.New<Account>();
        foreach (var term in item.IncludeGroupStartWith)
        {
            includeGroup = includeGroup.Or(x => x.Group.StartsWith(term));
        }

        // Initialize an exclusive predicate for group starting with specific terms
        var excludeGroup = PredicateBuilder.New<Account>();
        foreach (var term in item.ExcludeGroupStartWith)
        {
            excludeGroup = excludeGroup.And(x => x.Group.StartsWith(term) == false);
        }

        // Initialize an inclusive predicate for account numbers
        var includeLogin = PredicateBuilder.New<Account>();
        if (item.IncludeAccountNumber.Any())
        {
            includeLogin = includeLogin.Or(x => item.IncludeAccountNumber.Contains(x.AccountNumber));
        }

        // Initialize an exclusive predicate for account numbers
        var excludeLogin = PredicateBuilder.New<Account>();
        foreach (var term in item.ExcludeAccountNumber)
        {
            excludeLogin = excludeLogin.And(x => x.AccountNumber != term);
        }

        // Initialize an inclusive predicate for account Types
        var includeAccountType = PredicateBuilder.New<Account>();
        if (item.IncludeAccountType.HasValue)
        {
            var type = (short)item.IncludeAccountType.Value;
            includeAccountType = includeAccountType.And(x => x.Type == type);
        }

        // Initialize an exclusive predicate for account Types
        var excludeAccountType = PredicateBuilder.New<Account>();
        if (item.ExcludeAccountType.HasValue)
        {
            var type = (short)item.ExcludeAccountType.Value;
            excludeAccountType = excludeAccountType.And(x => x.Type != type);
        }

        // Combine the predicates to build the final predicate
        var final = PredicateBuilder.New<Account>();
        // Initialize a predicate for currency ID
        var hasCurrencyId = PredicateBuilder.New<Account>(false);
        if ((int)item.CurrencyId > 0)
        {
            // Add an AND condition to the predicate if a currency ID is specified
            hasCurrencyId = hasCurrencyId.And(x => x.CurrencyId == (int)item.CurrencyId);
        }

        if (referPathPredict.IsStarted)
        {
            final = final.And(referPathPredict);
        }

        // Combine the predicates to build the final predicate
        if (hasCurrencyId.IsStarted)
        {
            final = final.And(hasCurrencyId);
        }

        if (includeGroup.IsStarted)
        {
            final = final.And(includeGroup);
        }

        if (excludeGroup.IsStarted)
        {
            final = final.And(excludeGroup);
        }

        if (includeLogin.IsStarted)
        {
            final = final.Or(includeLogin);
        }

        if (excludeLogin.IsStarted)
        {
            final = final.And(excludeLogin);
        }

        if (includeAccountType.IsStarted)
        {
            final = final.And(includeAccountType);
        }

        if (excludeAccountType.IsStarted)
        {
            final = final.And(excludeAccountType);
        }

        var sql = final.ToString();
        //query = query.Where(predicateFinal);
        return final;
    }

    private async Task<Expression<Func<Account, bool>>> BuildExpressionTreeEquityReport(TenantDbContext tenantDbContext,
        ReportConfigurationItem item)
    {
        if (item.AccountNumbers != null && item.AccountNumbers.Any())
            return PredicateBuilder.New<Account>(false).And(x => item.AccountNumbers.Contains(x.AccountNumber));

        var includeEndWithCodes = item.IncludeSalesCode
            .Where(x => x.EndsWith("%"))
            .Select(x => x.Replace("%", "").Trim().ToUpper())
            .ToList();
        var includeCodes = item.IncludeSalesCode
            .Where(x => !x.EndsWith("%"))
            .Select(x => x.Trim().ToUpper())
            .ToList();

        foreach (var subCode in includeEndWithCodes)
        {
            var li = await tenantDbContext.Accounts
                .Where(x => x.Role == (short)AccountRoleTypes.Sales)
                .Where(x => x.Code.StartsWith(subCode))
                .Select(x => x.Code)
                .ToListAsync();
            includeCodes.AddRange(li);
        }

        var excludeEndWithReferCodes = item.ExcludeSalesCode
            .Where(x => x.EndsWith("%"))
            .Select(x => x.Replace("%", "").Trim().ToUpper())
            .ToList();

        var excludeReferCodes = item.ExcludeSalesCode
            .Where(x => !x.EndsWith("%"))
            .Select(x => x.Trim().ToUpper())
            .ToList();

        foreach (var subCode in excludeEndWithReferCodes)
        {
            var li = await tenantDbContext.Accounts
                .Where(x => x.Role == (short)AccountRoleTypes.Sales)
                .Where(x => x.Code.StartsWith(subCode))
                .Select(x => x.Code)
                .ToListAsync();
            excludeReferCodes.AddRange(li);
        }

        includeCodes = includeCodes.Distinct().ToList();
        excludeReferCodes = excludeReferCodes.Distinct().ToList();

        var salesReferPaths = await tenantDbContext.Accounts
            .Where(x => x.Role == (short)AccountRoleTypes.Sales)
            .Where(x => includeCodes.Contains(x.Code.ToUpper()))
            .Where(x => !excludeReferCodes.Contains(x.Code.ToUpper()))
            .Select(x => x.ReferPath)
            .ToListAsync();
        if (salesReferPaths.Count == 0)
        {
            salesReferPaths.Add(".Unknown.Unknown");
        }

        var referPathPredict = PredicateBuilder.New<Account>(false);
        referPathPredict = salesReferPaths.Aggregate(referPathPredict,
            (current, code) => current.Or(x => x.ReferPath.ToUpper().StartsWith(code)));

        var includeGroup = PredicateBuilder.New<Account>();
        foreach (var term in item.IncludeGroupStartWith)
        {
            includeGroup = includeGroup.Or(x => x.Group.StartsWith(term));
        }

        // Initialize an exclusive predicate for group starting with specific terms
        var excludeGroup = PredicateBuilder.New<Account>();
        foreach (var term in item.ExcludeGroupStartWith)
        {
            excludeGroup = excludeGroup.And(x => x.Group.StartsWith(term) == false);
        }

        // Initialize an inclusive predicate for account numbers
        var includeLogin = PredicateBuilder.New<Account>();
        if (item.IncludeAccountNumber.Any())
        {
            includeLogin = includeLogin.Or(x => item.IncludeAccountNumber.Contains(x.AccountNumber));
        }

        // Initialize an exclusive predicate for account numbers
        var excludeLogin = PredicateBuilder.New<Account>();
        foreach (var term in item.ExcludeAccountNumber)
        {
            excludeLogin = excludeLogin.And(x => x.AccountNumber != term);
        }

        // Initialize an inclusive predicate for account Types
        var includeAccountType = PredicateBuilder.New<Account>();
        if (item.IncludeAccountType.HasValue)
        {
            var type = (short)item.IncludeAccountType.Value;
            includeAccountType = includeAccountType.And(x => x.Type == type);
        }

        // Initialize an exclusive predicate for account Types
        var excludeAccountType = PredicateBuilder.New<Account>();
        if (item.ExcludeAccountType.HasValue)
        {
            var type = (short)item.ExcludeAccountType.Value;
            excludeAccountType = excludeAccountType.And(x => x.Type != type);
        }

        // Combine the predicates to build the final predicate
        var final = PredicateBuilder.New<Account>();
        // Initialize a predicate for currency ID
        var hasCurrencyId = PredicateBuilder.New<Account>(false);
        if ((int)item.CurrencyId > 0)
        {
            // Add an AND condition to the predicate if a currency ID is specified
            hasCurrencyId = hasCurrencyId.And(x => x.CurrencyId == (int)item.CurrencyId);
        }

        if (referPathPredict.IsStarted)
        {
            final = final.And(referPathPredict);
        }

        // Combine the predicates to build the final predicate
        if (hasCurrencyId.IsStarted)
        {
            final = final.And(hasCurrencyId);
        }

        if (includeGroup.IsStarted)
        {
            final = final.And(includeGroup);
        }

        if (excludeGroup.IsStarted)
        {
            final = final.And(excludeGroup);
        }

        if (includeLogin.IsStarted)
        {
            final = final.Or(includeLogin);
        }

        if (excludeLogin.IsStarted)
        {
            final = final.And(excludeLogin);
        }

        if (includeAccountType.IsStarted)
        {
            final = final.And(includeAccountType);
        }

        if (excludeAccountType.IsStarted)
        {
            final = final.And(excludeAccountType);
        }

        var sql = final.ToString();
        //query = query.Where(predicateFinal);
        return final;
    }


    public async Task<List<long>> GetLogins(ReportConfigurationItem item, int serviceId)
    {
        var tenantDbContext = await myDbContextPool.BorrowTenant(tenantGetter.GetTenantId());
        var mt4Ctx = await myDbContextPool.BorrowCentralMT4Async(serviceId);
        try
        {
            var expression = await BuildExpressionTreeEquityReport(tenantDbContext, item);
            var loginQuery = tenantDbContext.Accounts.Where(expression);
            var logins = await loginQuery.Select(x => x.AccountNumber).Distinct().ToListAsync();
            var loginsInInt = logins.Select(x => (int)x).ToList();
            logins = await mt4Ctx.Mt4Users
                .Where(x => loginsInInt.Contains(x.Login))
                .Where(x => !x.Group.Contains("OFF"))
                .Select(x => (long)x.Login)
                .ToListAsync();
            return logins;
        }
        finally
        {
            myDbContextPool.ReturnCentralMT4(mt4Ctx);
            myDbContextPool.ReturnTenant(tenantDbContext);
        }
    }

    public async Task<List<MetaTrade4EquityReport>> GenerateMT4EquityDailyReport(int serviceId,
        ReportConfiguration configuration)
    {
        var from = configuration.From.Date;
        var to = from.AddDays(1);

        var fromForLots = from.AddHours(Utils.IsCurrentDSTLosAngeles(from) ? 21 : 22);
        var toForLots = to.AddHours(Utils.IsCurrentDSTLosAngeles(to) ? 21 : 22);

        var previousDay = GetLastWorkDay(from);
        var tasks = configuration.Items.Select(async item =>
        {
            var logins = await GetLogins(item, serviceId);

            var task = new
            {
                item.Name,
                item.Group,
                PreviousEquity = SumEquity(serviceId, logins, previousDay, previousDay.AddDays(1)),
                Equity = SumEquity(serviceId, logins, from, to),
                Deposit = SumDeposit(serviceId, logins, fromForLots, toForLots),
                Withdraw = SumWithdraw(serviceId, logins, fromForLots, toForLots),
                Adjust = SumAdjust(serviceId, logins, fromForLots, toForLots),
                Credit = SumCredit(serviceId, logins, fromForLots, toForLots),
                Agent = SumAgentFee(serviceId, logins, fromForLots, toForLots),
                Transfer = SumTransfer(serviceId, logins, fromForLots, toForLots),
                NewAccount = CountNewAccount(serviceId, logins, fromForLots, toForLots),

                Oil = SumOilVolume(serviceId, logins, fromForLots, toForLots),
                Forex = SumForexVolume(serviceId, logins, fromForLots, toForLots),
                Metal = SumPmVolume(serviceId, logins, fromForLots, toForLots),
                Other = SumOtherVolume(serviceId, logins, fromForLots, toForLots),
                Lots = SumTotalVolume(serviceId, logins, fromForLots, toForLots),
            };

            var report = new MetaTrade4EquityReport
            {
                Name = task.Name,
                Group = task.Group,
                PreviousEquity = await task.PreviousEquity,
                Equity = await task.Equity,
                Deposit = await task.Deposit,
                Withdraw = await task.Withdraw,
                Adjust = await task.Adjust,
                Credit = await task.Credit,
                Agent = await task.Agent,
                Transfer = await task.Transfer,
                NewAccount = await task.NewAccount,
                Oil = await task.Oil,
                Forex = await task.Forex,
                Metal = await task.Metal,
                Other = await task.Other,
                Lots = await task.Lots,
            };
            return report;
        });
        var reports = await Task.WhenAll(tasks);
        return reports.ToList();
    }


    public async Task<List<MetaTrade4EquityReport>> GenerateMT4EquityMonthlyReport(int serviceId,
        ReportConfiguration configuration)
    {
        var date = new DateTime(configuration.From.Year, configuration.From.Month, 1);
        var from = date.Date;

        var currentDate = new DateTime(configuration.From.Year, configuration.From.Month, configuration.From.Day);
        var currentDay = currentDate.Date;
        var to = currentDate.AddDays(1);

        var fromForLots = from.AddHours(Utils.IsCurrentDSTLosAngeles(from) ? 21 : 22);
        var toForLots = to.AddHours(Utils.IsCurrentDSTLosAngeles(to) ? 21 : 22);

        var previousMonthFirstDay = from.AddDays(-1);
        var tasks = configuration.Items.Select(async item =>
        {
            var logins = await GetLogins(item, serviceId);
            var tasks = new
            {
                item.Name,
                item.Group,
                PreviousEquity = SumEquity(serviceId, logins, previousMonthFirstDay,
                    previousMonthFirstDay.AddDays(1)),
                Equity = SumEquity(serviceId, logins, currentDay, currentDay.AddDays(1)),

                Deposit = SumDeposit(serviceId, logins, fromForLots, toForLots),
                Withdraw = SumWithdraw(serviceId, logins, fromForLots, toForLots),
                Adjust = SumAdjust(serviceId, logins, fromForLots, toForLots),
                Credit = SumCredit(serviceId, logins, fromForLots, toForLots),
                Agent = SumAgentFee(serviceId, logins, fromForLots, toForLots),
                Transfer = SumTransfer(serviceId, logins, fromForLots, toForLots),
                NewAccount = CountNewAccount(serviceId, logins, fromForLots, toForLots),

                Oil = SumOilVolume(serviceId, logins, fromForLots, toForLots),
                Forex = SumForexVolume(serviceId, logins, fromForLots, toForLots),
                Metal = SumPmVolume(serviceId, logins, fromForLots, toForLots),
                Other = SumOtherVolume(serviceId, logins, fromForLots, toForLots),
                Lots = SumTotalVolume(serviceId, logins, fromForLots, toForLots),
            };

            var report = new MetaTrade4EquityReport
            {
                Name = tasks.Name,
                Group = tasks.Group,
                PreviousEquity = await tasks.PreviousEquity,
                Equity = await tasks.Equity,
                Deposit = await tasks.Deposit,
                Withdraw = await tasks.Withdraw,
                Adjust = await tasks.Adjust,
                Credit = await tasks.Credit,
                Agent = await tasks.Agent,
                Transfer = await tasks.Transfer,
                NewAccount = await tasks.NewAccount,
                Oil = await tasks.Oil,
                Forex = await tasks.Forex,
                Metal = await tasks.Metal,
                Other = await tasks.Other,
                Lots = await tasks.Lots,
            };
            return report;
        });
        var reports = await Task.WhenAll(tasks);
        return reports.ToList();
    }

    private static DateTime GetLocalDateTime(DateTime utcDateTime, TimeZoneInfo timeZone)
    {
        utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        var time = TimeZoneInfo.ConvertTime(utcDateTime, timeZone);
        return time;
    }

    public string GenerateMT4EquityReportHtml(string title, List<MetaTrade4EquityReport> dailyReports,
        List<MetaTrade4EquityReport> monthlyReports)
    {
        var html = HtmlHeader(title + " Daily Equity Report");
        foreach (var item in dailyReports.GroupBy(x => x.Group))
        {
            foreach (var line in item)
            {
                html += HtmlLineItem(line);
            }

            var total = new MetaTrade4EquityReport()
            {
                Name = "Total",
                Group = "",
                NewAccount = item.Sum(x => x.NewAccount),
                PreviousEquity = item.Sum(x => x.PreviousEquity),
                Equity = item.Sum(x => x.Equity),
                Deposit = item.Sum(x => x.Deposit),
                Withdraw = item.Sum(x => x.Withdraw),
                Transfer = item.Sum(x => x.Transfer),
                Credit = item.Sum(x => x.Credit),
                Adjust = item.Sum(x => x.Adjust),
                Agent = item.Sum(x => x.Agent),
                Forex = item.Sum(x => x.Forex),
                Metal = item.Sum(x => x.Metal),
                Oil = item.Sum(x => x.Oil),
                Other = item.Sum(x => x.Other),
                Lots = item.Sum(x => x.Lots)
            };
            html += HtmlSubtitle(total);
        }

        html += HtmlFooter();

        html += HtmlHeader(title + " Monthly Equity Report");
        foreach (var item in monthlyReports.GroupBy(x => x.Group))
        {
            foreach (var line in item)
            {
                html += HtmlLineItem(line);
            }

            var total = new MetaTrade4EquityReport()
            {
                Name = "Total",
                Group = "",
                NewAccount = item.Sum(x => x.NewAccount),
                PreviousEquity = item.Sum(x => x.PreviousEquity),
                Equity = item.Sum(x => x.Equity),
                Deposit = item.Sum(x => x.Deposit),
                Withdraw = item.Sum(x => x.Withdraw),
                Transfer = item.Sum(x => x.Transfer),
                Credit = item.Sum(x => x.Credit),
                Adjust = item.Sum(x => x.Adjust),
                Agent = item.Sum(x => x.Agent),
                Forex = item.Sum(x => x.Forex),
                Metal = item.Sum(x => x.Metal),
                Oil = item.Sum(x => x.Oil),
                Other = item.Sum(x => x.Other),
                Lots = item.Sum(x => x.Lots)
            };
            html += HtmlSubtitle(total);
        }

        html += HtmlFooter();
        return html;
    }


    private static string HtmlFooter() => "</table>";

    private static DateTime GetLastWorkDay(DateTime date)
    {
        do
        {
            date = date.AddDays(-1);
        } while (IsWeekend(date));

        return date;
    }

    private static bool IsWeekend(DateTime date)
        => date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    private const int PageSize = 2000;

    private async Task<double> SumEquity(int serviceId, ICollection<long> logins, DateTime from,
        DateTime to)
    {
        var ctx = await myDbContextPool.BorrowCentralMT4Async(serviceId);
        try
        {
            var sum = 0d;
            var page = 0;
            while (true)
            {
                var ids = logins.Skip(page * PageSize).Take(PageSize).ToList();
                if (!ids.Any())
                    break;
                page++;

                sum += await ctx.Mt4Dailies
                    .Where(x => x.Time > from)
                    .Where(x => x.Time <= to)
                    .Where(x => ids.Contains(x.Login))
                    .SumAsync(x => x.Equity);
            }

            return sum;
        }
        finally
        {
            myDbContextPool.ReturnCentralMT4(ctx);
        }
    }

    private async Task<double> SumPmVolume(int serviceId, ICollection<long> logins, DateTime from,
        DateTime to)
    {
        var ctx = await myDbContextPool.BorrowCentralMT4Async(serviceId);
        try
        {
            var sum = 0;
            var page = 0;
            while (true)
            {
                var ids = logins.Skip(page * PageSize).Take(PageSize).ToList();
                if (!ids.Any())
                    break;
                page++;

                sum += await ctx.Mt4Trades
                    .Where(x => x.CloseTime >= from)
                    .Where(x => x.CloseTime < to)
                    .Where(x => ids.Contains(x.Login))
                    .Where(x => x.Cmd < 2)
                    .Where(x => x.Symbol.Contains("GOLD") ||
                                x.Symbol.Contains("SILVER") ||
                                x.Symbol.StartsWith("XAGUSD") ||
                                x.Symbol.StartsWith("XAUUSD"))
                    .SumAsync(x => x.Volume);
            }

            return sum;
        }
        finally
        {
            myDbContextPool.ReturnCentralMT4(ctx);
        }
    }

    private async Task<double> SumForexVolume(int serviceId, ICollection<long> logins, DateTime from,
        DateTime to)
    {
        var ctx = await myDbContextPool.BorrowCentralMT4Async(serviceId);
        try
        {
            var sum = 0d;
            var page = 0;
            while (true)
            {
                var ids = logins.Skip(page * PageSize).Take(PageSize).ToList();
                if (!ids.Any())
                    break;
                page++;

                sum += await ctx.Mt4Trades
                    .Where(x => x.CloseTime >= from)
                    .Where(x => x.CloseTime < to)
                    .Where(x => ids.Contains(x.Login))
                    .Where(x => x.Cmd < 2)
                    .Where(x => !x.Symbol.StartsWith("GOLD")
                                && !x.Symbol.StartsWith("SILVER")
                                && !x.Symbol.StartsWith("#")
                                && !x.Symbol.StartsWith("XAGUSD")
                                && !x.Symbol.StartsWith("XAUUSD")
                                && !x.Symbol.StartsWith("XTIUSD")
                    )
                    .SumAsync(x => x.Volume);
            }

            return sum;
        }
        finally
        {
            myDbContextPool.ReturnCentralMT4(ctx);
        }
    }

    private async Task<double> SumOilVolume(int serviceId, ICollection<long> logins, DateTime from,
        DateTime to)
    {
        var ctx = await myDbContextPool.BorrowCentralMT4Async(serviceId);
        try
        {
            var sum = 0d;
            var page = 0;
            while (true)
            {
                var ids = logins.Skip(page * PageSize).Take(PageSize).ToList();
                if (!ids.Any())
                    break;
                page++;

                sum += await ctx.Mt4Trades
                    .Where(x => x.CloseTime >= from)
                    .Where(x => x.CloseTime < to)
                    .Where(x => ids.Contains(x.Login))
                    .Where(x => x.Cmd < 2)
                    .Where(x => x.Symbol.StartsWith("XTIUSD") || x.Symbol.StartsWith("#CL"))
                    .SumAsync(x => x.Volume);
            }

            return sum;
        }
        finally
        {
            myDbContextPool.ReturnCentralMT4(ctx);
        }
    }

    private async Task<double> SumOtherVolume(int serviceId, ICollection<long> logins, DateTime from,
        DateTime to)
    {
        var ctx = await myDbContextPool.BorrowCentralMT4Async(serviceId);
        try
        {
            var sum = 0d;
            var page = 0;
            while (true)
            {
                var ids = logins.Skip(page * PageSize).Take(PageSize).ToList();
                if (!ids.Any())
                    break;
                page++;

                sum += await ctx.Mt4Trades
                    .Where(x => x.CloseTime >= from)
                    .Where(x => x.CloseTime < to)
                    .Where(x => ids.Contains(x.Login))
                    .Where(x => x.Cmd < 2)
                    .Where(x => x.Symbol.StartsWith("#") && !x.Symbol.StartsWith("#CL"))
                    .SumAsync(x => x.Volume);
            }

            return sum;
        }
        finally
        {
            myDbContextPool.ReturnCentralMT4(ctx);
        }
    }

    private async Task<double> SumTotalVolume(int serviceId, ICollection<long> logins, DateTime from,
        DateTime to)
    {
        var ctx = await myDbContextPool.BorrowCentralMT4Async(serviceId);
        try
        {
            var sum = 0d;
            var page = 0;
            while (true)
            {
                var ids = logins.Skip(page * PageSize).Take(PageSize).ToList();
                if (!ids.Any())
                    break;
                page++;

                sum += await ctx.Mt4Trades
                    .Where(x => x.CloseTime >= from)
                    .Where(x => x.CloseTime < to)
                    .Where(x => ids.Contains(x.Login))
                    .Where(x => x.Cmd < 2)
                    .SumAsync(x => x.Volume);
            }

            return sum;
        }
        finally
        {
            myDbContextPool.ReturnCentralMT4(ctx);
        }
    }

    private async Task<double> SumDeposit(int serviceId, ICollection<long> logins, DateTime from,
        DateTime to)
    {
        var ctx = await myDbContextPool.BorrowCentralMT4Async(serviceId);
        try
        {
            var sum = 0d;
            var page = 0;
            while (true)
            {
                var ids = logins.Skip(page * PageSize).Take(PageSize).ToList();
                if (!ids.Any())
                    break;
                page++;

                sum += await ctx.Mt4Trades
                    .Where(x => ids.Contains(x.Login))
                    .Where(x => x.Cmd == 6)
                    .Where(x => x.CloseTime > from)
                    .Where(x => x.CloseTime <= to)
                    .Where(x => x.Comment.Contains("deposit")
                                || (x.Comment.Contains("margin") && x.Comment.Contains("in")))
                    .SumAsync(x => x.Profit);
            }

            return sum;
        }
        finally
        {
            myDbContextPool.ReturnCentralMT4(ctx);
        }
    }

    private async Task<double> SumWithdraw(int serviceId, ICollection<long> logins, DateTime from,
        DateTime to)
    {
        var ctx = await myDbContextPool.BorrowCentralMT4Async(serviceId);
        try
        {
            var sum = 0d;
            var page = 0;
            while (true)
            {
                var ids = logins.Skip(page * PageSize).Take(PageSize).ToList();
                if (!ids.Any())
                    break;
                page++;

                sum += await ctx.Mt4Trades
                    .Where(x => ids.Contains(x.Login))
                    .Where(x => x.Cmd == 6)
                    .Where(x => x.CloseTime > from)
                    .Where(x => x.CloseTime <= to)
                    .Where(x => x.Comment.Contains("withdraw")
                                || (x.Comment.Contains("margin") && x.Comment.Contains("out")))
                    .SumAsync(x => x.Profit);
            }

            return sum;
        }
        finally
        {
            myDbContextPool.ReturnCentralMT4(ctx);
        }
    }

    private async Task<double> SumTransfer(int serviceId, ICollection<long> logins, DateTime from,
        DateTime to)
    {
        var ctx = await myDbContextPool.BorrowCentralMT4Async(serviceId);
        try
        {
            var sum = 0d;
            var page = 0;
            while (true)
            {
                var ids = logins.Skip(page * PageSize).Take(PageSize).ToList();
                if (!ids.Any())
                    break;
                page++;

                sum += await ctx.Mt4Trades
                    .Where(x => ids.Contains(x.Login))
                    .Where(x => x.Cmd == 6)
                    .Where(x => x.CloseTime > from)
                    .Where(x => x.CloseTime <= to)
                    .Where(x => x.Comment.Contains("trans"))
                    .SumAsync(x => x.Profit);
            }

            return sum;
        }
        finally
        {
            myDbContextPool.ReturnCentralMT4(ctx);
        }
    }

    private async Task<double> SumAdjust(int serviceId, ICollection<long> logins, DateTime from,
        DateTime to)
    {
        var ctx = await myDbContextPool.BorrowCentralMT4Async(serviceId);
        try
        {
            var sum = 0d;
            var page = 0;
            while (true)
            {
                var ids = logins.Skip(page * PageSize).Take(PageSize).ToList();
                if (!ids.Any())
                    break;
                page++;

                sum += await ctx.Mt4Trades
                    .Where(x => ids.Contains(x.Login))
                    .Where(x => x.Cmd == 6)
                    .Where(x => x.CloseTime > from)
                    .Where(x => x.CloseTime <= to)
                    .Where(x => x.Comment.StartsWith("Adjust"))
                    .SumAsync(x => x.Profit);
            }

            return sum;
        }
        finally
        {
            myDbContextPool.ReturnCentralMT4(ctx);
        }
    }

    private async Task<double> SumAgentFee(int serviceId, ICollection<long> logins, DateTime from,
        DateTime to)
    {
        var ctx = await myDbContextPool.BorrowCentralMT4Async(serviceId);
        try
        {
            var sum = 0d;
            var page = 0;
            while (true)
            {
                var ids = logins.Skip(page * PageSize).Take(PageSize).ToList();
                if (!ids.Any())
                    break;
                page++;

                sum += await ctx.Mt4Trades
                    .Where(x => ids.Contains(x.Login))
                    .Where(x => x.Cmd == 6)
                    .Where(x => x.CloseTime > from)
                    .Where(x => x.CloseTime <= to)
                    .Where(x => x.Comment.StartsWith("rebate") || x.Comment.StartsWith("agent%"))
                    .SumAsync(x => x.Profit);
            }

            return sum;
        }
        finally
        {
            myDbContextPool.ReturnCentralMT4(ctx);
        }
    }

    private async Task<double> SumCredit(int serviceId, ICollection<long> logins, DateTime from,
        DateTime to)
    {
        var ctx = await myDbContextPool.BorrowCentralMT4Async(serviceId);
        try
        {
            var sum = 0d;
            var page = 0;
            while (true)
            {
                var ids = logins.Skip(page * PageSize).Take(PageSize).ToList();
                if (!ids.Any())
                    break;
                page++;

                sum += await ctx.Mt4Trades
                    .Where(x => ids.Contains(x.Login))
                    .Where(x => x.Cmd == 7)
                    .Where(x => x.OpenTime > from)
                    .Where(x => x.OpenTime <= to)
                    .SumAsync(x => x.Profit);
            }

            return sum;
        }
        finally
        {
            myDbContextPool.ReturnCentralMT4(ctx);
        }
    }

    private async Task<int> CountNewAccount(int serviceId, ICollection<long> logins, DateTime from,
        DateTime to)
    {
        var ctx = await myDbContextPool.BorrowCentralMT4Async(serviceId);
        try
        {
            var sum = 0;
            var page = 0;
            while (true)
            {
                var ids = logins.Skip(page * PageSize).Take(PageSize).ToList();
                if (!ids.Any())
                    break;
                page++;

                sum += await ctx.Mt4Trades
                    .Where(x => ids.Contains(x.Login))
                    .Where(x => x.Cmd == 6)
                    .Where(x => x.CloseTime > from)
                    .Where(x => x.CloseTime <= to)
                    .Where(x => x.Comment.Contains("deposit") && x.Comment.Contains("new"))
                    .Select(x => x.Login)
                    .Distinct()
                    .CountAsync();
            }

            return sum;
        }
        finally
        {
            myDbContextPool.ReturnCentralMT4(ctx);
        }
    }


    private static string NegativeStyle(double number) => number < 0 ? "color:red;" : "";

    private static string HtmlLineItem(MetaTrade4EquityReport lineItem) =>
        "    <tr style=\"font-size:13px;\">\n"
        + $"        <td style=\"text-align:center;\">{lineItem.Group}</td>\n"
        + $"        <td style=\"text-align:center;\">{lineItem.Name}</td>\n"
        + $"        <td style=\"text-align:center;\">{lineItem.NewAccount}</td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.PreviousEquity)}\">{(lineItem.PreviousEquity < 0 ? $"({-lineItem.PreviousEquity:#,0.00})" : $"{lineItem.PreviousEquity:#,0.00}")}</td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Equity)}\">{(lineItem.Equity < 0 ? $"({-lineItem.Equity:#,0.00})" : $"{lineItem.Equity:#,0.00}")}</td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Deposit)}\">{(lineItem.Deposit < 0 ? $"({-lineItem.Deposit:#,0.00})" : $"{lineItem.Deposit:#,0.00}")}</td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Withdraw)}\">{(lineItem.Withdraw < 0 ? $"({-lineItem.Withdraw:#,0.00})" : $"{lineItem.Withdraw:#,0.00}")}</td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Transfer)}\">{(lineItem.Transfer < 0 ? $"({-lineItem.Transfer:#,0.00})" : $"{lineItem.Transfer:#,0.00}")}</td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Credit)}\">{(lineItem.Credit < 0 ? $"({-lineItem.Credit:#,0.00})" : $"{lineItem.Credit:#,0.00}")}</td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Adjust)}\">{(lineItem.Adjust < 0 ? $"({-lineItem.Adjust:#,0.00})" : $"{lineItem.Adjust:#,0.00}")}</td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Agent)}\">{(lineItem.Agent < 0 ? $"({-lineItem.Agent:#,0.00})" : $"{lineItem.Agent:#,0.00}")}</td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.NetInOut)}\">{(lineItem.NetInOut < 0 ? $"({-lineItem.NetInOut:#,0.00})" : $"{lineItem.NetInOut:#,0.00}")}</td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Pl)}\">{(lineItem.Pl < 0 ? $"({-lineItem.Pl:#,0.00})" : $"{lineItem.Pl:#,0.00}")}</td>\n"
        + $"        <td style=\"text-align:center;\">{(lineItem.Metal / 100):#,0.00}</td>\n"
        + $"        <td style=\"text-align:center;\">{(lineItem.Forex / 100):#,0.00}</td>\n"
        + $"        <td style=\"text-align:center;\">{(lineItem.Oil / 100):#,0.00}</td>\n"
        + $"        <td style=\"text-align:center;\">{(lineItem.Other / 100):#,0.00}</td>\n"
        + $"        <td style=\"text-align:center;\">{(lineItem.Lots / 100):#,0.00}</td>\n"
        + "    </tr>\n";

    private static string HtmlSubtitle(MetaTrade4EquityReport lineItem) =>
        "    <tr style=\"font-size:13px;\">\n"
        + "         <td style=\"text-align:center;\" colspan=\"2\"><strong>Total</strong></td>\n"
        + $"        <td style=\"text-align:center;\"><strong>{lineItem.NewAccount}</td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.PreviousEquity)}\"><strong>{(lineItem.PreviousEquity < 0 ? $"({-lineItem.PreviousEquity:#,0.00})" : $"{lineItem.PreviousEquity:#,0.00}")}</strong></td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Equity)}\"><strong>{(lineItem.Equity < 0 ? $"({-lineItem.Equity:#,0.00})" : $"{lineItem.Equity:#,0.00}")}</strong></td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Deposit)}\"><strong>{(lineItem.Deposit < 0 ? $"({-lineItem.Deposit:#,0.00})" : $"{lineItem.Deposit:#,0.00}")}</strong></td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Withdraw)}\"><strong>{(lineItem.Withdraw < 0 ? $"({-lineItem.Withdraw:#,0.00})" : $"{lineItem.Withdraw:#,0.00}")}</strong></td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Transfer)}\"><strong>{(lineItem.Transfer < 0 ? $"({-lineItem.Transfer:#,0.00})" : $"{lineItem.Transfer:#,0.00}")}</strong></td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Credit)}\"><strong>{(lineItem.Credit < 0 ? $"({-lineItem.Credit:#,0.00})" : $"{lineItem.Credit:#,0.00}")}</strong></td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Adjust)}\"><strong>{(lineItem.Adjust < 0 ? $"({-lineItem.Adjust:#,0.00})" : $"{lineItem.Adjust:#,0.00}")}</strong></td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Agent)}\"><strong>{(lineItem.Agent < 0 ? $"({-lineItem.Agent:#,0.00})" : $"{lineItem.Agent:#,0.00}")}</strong></td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.NetInOut)}\"><strong>{(lineItem.NetInOut < 0 ? $"({-lineItem.NetInOut:#,0.00})" : $"{lineItem.NetInOut:#,0.00}")}</strong></td>\n"
        + $"        <td style=\"text-align:center;{NegativeStyle(lineItem.Pl)}\"><strong>{(lineItem.Pl < 0 ? $"({-lineItem.Pl:#,0.00})" : $"{lineItem.Pl:#,0.00}")}</strong></td>\n"
        + $"        <td style=\"text-align:center;\"><strong>{lineItem.Metal / 100:#,0.00}</strong></td>\n"
        + $"        <td style=\"text-align:center;\"><strong>{lineItem.Forex / 100:#,0.00}</strong></td>\n"
        + $"        <td style=\"text-align:center;\"><strong>{lineItem.Oil / 100:#,0.00}</strong></td>\n"
        + $"        <td style=\"text-align:center;\"><strong>{lineItem.Other / 100:#,0.00}</strong></td>\n"
        + $"        <td style=\"text-align:center;\"><strong>{lineItem.Lots / 100:#,0.00}</strong></td>\n"
        + "    </tr>\n";

    private static string HtmlHeader(string title) =>
        "<table align=\"center\" style=\"width:1280px; font-family:arial, sans-serif; font-size:14px; border-collapse:collapse\" border=\"1\">\n"
        + "    <tr>\n"
        + $"        <td colspan=\"17\" style=\"font-weight:bold; padding-left:20px;\">{title}</td>\n"
        + "    </tr>\n"
        + "    <tr style=\"background:#999999;\">\n"
        + "        <td style=\"width:70px;text-align:center\">Currency</td>\n"
        + "        <td style=\"width:50px;text-align:center\">Office</td>\n"
        + "        <td style=\"width:35px;text-align:center\">New</td>\n"
        + "        <td style=\"width:120px;text-align:center\">Previous Equity</td>\n"
        + "        <td style=\"width:120px;text-align:center\">Current Equity</td>\n"
        + "        <td style=\"width:100px;text-align:center\">Deposit</td>\n"
        + "        <td style=\"width:100px;text-align:center\">Withdraw</td>\n"
        + "        <td style=\"width:80px;text-align:center\">Transfer</td>\n"
        + "        <td style=\"width:100px;text-align:center\">Credit</td>\n"
        + "        <td style=\"width:100px;text-align:center\">Adjust</td>\n"
        + "        <td style=\"width:100px;text-align:center\">Agent</td>\n"
        + "        <td style=\"width:100px;text-align:center\">Net In/Out</td>\n"
        + "        <td style=\"width:100px;text-align:center\">P&L</td>\n"
        + "        <td style=\"width:85px;text-align:center\">Metal</td>\n"
        + "        <td style=\"width:70px;text-align:center\">Forex</td>\n"
        + "        <td style=\"width:70px;text-align:center\">Oil</td>\n"
        + "        <td style=\"width:70px;text-align:center\">Other</td>\n"
        + "        <td style=\"width:70px;text-align:center\">Lots</td>\n"
        + "    </tr>\n";
}