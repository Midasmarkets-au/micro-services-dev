using System.Linq.Expressions;
using Bacera.Gateway.Connection;
using Bacera.Gateway.Context;
using Bacera.Gateway.Services.Message;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Bacera.Gateway.Web.Services.Interface;
using LinqKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bacera.Gateway.Services;

public partial class ReportService(
    AuthDbContext authDbContext,
    TenantDbContext tenantDbContext,
    TenantDbConnection tenantCon,
    IStorageService storageService,
    IMyCache myCache,
    ITenantGetter tenantGetter,
    ILogger<ReportService> logger,
    IEmailSender emailSender,
    MyDbContextPool pool,
    ISendMailService sendMailSvc,
    ConfigurationService configurationSvc,
    ITradingApiService tradingApiSvc,
    MyDbContextPool myDbContextPool,
    MessageService messageSvc,
    ConfigService configSvc)
{
    private long GetTenantId => tenantGetter.GetTenantId();

    // TODO: Move to MT4/MT5 database
    // public async Task<int> GenerateTradeList(TradeReportFilter model, string outputFileName)
    // {
    //     var query = await model.ToQuery(_tenantDbContext);
    //     var totalRecords = 0;
    //     var page = 0;
    //     const int pageSize = 200;
    //     await using var fileStream = File.OpenWrite(outputFileName);
    //     await using var writer = new StreamWriter(fileStream);
    //     await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
    //     csv.Context.RegisterClassMap<TradeMap>();
    //     bool hasMore;
    //     do
    //     {
    //         var items = await query
    //             .Skip(page * pageSize).Take(pageSize)
    //             .ToListAsync();
    //
    //         hasMore = items.Any();
    //         page++;
    //         if (!hasMore) break;
    //         await csv.WriteRecordsAsync(items);
    //         totalRecords += items.Count;
    //     } while (hasMore);
    //
    //     return totalRecords;
    // }

    // TODO: Move to MT4/MT5 database
    // public async Task<List<TransactionReport>> GetReport(ReportConfiguration configuration)
    // {
    //     var items = new List<TransactionReport>();
    //     foreach (var item in configuration.Items)
    //     {
    //         var query = _tenantDbContext.TradeTransactions
    //             .Where(x => x.CloseAt >= configuration.From.Add(configuration.TimezoneOffset))
    //             .Where(x => x.CloseAt < configuration.To.Add(configuration.TimezoneOffset))
    //             .Where(item.ToPredicate());
    //
    //         var response = await query
    //             .GroupBy(x => x.CloseAt!.Value.Date)
    //             .Select(g
    //                 => new TransactionReport
    //                 {
    //                     Name = item.Name,
    //                     Group = item.Group,
    //                     Date = g.Key.ToString("yyyy-MM-dd"),
    //                     TransactionCount = g.Count(),
    //                     TotalAmount = g.Sum(x => x.Profit),
    //                     AccountCount = g.Select(x => x.TradeAccountId).Distinct().Count(),
    //                 })
    //             .ToListAsync();
    //
    //         items.AddRange(response);
    //     }
    //
    //     return items;
    // }

    // TODO: Move to MT4/MT5 database
    // private sealed class TradeMap : ClassMap<TradeTransaction>
    // {
    //     private TradeMap()
    //     {
    //         Map(m => m.Id).Index(0).Name("Id");
    //         //Map(m => m.TradeAccountId).Index(1).Name("TradeAccountId");
    //         Map(m => m.AccountNumber).Index(2).Name("AccountNumber");
    //         //Map(m => m.ServiceId).Index(3).Name("ServiceId");
    //         Map(m => m.Ticket).Index(4).Name("Ticket");
    //         Map(m => m.Symbol).Index(5).Name("Symbol");
    //         Map(m => m.Digits).Index(6).Name("Digits");
    //         Map(m => m.Cmd).Index(7).Name("CMD");
    //         Map(m => m.Volume).Index(8).Name("Volume");
    //         Map(m => m.OpenAt).Index(9).Name("OpenAt");
    //         Map(m => m.OpenPrice).Index(10).Name("OpenPrice");
    //         Map(m => m.Sl).Index(11).Name("SL");
    //         Map(m => m.Tp).Index(12).Name("TP");
    //         Map(m => m.CloseAt).Index(13).Name("CloseAt");
    //         Map(m => m.ClosePrice).Index(14).Name("ClosePrice");
    //         Map(m => m.ExpiresAt).Index(15).Name("ExpiresAt");
    //         Map(m => m.Reason).Index(16).Name("Reason");
    //         Map(m => m.ConvertRate).Index(17).Name("ConvertRate");
    //         Map(m => m.ConvertRate2).Index(18).Name("ConvertRate2");
    //         Map(m => m.Commission).Index(19).Name("Commission");
    //         Map(m => m.CommissionAgent).Index(20).Name("CommissionAgent");
    //         Map(m => m.Swaps).Index(21).Name("Swaps");
    //         Map(m => m.Profit).Index(22).Name("Profit");
    //         Map(m => m.Taxes).Index(23).Name("Taxes");
    //         Map(m => m.Comment).Index(24).Name("Comment");
    //         Map(m => m.MarginRate).Index(25).Name("MarginRate");
    //         Map(m => m.ModifiedAt).Index(26).Name("ModifiedAt");
    //         //Map(m => m.CreatedOn).Index(27).Name("CreatedOn");
    //         //Map(m => m.UpdatedOn).Index(28).Name("UpdatedOn");
    //         //Map(m => m.Status).Index(29).Name("Status");
    //         Map(m => m.TimeStamp).Index(30).Name("TimeStamp");
    //     }
    //
    //     public static ClassMap<TradeTransaction> Instance { get; } = new TradeMap();
    // }
}

