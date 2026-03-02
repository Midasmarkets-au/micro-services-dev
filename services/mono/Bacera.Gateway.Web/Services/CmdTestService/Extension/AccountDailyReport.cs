using System.Globalization;
using System.Text;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Connection;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Core.Utility;
using Bacera.Gateway.Integration;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.MetaTrader.Models;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using CsvHelper;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Services;

public partial class CmdTestService
{
    /// <summary>
    /// Regenerate report(s) by ID(s). Supports both single ID and multiple IDs.
    /// </summary>
    private async Task RegenReport(params long[] reportIds)
    {
        if (reportIds is null || reportIds.Count() == 0)
        {
            reportIds = new long[] { 447 };
        }

        //var reportIds = new long[] { 314 };
        if (reportIds == null || reportIds.Length == 0)
        {
            Console.WriteLine("No report IDs provided. Usage: RegenReport(447) or RegenReport(447, 450, 456)");
            return;
        }

        const long tenantId = 10000; // Change this to your target tenant ID (10000 for Testing, 10004 for Staging)
        using var scope = _serviceProvider.CreateTenantScope(tenantId);
        var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var reportSvc = scope.ServiceProvider.GetRequiredService<ReportService>();

        Console.WriteLine($"Regenerating {reportIds.Length} report(s) for tenant {tenantId}...");
        
        var successCount = 0;
        var failCount = 0;
        var notFoundIds = new List<long>();

        foreach (var reportId in reportIds)
        {
            try
            {
                var report = await tenantCtx.ReportRequests
                    .SingleOrDefaultAsync(x => x.Id == reportId);
                
                if (report == null)
                {
                    Console.WriteLine($"❌ ReportRequest with Id {reportId} not found in tenant {tenantId}");
                    notFoundIds.Add(reportId);
                    failCount++;
                    continue;
                }

                Console.WriteLine($"🔄 Processing ReportRequest Id: {reportId}, Name: {report.Name}...");
                
                // 如果是 Rebate 或 WalletTransactionForTenant 类型，需要同时重新生成配对报告（双向支持）
                ReportRequest? pairedReport = null;
                if (report.Type == (int)ReportRequestTypes.Rebate || report.Type == (int)ReportRequestTypes.WalletTransactionForTenant)
                {
                    var isClosingTimeBased = report.Name.Contains("MT5 ClosingTime Based");
                    var isReleasedTimeBased = report.Name.Contains("ReleasedTime Based");
                    
                    // 从 query 中提取日期
                    DateTime? reportDate = null;
                    try
                    {
                        var queryObj = JsonConvert.DeserializeObject<dynamic>(report.Query);
                        if (queryObj?.to != null)
                        {
                            reportDate = DateTime.Parse(queryObj.to.ToString());
                        }
                    }
                    catch
                    {
                        // 如果从 query 解析失败，尝试从名称中提取日期
                        var dateMatch = System.Text.RegularExpressions.Regex.Match(report.Name, @"(\d{4}-\d{2}-\d{2})");
                        if (dateMatch.Success)
                        {
                            reportDate = DateTime.Parse(dateMatch.Groups[1].Value);
                        }
                    }
                    
                    if (reportDate.HasValue)
                    {
                        var dateStr = reportDate.Value.ToString("yyyy-MM-dd");
                        var fromDate = reportDate.Value.AddDays(-1);
                        
                        // 确定报告类型和名称模式
                        var reportType = report.Type;
                        string baseNamePattern;
                        string closingTimeNamePattern;
                        string releasedTimeNamePattern;
                        
                        if (reportType == (int)ReportRequestTypes.Rebate)
                        {
                            baseNamePattern = "Rebate Daily Record";
                            closingTimeNamePattern = $"Rebate Daily Record (MT5 ClosingTime Based) {dateStr}";
                            releasedTimeNamePattern = $"Rebate Daily Record (ReleasedTime Based) {dateStr}";
                        }
                        else if (reportType == (int)ReportRequestTypes.WalletTransactionForTenant)
                        {
                            baseNamePattern = "Wallet Daily Transaction";
                            closingTimeNamePattern = $"Wallet Daily Transaction (MT5 ClosingTime Based) {dateStr}";
                            releasedTimeNamePattern = $"Wallet Daily Transaction (ReleasedTime Based) {dateStr}";
                        }
                        else
                        {
                            baseNamePattern = "";
                            closingTimeNamePattern = "";
                            releasedTimeNamePattern = "";
                        }
                        
                        // 更新报告名称为新格式（如果还是旧格式）
                        if (!isClosingTimeBased && !isReleasedTimeBased && !string.IsNullOrEmpty(baseNamePattern))
                        {
                            // 根据 IsFromApi 决定默认名称
                            if (report.IsFromApi == 0)
                            {
                                report.Name = closingTimeNamePattern;
                                isClosingTimeBased = true;
                            }
                            else
                            {
                                report.Name = releasedTimeNamePattern;
                                isReleasedTimeBased = true;
                            }
                            Console.WriteLine($"   📝 Updated report name to new format: {report.Name}");
                        }
                        
                        if (isClosingTimeBased)
                        {
                            // 当前是 MT5 ClosingTime Based，查找 ReleasedTime Based 配对报告
                            pairedReport = await tenantCtx.ReportRequests
                                .FirstOrDefaultAsync(x => 
                                    x.Type == reportType
                                    && x.IsFromApi == 1
                                    && x.Name.Contains("ReleasedTime Based")
                                    && x.Name.Contains(dateStr)
                                    && x.PartyId == report.PartyId
                                    && x.Id != report.Id);
                            
                            // 如果没有配对报告，创建一个新的
                            if (pairedReport == null && !string.IsNullOrEmpty(releasedTimeNamePattern))
                            {
                                Console.WriteLine($"   ➕ Creating paired report (ReleasedTime Based) for date {dateStr}...");
                                // 确保日期时间为 UTC 格式
                                var fromDateUtc = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);
                                var toDateUtc = DateTime.SpecifyKind(reportDate.Value, DateTimeKind.Utc);
                                
                                if (reportType == (int)ReportRequestTypes.Rebate)
                                {
                                    pairedReport = ReportRequest.Build(
                                        report.PartyId,
                                        ReportRequestTypes.Rebate,
                                        releasedTimeNamePattern,
                                        JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings));
                                }
                                else if (reportType == (int)ReportRequestTypes.WalletTransactionForTenant)
                                {
                                    pairedReport = new ReportRequest
                                    {
                                        PartyId = report.PartyId,
                                        Type = (int)ReportRequestTypes.WalletTransactionForTenant,
                                        Name = releasedTimeNamePattern,
                                        Query = JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings),
                                        IsFromApi = 1
                                    };
                                }
                                
                                if (pairedReport != null)
                                {
                                    pairedReport.IsFromApi = 1; // 标记为API入口
                                    tenantCtx.ReportRequests.Add(pairedReport);
                                    await tenantCtx.SaveChangesAsync();
                                    Console.WriteLine($"   ✅ Created paired report Id: {pairedReport.Id}");
                                }
                            }
                            else if (pairedReport != null)
                            {
                                Console.WriteLine($"   📎 Found paired report Id: {pairedReport.Id}, Name: {pairedReport.Name}");
                            }
                        }
                        else if (isReleasedTimeBased)
                        {
                            // 当前是 ReleasedTime Based，查找 MT5 ClosingTime Based 配对报告
                            pairedReport = await tenantCtx.ReportRequests
                                .FirstOrDefaultAsync(x => 
                                    x.Type == reportType
                                    && x.IsFromApi == 0
                                    && x.Name.Contains("MT5 ClosingTime Based")
                                    && x.Name.Contains(dateStr)
                                    && x.PartyId == report.PartyId
                                    && x.Id != report.Id);
                            
                            // 如果没有配对报告，创建一个新的
                            if (pairedReport == null && !string.IsNullOrEmpty(closingTimeNamePattern))
                            {
                                Console.WriteLine($"   ➕ Creating paired report (MT5 ClosingTime Based) for date {dateStr}...");
                                // 确保日期时间为 UTC 格式
                                var fromDateUtc = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);
                                var toDateUtc = DateTime.SpecifyKind(reportDate.Value, DateTimeKind.Utc);
                                
                                if (reportType == (int)ReportRequestTypes.Rebate)
                                {
                                    pairedReport = ReportRequest.Build(
                                        report.PartyId,
                                        ReportRequestTypes.Rebate,
                                        closingTimeNamePattern,
                                        JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings));
                                }
                                else if (reportType == (int)ReportRequestTypes.WalletTransactionForTenant)
                                {
                                    pairedReport = new ReportRequest
                                    {
                                        PartyId = report.PartyId,
                                        Type = (int)ReportRequestTypes.WalletTransactionForTenant,
                                        Name = closingTimeNamePattern,
                                        Query = JsonConvert.SerializeObject(new { from = fromDateUtc, to = toDateUtc }, Utils.AppJsonSerializerSettings),
                                        IsFromApi = 0
                                    };
                                }
                                
                                if (pairedReport != null)
                                {
                                    pairedReport.IsFromApi = 0; // 标记为Job入口（默认值）
                                    tenantCtx.ReportRequests.Add(pairedReport);
                                    await tenantCtx.SaveChangesAsync();
                                    Console.WriteLine($"   ✅ Created paired report Id: {pairedReport.Id}");
                                }
                            }
                            else if (pairedReport != null)
                            {
                                Console.WriteLine($"   📎 Found paired report Id: {pairedReport.Id}, Name: {pairedReport.Name}");
                            }
                        }
                        
