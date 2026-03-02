using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Report.Models;
using Bacera.Gateway.Web.BackgroundJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Bacera.Gateway.Web.Tests;

public class ReportServiceTests : Startup
{
    // private readonly ITestOutputHelper _testOutputHelper;
    // private readonly ReportService _reportService;

    // private readonly TradingService _tradingService;
    // private readonly IRebateJob _rebateJob;

    // private readonly IReportJob _reportJob;
    // private readonly ISendMailService _sendMailService;

    public ReportServiceTests(ITestOutputHelper testOutputHelper)
    {
        // _testOutputHelper = testOutputHelper;
        // _tradingService = AppServiceProvider.GetRequiredService<TradingService>();
        // _rebateJob = AppServiceProvider.GetRequiredService<IRebateJob>();
        // _reportJob = AppServiceProvider.GetRequiredService<IReportJob>();
        // _sendMailService = AppServiceProvider.GetRequiredService<ISendMailService>();
        // _reportService = AppServiceProvider.GetRequiredService<ReportService>();
    }

    // [Fact]
    // public async Task TestGenerateTradeRebate()
    // {
    //     // await _tradingService.GenerateTradeRebateAsync(tradingService, from);
    //     await _rebateJob.GenerateTradeRebate();
    // }

    // [Fact]
    // public async Task TestSendReport()
    // {
    //     var results = await _reportJob.GenerateAccountDailyConfirmationReport();
    //     foreach (var (count, msg) in results)
    //     {
    //         _testOutputHelper.WriteLine($"Processed {count} records for tenant, {msg}");
    //     }
    // }

    // [Fact]
    // public async Task GenerateTradeList_ForRandomAccount_ReturnSuccess()
    // {
    //     var request = new TradeReportFilter
    //     {
    //         AccountUid = 26352042,
    //         Scope = ScopeType.ForSubAccounts,
    //         ClosedFrom = DateTime.UtcNow.AddMonths(-2),
    //         ClosedTo = DateTime.UtcNow,
    //     };
    //     var tempFile = Path.GetTempFileName();
    //     var total = await _svc.GenerateTradeList(request, tempFile);
    //     total.ShouldBeGreaterThan(0);
    //     var file = new FileInfo(tempFile);
    //     file.Exists.ShouldBeTrue();
    //     file.Length.ShouldBeGreaterThan(0);
    // }

    // [Fact]
    // public async Task TransactionSummaryReport_TestResponseModel_ReturnHasValues()
    // {
    //     var record = await tenantDbContext.TransactionSummaryReports.FirstOrDefaultAsync();
    //     record.ShouldNotBeNull();
    //
    //     var criteria = new TransactionSummaryReport.Criteria
    //     {
    //         PartyId = record.PartyId,
    //         RowId = record.RowId,
    //         From = record.DateTime.AddMonths(-2),
    //         To = record.DateTime.AddMonths(1),
    //         CurrencyId = (CurrencyTypes)record.CurrencyId,
    //         Type = (TransactionSummaryReportTypes)record.Type,
    //         TimeZoneOffset = 0,
    //         PeriodType = (ReportPeriodTypes)record.PeriodType,
    //         Size = int.MaxValue
    //     };
    //
    //     var query = tenantDbContext.TransactionSummaryReports
    //             .FilterBy(criteria);
    //
    //     //var rawItems = await query.ToListAsync();
    //     //rawItems.ShouldNotBeNull();
    //     //rawItems.Any().ShouldBeTrue();
    //
    //     var monthlyReports = await query.ToMonthlyResponseModel().ToListAsync();
    //     monthlyReports.Any().ShouldBeTrue();
    //
    //     var dailyReports = await query.ToDailyResponseModel().ToListAsync();
    //     dailyReports.Any().ShouldBeTrue();
    //
    //     var hourlyReports = await query.ToHourlyResponseModel().ToListAsync();
    //     hourlyReports.Any().ShouldBeTrue();
    // }
}