public static class ReportServiceExt
{
    // // TODO: Move to MT4/MT5 database
    // public static async Task<IQueryable<TradeTransaction>> ToQuery(this TradeReportFilter model,
    //     TenantDbContext tenantDbContext)
    // {
    //     var account = await tenantDbContext.Accounts
    //         .Where(x => x.Uid == model.AccountUid)
    //         .Select(x => new Account { Id = x.Id, Uid = x.Uid, Role = x.Role })
    //         .SingleAsync();
    //
    //     var baseQuery = tenantDbContext.TradeTransactions
    //         .applyDateRangeFilter(model.ClosedFrom, model.ClosedTo);
    //
    //     switch (model.Scope)
    //     {
    //         case ScopeType.ForSelf:
    //             return baseQuery.Where(x => x.TradeAccountId == account.Id);
    //         case ScopeType.ForSubAccounts:
    //             switch (account.Role)
    //             {
    //                 case (int)AccountRoleTypes.Sales:
    //                     return baseQuery.Where(x => x.TradeAccount.IdNavigation.SalesAccountId == account.Id);
    //                 case (int)AccountRoleTypes.Agent:
    //                 {
    //                     var sql =
    //                         $"SELECT t.* FROM trd.\"_TradeTransaction\" t JOIN (SELECT \"Id\" FROM get_all_child_accounts({account.Id})) a ON t.\"Id\" = a.\"Id\"";
    //                     var childTransactionQuery = tenantDbContext.TradeTransactions
    //                         .FromSqlRaw(sql)
    //                         .AsQueryable();
    //                     return childTransactionQuery.applyDateRangeFilter(model.ClosedFrom, model.ClosedTo);
    //                 }
    //             }
    //
    //             break;
    //     }
    //
    //     throw new ArgumentOutOfRangeException
    //     {
    //         HelpLink = null,
    //         HResult = 0,
    //         Source = null
    //     };
    // }
    //
    // private static IQueryable<TradeTransaction> applyDateRangeFilter(this IQueryable<TradeTransaction> query,
    //     DateTime closedFrom, DateTime closedTo)
    // {
    //     return query.Where(x => x.CloseAt >= closedFrom && x.CloseAt <= closedTo);
    // }
    //
    // public static Expression<Func<TradeTransaction, bool>> ToPredicate(this ReportConfigurationItem me)
    // {
    //     // Initialize an inclusive predicate for Sales Code starting with specific terms
    //     var includeSalesCode = PredicateBuilder.New<TradeTransaction>();
    //     foreach (var term in me.IncludeSalesCode)
    //     {
    //         if (string.IsNullOrEmpty(term))
    //         {
    //         }
    //         else if (term.Trim().EndsWith("%"))
    //         {
    //             // Add an AND condition to the predicate if a currency ID is specified
    //             includeSalesCode = includeSalesCode.Or(x =>
    //                 x.TradeAccount.IdNavigation.SalesAccountId > 0
    //                 && x.TradeAccount.IdNavigation.SalesAccount!.Code
    //                     .ToUpper().StartsWith(term.Trim().TrimEnd('%').ToUpper()
    //                     ));
    //         }
    //         else
    //         {
    //             includeSalesCode = includeSalesCode.Or(x =>
    //                 x.TradeAccount.IdNavigation.SalesAccountId > 0
    //                 && x.TradeAccount.IdNavigation.SalesAccount!.Code.ToUpper().Equals(term.Trim().ToUpper()));
    //         }
    //     }
    //
    //
    //     // Initialize an exclusive predicate for group starting with specific terms
    //     var excludeSalesCode = PredicateBuilder.New<TradeTransaction>();
    //     foreach (var term in me.ExcludeSalesCode)
    //     {
    //         if (string.IsNullOrEmpty(term))
    //         {
    //         }
    //         else if (term.Trim().EndsWith("%"))
    //         {
    //             // Add an AND condition to the predicate if a currency ID is specified
    //             excludeSalesCode = excludeSalesCode.And(x =>
    //                 x.TradeAccount.IdNavigation.SalesAccountId > 0
    //                 && x.TradeAccount.IdNavigation.SalesAccount!.Code
    //                     .ToUpper().StartsWith(
    //                         term.Trim().TrimEnd('%').ToUpper()) == false
    //             );
    //         }
    //         else
    //         {
    //             excludeSalesCode = excludeSalesCode.And(x =>
    //                 x.TradeAccount.IdNavigation.SalesAccountId > 0
    //                 && x.TradeAccount.IdNavigation.SalesAccount!
    //                     .Code.ToUpper()
    //                     .Equals(
    //                         term.Trim().ToUpper()
    //                     ) == false
    //             );
    //         }
    //     }
    //
    //     // Initialize an inclusive predicate for group starting with specific terms
    //     var includeGroup = PredicateBuilder.New<TradeTransaction>();
    //     foreach (var term in me.IncludeGroupStartWith)
    //     {
    //         includeGroup = includeGroup.Or(x =>
    //             x.TradeAccount.IdNavigation.Group.StartsWith(term));
    //     }
    //
    //     // Initialize an exclusive predicate for group starting with specific terms
    //     var excludeGroup = PredicateBuilder.New<TradeTransaction>();
    //     foreach (var term in me.ExcludeGroupStartWith)
    //     {
    //         excludeGroup = excludeGroup.And(x =>
    //             x.TradeAccount.IdNavigation.Group.StartsWith(term) == false);
    //     }
    //
    //     // Initialize an inclusive predicate for group starting with specific terms
    //     var includeTradeGroup = PredicateBuilder.New<TradeTransaction>();
    //     foreach (var term in me.IncludeTradeGroup)
    //     {
    //         includeTradeGroup = includeTradeGroup.Or(x =>
    //             x.TradeAccount.TradeAccountStatus != null
    //             && x.TradeAccount.TradeAccountStatus.Group == term);
    //     }
    //
    //     // Initialize an exclusive predicate for group starting with specific terms
    //     var excludeTradeGroup = PredicateBuilder.New<TradeTransaction>();
    //     foreach (var term in me.ExcludeTradeGroup)
    //     {
    //         excludeTradeGroup = excludeTradeGroup.And(x =>
    //             x.TradeAccount.TradeAccountStatus != null
    //             && x.TradeAccount.TradeAccountStatus.Group == term);
    //     }
    //
    //     // Initialize an inclusive predicate for account numbers
    //     var includeLogin = PredicateBuilder.New<TradeTransaction>();
    //     if (me.IncludeAccountNumber.Any())
    //     {
    //         includeLogin = includeLogin.Or(x =>
    //             me.IncludeAccountNumber.Contains(x.TradeAccount.AccountNumber));
    //     }
    //
    //     // Initialize an exclusive predicate for account numbers
    //     var excludeLogin = PredicateBuilder.New<TradeTransaction>();
    //     foreach (var term in me.ExcludeAccountNumber)
    //     {
    //         excludeLogin = excludeLogin.And(x =>
    //             x.TradeAccount.AccountNumber != term);
    //     }
    //
    //     // Initialize a predicate for currency ID
    //     var hasCurrencyId = PredicateBuilder.New<TradeTransaction>(false);
    //     if ((int)me.CurrencyId > 0)
    //     {
    //         // Add an AND condition to the predicate if a currency ID is specified
    //         hasCurrencyId = hasCurrencyId.And(x =>
    //             x.TradeAccount.CurrencyId == (int)me.CurrencyId);
    //     }
    //
    //     // Combine the predicates to build the final predicate
    //     var final = hasCurrencyId;
    //
    //     if (includeSalesCode.IsStarted)
    //         final = final.And(includeSalesCode);
    //
    //     if (excludeSalesCode.IsStarted)
    //         final = final.And(includeSalesCode);
    //
    //     if (includeGroup.IsStarted)
    //         final = final.And(includeGroup);
    //
    //     if (excludeGroup.IsStarted)
    //         final = final.And(excludeGroup);
    //
    //     if (includeTradeGroup.IsStarted)
    //         final = final.And(includeTradeGroup);
    //
    //     if (excludeTradeGroup.IsStarted)
    //         final = final.And(excludeTradeGroup);
    //
    //     if (includeLogin.IsStarted)
    //         final = final.Or(includeLogin);
    //
    //     if (excludeLogin.IsStarted)
    //         final = final.And(excludeLogin);
    //
    //     return final;
    //     //var sql = predicateFinal.ToString();
    //     //query = query.Where(predicateFinal);
    // }