                        // 如果找到或创建了配对报告，同时重新生成配对报告csv
                        if (pairedReport != null)
                        {
                            pairedReport.FileName = "";
                            pairedReport.GeneratedOn = null;
                            await tenantCtx.SaveChangesAsync();
                            
                            Console.WriteLine($"   🔄 Processing paired ReportRequest Id: {pairedReport.Id}...");
                            await reportSvc.ProcessRequestAsync(pairedReport);
                            Console.WriteLine($"   ✅ Successfully regenerated paired ReportRequest Id: {pairedReport.Id}");
                        }
                    }
                }
                
                // 重置当前报告状态
                report.FileName = "";
                report.GeneratedOn = null;
                await tenantCtx.SaveChangesAsync();
                
                // 重新生成当前报告
                await reportSvc.ProcessRequestAsync(report);
                Console.WriteLine($"✅ Successfully regenerated ReportRequest Id: {reportId}");
                successCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error regenerating ReportRequest Id {reportId}: {ex.Message}");
                Console.WriteLine($"   Stack trace: {ex.StackTrace}");
                failCount++;
            }
        }

        Console.WriteLine($"\n📊 Summary:");
        Console.WriteLine($"   ✅ Success: {successCount}");
        Console.WriteLine($"   ❌ Failed: {failCount}");
        if (notFoundIds.Any())
        {
            Console.WriteLine($"   ⚠️  Not Found IDs: {string.Join(", ", notFoundIds)}");
        }
    }

    /// <summary>
    /// Regenerate reports by name pattern (e.g., "Rebate Daily Record%" or "%Deposit%").
    /// Usage examples:
    /// - await RegenReportByNamePattern("Rebate Daily Record%");
    /// - await RegenReportByNamePattern("%Deposit%");
    /// - await RegenReportByNamePattern("Rebate Daily Record%", "%Deposit%");
    /// </summary>
    private async Task RegenReportByNamePattern(params string[] namePatterns)
    {
        if (namePatterns == null || namePatterns.Length == 0)
        {
            Console.WriteLine("No name patterns provided. Usage: RegenReportByNamePattern(\"Rebate Daily Record%\")");
            return;
        }

        const long tenantId = 10000; // Change this to your target tenant ID (10000 for Testing, 10004 for Staging)
        using var scope = _serviceProvider.CreateTenantScope(tenantId);
        var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var reportSvc = scope.ServiceProvider.GetRequiredService<ReportService>();

        // Build query with OR conditions for multiple patterns
        var reports = await tenantCtx.ReportRequests
            .Where(x => namePatterns.Any(pattern => EF.Functions.ILike(x.Name, pattern)))
            .OrderByDescending(x => x.CreatedOn)
            .ToListAsync();

        if (!reports.Any())
        {
            Console.WriteLine($"No reports found matching pattern(s): {string.Join(", ", namePatterns)}");
            return;
        }

        var reportIds = reports.Select(x => x.Id).ToArray();
        Console.WriteLine($"Found {reports.Count} report(s) matching pattern(s): {string.Join(", ", namePatterns)}");
        Console.WriteLine($"Report IDs: {string.Join(", ", reportIds)}");
        Console.WriteLine();

        // Use the existing RegenReport method to process them
        await RegenReport(reportIds);
    }

    /// <summary>
    /// Test ExecuteCloseTradeJobAsync() - triggers the Daily Rebate Report generation
    /// This simulates the Hangfire job that runs at 22:00 (or configured time)
    /// </summary>
    private async Task TestCloseTradeJob()
    {
        Console.WriteLine("🚀 Starting TestCloseTradeJob - Executing ExecuteCloseTradeJobAsync()...");
        Console.WriteLine("This will trigger GenerateCloseTradeReportAsync() which processes Daily Rebate Reports for tenants 10000 and 10004");
        Console.WriteLine();

        try
        {
            var reportJob = _serviceProvider.GetRequiredService<IReportJob>();
            await reportJob.ExecuteCloseTradeJobAsync();
            Console.WriteLine("✅ ExecuteCloseTradeJobAsync() completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error executing ExecuteCloseTradeJobAsync(): {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}