    // only sea customers have level sales
    // public static Expression<Func<Account, bool>> ToSeaLoginPredicate(this ReportConfigurationItem me)
    // {
    //     var includeSalesCode = PredicateBuilder.New<Account>();
    //     foreach (var term in me.IncludeSalesCode)
    //     {
    //         if (string.IsNullOrEmpty(term))
    //         {
    //         }
    //         else if (term.Trim().EndsWith("%"))
    //         {
    //             // Add an AND condition to the predicate if a currency ID is specified
    //             includeSalesCode = includeSalesCode.Or(x =>
    //                 x.SalesAccountId > 0
    //                 && x.SalesAccount!.Code
    //                     .ToUpper().StartsWith(term.Trim().TrimEnd('%').ToUpper()
    //                     ));
    //         }
    //         else
    //         {
    //             includeSalesCode = includeSalesCode.Or(x =>
    //                 x.SalesAccountId > 0
    //                 && x.SalesAccount!.Code.ToUpper().Equals(term.Trim().ToUpper()));
    //         }
    //     }
    // }

    public static Expression<Func<Account, bool>> ToBasicLoginPredicate(this ReportConfigurationItem me)
    {
        // Initialize an inclusive predicate for group starting with specific terms
        var includeGroup = PredicateBuilder.New<Account>();
        foreach (var term in me.IncludeGroupStartWith)
        {
            includeGroup = includeGroup.Or(x => x.Group.StartsWith(term));
        }

        // Initialize an exclusive predicate for group starting with specific terms
        var excludeGroup = PredicateBuilder.New<Account>();
        foreach (var term in me.ExcludeGroupStartWith)
        {
            excludeGroup = excludeGroup.And(x => x.Group.StartsWith(term) == false);
        }

        // Initialize an inclusive predicate for account numbers
        var includeLogin = PredicateBuilder.New<Account>();
        if (me.IncludeAccountNumber.Any())
        {
            includeLogin = includeLogin.Or(x => me.IncludeAccountNumber.Contains(x.AccountNumber));
        }

        // Initialize an exclusive predicate for account numbers
        var excludeLogin = PredicateBuilder.New<Account>();
        foreach (var term in me.ExcludeAccountNumber)
        {
            excludeLogin = excludeLogin.And(x => x.AccountNumber != term);
        }

        // Initialize an inclusive predicate for account Types
        var includeAccountType = PredicateBuilder.New<Account>();
        if (me.IncludeAccountType.HasValue)
        {
            var type = (short)me.IncludeAccountType.Value;
            includeAccountType = includeAccountType.And(x => x.Type == type);
        }

        // Initialize an exclusive predicate for account Types
        var excludeAccountType = PredicateBuilder.New<Account>();
        if (me.ExcludeAccountType.HasValue)
        {
            var type = (short)me.ExcludeAccountType.Value;
            excludeAccountType = excludeAccountType.And(x => x.Type != type);
        }

        // Combine the predicates to build the final predicate
        var final = PredicateBuilder.New<Account>();
        // Initialize a predicate for currency ID
        var hasCurrencyId = PredicateBuilder.New<Account>(false);
        if ((int)me.CurrencyId > 0)
        {
            // Add an AND condition to the predicate if a currency ID is specified
            hasCurrencyId = hasCurrencyId.And(x => x.CurrencyId == (int)me.CurrencyId);
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


    public static Expression<Func<Account, bool>> ToLoginPredicate(this ReportConfigurationItem me)
    {
        // Initialize an inclusive predicate for Sales Code starting with specific terms
        var includeSalesCode = PredicateBuilder.New<Account>();
        foreach (var term in me.IncludeSalesCode)
        {
            if (string.IsNullOrEmpty(term))
            {
            }
            else if (term.Trim().EndsWith("%"))
            {
                // Add an AND condition to the predicate if a currency ID is specified
                includeSalesCode = includeSalesCode.Or(x =>
                    x.SalesAccountId > 0 &&
                    x.SalesAccount!.Code.ToUpper().StartsWith(term.Trim().TrimEnd('%').ToUpper()));
            }
            else
            {
                includeSalesCode = includeSalesCode.Or(x =>
                    x.SalesAccountId > 0 &&
                    x.SalesAccount!.Code.ToUpper().Equals(term.Trim().ToUpper()));
            }
        }

        // Initialize an exclusive predicate for group starting with specific terms
        var excludeSalesCode = PredicateBuilder.New<Account>();
        foreach (var term in me.ExcludeSalesCode)
        {
            if (string.IsNullOrEmpty(term))
            {
            }
            else if (term.Trim().EndsWith("%"))
            {
                // Add an AND condition to the predicate if a currency ID is specified
                excludeSalesCode = excludeSalesCode.And(x =>
                    x.SalesAccountId > 0
                    && x.SalesAccount!.Code.ToUpper().StartsWith(term.Trim().TrimEnd('%').ToUpper()) == false
                );
            }
            else
            {
                excludeSalesCode = excludeSalesCode.And(x =>
                    x.SalesAccountId > 0
                    && x.SalesAccount!.Code.ToUpper().Equals(term.Trim().ToUpper()) == false
                );
            }
        }

        // Initialize an inclusive predicate for group starting with specific terms
        var includeGroup = PredicateBuilder.New<Account>();
        foreach (var term in me.IncludeGroupStartWith)
        {
            includeGroup = includeGroup.Or(x => x.Group.StartsWith(term));
        }

        // Initialize an exclusive predicate for group starting with specific terms
        var excludeGroup = PredicateBuilder.New<Account>();
        foreach (var term in me.ExcludeGroupStartWith)
        {
            excludeGroup = excludeGroup.And(x => x.Group.StartsWith(term) == false);
        }

        // Initialize an inclusive predicate for account numbers
        var includeLogin = PredicateBuilder.New<Account>();
        if (me.IncludeAccountNumber.Any())
        {
            includeLogin = includeLogin.Or(x => me.IncludeAccountNumber.Contains(x.AccountNumber));
        }

        // Initialize an exclusive predicate for account numbers
        var excludeLogin = PredicateBuilder.New<Account>();
        foreach (var term in me.ExcludeAccountNumber)
        {
            excludeLogin = excludeLogin.And(x => x.AccountNumber != term);
        }

        // Initialize an inclusive predicate for account Types
        var includeAccountType = PredicateBuilder.New<Account>();
        if (me.IncludeAccountType.HasValue)
        {
            var type = (short)me.IncludeAccountType.Value;
            includeAccountType = includeAccountType.And(x => x.Type == type);
        }

        // Initialize an exclusive predicate for account Types
        var excludeAccountType = PredicateBuilder.New<Account>();
        if (me.ExcludeAccountType.HasValue)
        {
            var type = (short)me.ExcludeAccountType.Value;
            excludeAccountType = excludeAccountType.And(x => x.Type != type);
        }

        // Combine the predicates to build the final predicate
        var final = PredicateBuilder.New<Account>();
        // Initialize a predicate for currency ID
        var hasCurrencyId = PredicateBuilder.New<Account>(false);
        if ((int)me.CurrencyId > 0)
        {
            // Add an AND condition to the predicate if a currency ID is specified
            hasCurrencyId = hasCurrencyId.And(x => x.CurrencyId == (int)me.CurrencyId);
        }

        // Combine the predicates to build the final predicate
        if (hasCurrencyId.IsStarted)
        {
            final = final.And(hasCurrencyId);
        }

        if (includeSalesCode.IsStarted)
        {
            final = final.And(includeSalesCode);
        }

        if (excludeSalesCode.IsStarted)
        {
            final = final.And(excludeSalesCode);
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
}