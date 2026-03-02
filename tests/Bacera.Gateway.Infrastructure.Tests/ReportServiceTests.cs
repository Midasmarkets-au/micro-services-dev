using Bacera.Gateway.Integration;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Report.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
public class ReportServiceTests : Startup
{
    private readonly ReportService _svc;
    private readonly MetaTrade4DbContext _mt4Ctx;

    public ReportServiceTests()
    {
        var tenantConnection = Configuration.GetConnectionString("TenantConnectionTemplate-au");
        tenantConnection.ShouldNotBeNullOrEmpty();
        tenantConnection = tenantConnection.Replace("{{DATABASE}}", "portal_tenant_0723_au");
        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        optionsBuilder.UseNpgsql(tenantConnection);
        var ctx = new TenantDbContext(optionsBuilder.Options);

        var mt4Connection = Configuration.GetConnectionString("MT4Connection");
        mt4Connection.ShouldNotBeNullOrEmpty();
        var mt4OptionsBuilder = new DbContextOptionsBuilder<MetaTrade4DbContext>();
        mt4OptionsBuilder.UseMySql(mt4Connection, ServerVersion.Parse("5.7.38-mysql"));
        _mt4Ctx = new MetaTrade4DbContext(mt4OptionsBuilder.Options);

        var cache = ServiceProvider.GetRequiredService<IMyCache>();
        var tradingSvc = ServiceProvider.GetRequiredService<TradingService>();
        var tenantGetter = ServiceProvider.GetRequiredService<ITenantGetter>();
        ILogger<ReportService> logger = new Logger<ReportService>(new NullLoggerFactory());
        // _svc = new ReportService(AuthDbContext, ctx, Mock.Of<IStorageService>(), cache, tradingSvc, tenantGetter,
        //     logger,);
    }

    [Fact]
    public void SerializeConfiguration_FromJson_Success()
    {
        var cfg = JsonConvert.DeserializeObject<ReportConfiguration>(_testJson);
        cfg.ShouldNotBeNull();
        cfg.MailTo.Any().ShouldBeTrue();
        cfg.Items.Any().ShouldBeTrue();
    }

    [Fact]
    public async Task GenerateMT4EquityDailyReport_BatchFromConfigFiles_ReturnHtmlFiles()
    {
        const string inputFolder = "C:/develop/debug/equity-input";
        const string outputFolder = "C:/develop/debug/equity-output";
        var files = Directory.GetFiles(inputFolder, "*.json");
        files.ShouldNotBeNull();
        files.Any().ShouldBeTrue();

        var requests = new List<ReportConfiguration>();
        foreach (var file in files)
        {
            var request = JsonConvert.DeserializeObject<ReportConfiguration>(await File.ReadAllTextAsync(file));
            request.ShouldNotBeNull();
            request.Name.ShouldNotBeNullOrEmpty();
            requests.Add(request);
        }

        var dateTime = new DateTime(2023, 8, 11, 0, 0, 0, DateTimeKind.Utc);
        const int days = 0;
        for (var i = 0; i <= days; i++)
        {
            foreach (var request in requests)
            {
                // request.From = dateTime.AddDays(i);
                // request.To = request.From.AddDays(1);
                //
                // var dailyReport = await _svc.GenerateMT4EquityDailyReport(_mt4Ctx, request);
                // var monthlyReport = await _svc.GenerateMT4EquityMonthlyReport(_mt4Ctx, request);
                //
                // var title = request.Name + " " + request.From.ToString("yyyy-MM-dd");
                // var html = _svc.GenerateMT4EquityReportHtml(title, dailyReport, monthlyReport);
                // await File.WriteAllTextAsync(outputFolder + "/" + title + ".html", html);
            }
        }
    }

    [Fact]
    public async Task GenerateMT4EquityDailyReport_ForEaConfigJson_ReturnHtmlFiles()
    {
        const string inputFolder = "C:/develop/debug/equity-input/ev";
        const string outputFolder = "C:/develop/debug/equity-output";
        var files = Directory.GetFiles(inputFolder, "*.json");
        files.ShouldNotBeNull();
        files.Any().ShouldBeTrue();

        var requests = new List<ReportConfiguration>();
        foreach (var file in files)
        {
            var request = JsonConvert.DeserializeObject<ReportConfiguration>(await File.ReadAllTextAsync(file));
            request.ShouldNotBeNull();
            request.Name.ShouldNotBeNullOrEmpty();
            requests.Add(request);
        }

        var dateTime = new DateTime(2023, 8, 16, 0, 0, 0, DateTimeKind.Utc);
        foreach (var request in requests)
        {
            request.From = dateTime;
            request.To = request.From.AddDays(1);

            // var dailyReport = await _svc.GenerateMT4EquityDailyReport(_mt4Ctx, request);
            // var monthlyReport = await _svc.GenerateMT4EquityMonthlyReport(_mt4Ctx, request);
            //
            // var title = request.Name + " " + request.From.ToString("yyyy-MM-dd");
            // var html = _svc.GenerateMT4EquityReportHtml(title, dailyReport, monthlyReport);
            // await File.WriteAllTextAsync(outputFolder + "/" + title + ".html", html);
        }
    }

    // [Fact]
    // public async Task GenerateMT4EquityLoginReport_BatchFromConfigFiles_ReturnCsvFiles()
    // {
    //     const string inputFolder = "C:/develop/debug/equity-input";
    //     const string outputFolder = "C:/develop/debug/equity-output";
    //     var files = Directory.GetFiles(inputFolder, "*.json");
    //     files.ShouldNotBeNull();
    //     files.Any().ShouldBeTrue();
    //
    //     var requests = new List<ReportConfiguration>();
    //     foreach (var file in files)
    //     {
    //         var request = JsonConvert.DeserializeObject<ReportConfiguration>(await File.ReadAllTextAsync(file));
    //         request.ShouldNotBeNull();
    //         request.Name.ShouldNotBeNullOrEmpty();
    //         requests.Add(request);
    //     }
    //
    //     foreach (var request in requests)
    //     {
    //         var dict = await _svc.GenerateMT4EquityLoginReport(request);
    //         await File.WriteAllLinesAsync(outputFolder + "/" + request.Name + "_logins.csv", dict);
    //     }
    // }


    [Fact]
    public async Task GenerateMT4EquityDailyReport_ReturnNotNull()
    {
        // Arrange
        var file = "C:/develop/debug/report-request-au-7-13.json";
        var request = JsonConvert.DeserializeObject<ReportConfiguration>(await File.ReadAllTextAsync(file));
        request.ShouldNotBeNull();

        // Act
        // var result = await _svc.GenerateMT4EquityDailyReport(_mt4Ctx, request);
        //
        // // Assert
        // result.ShouldNotBeNull();
        // // result.Count.ShouldBe(1);
        //
        // var firstResult = result.FirstOrDefault();
        // firstResult.ShouldNotBeNull();
        // // firstResult.TotalAmount.ShouldNotBe(0);
        // // firstResult.AccountCount.ShouldNotBe(0);
        //
        // var requestPath =
        //     $"C:/develop/debug/report-request-au-{request.From.Month}-{request.From.Day}.json";
        // var resultPath = $"C:/develop/debug/report-result-au-{request.From.Month}-{request.From.Day}.json";
        // // Arrange
        // var json = JsonConvert.SerializeObject(request);
        // await File.WriteAllTextAsync(requestPath, json);
        //
        // result.ShouldNotBeNull();
        // result.Any().ShouldBeTrue();
        // json = JsonConvert.SerializeObject(result);
        // await File.WriteAllTextAsync(resultPath, json);
    }

    [Fact]
    public async Task GenerateMT4EquityMonthlyReport_ReturnNotNull()
    {
        // Arrange
        var file = "C:/develop/debug/report-request-au-7-13.json";
        var request = JsonConvert.DeserializeObject<ReportConfiguration>(await File.ReadAllTextAsync(file));
        request.ShouldNotBeNull();
        // Act
        // var result = await _svc.GenerateMT4EquityMonthlyReport(_mt4Ctx, request);
        //
        // // Assert
        // result.ShouldNotBeNull();
        // // result.Count.ShouldBe(1);
        //
        // var firstResult = result.FirstOrDefault();
        // firstResult.ShouldNotBeNull();
        // // firstResult.TotalAmount.ShouldNotBe(0);
        // // firstResult.AccountCount.ShouldNotBe(0);
        //
        // var requestPath =
        //     $"C:/develop/debug/report-monthly-request-au-{request.From.Month}-{request.From.Day}.json";
        // var resultPath = $"C:/develop/debug/report-monthly-result-au-{request.From.Month}-{request.From.Day}.json";
        // // Arrange
        // var json = JsonConvert.SerializeObject(request);
        // await File.WriteAllTextAsync(requestPath, json);
        //
        // result.ShouldNotBeNull();
        // result.Any().ShouldBeTrue();
        // json = JsonConvert.SerializeObject(result);
        // await File.WriteAllTextAsync(resultPath, json);
    }

    [Fact]
    public async Task GenerateMT4EquityReportHtml_ReturnNotNull()
    {
        const string file = "C:/develop/debug/report-request-au-7-13.json";
        var request = JsonConvert.DeserializeObject<ReportConfiguration>(await File.ReadAllTextAsync(file));
        request.ShouldNotBeNull();

        var resultPath = $"C:/develop/debug/report-result-au-{request.From.Month}-{request.From.Day}.json";
        var monthlyResultPath =
            $"C:/develop/debug/report-monthly-result-au-{request.From.Month}-{request.From.Day}.json";

        var dailyReports =
            JsonConvert.DeserializeObject<List<MetaTrade4EquityReport>>(await File.ReadAllTextAsync(resultPath)) ??
            new List<MetaTrade4EquityReport>();
        var monthlyReports =
            JsonConvert.DeserializeObject<List<MetaTrade4EquityReport>>(
                await File.ReadAllTextAsync(monthlyResultPath)) ??
            new List<MetaTrade4EquityReport>();

        var html = _svc.GenerateMT4EquityReportHtml(request.Name, dailyReports, monthlyReports);
        html.ShouldNotBeNullOrEmpty();
        var monthlyResultHtml =
            $"C:/develop/debug/report-monthly-result-au-{request.From.Month}-{request.From.Day}.html";
        await File.WriteAllTextAsync(monthlyResultHtml, html);
    }

    [Fact]
    public void GenerateJson()
    {
        var dict = new Dictionary<string, long>
        {
            { "USD", 65000000 },
            { "AUD", 63000000 }
        };
        var str = System.Text.Json.JsonSerializer.Serialize(dict);
        str.ShouldNotBeEmpty();
    }

    private static SymbolReportConfiguration BuildSymbolReportConfiguration() =>
        new()
        {
            Name = "BAC_SYMBOL",
            TimezoneOffset = TimeSpan.FromHours(-7),
            Date = new DateTime(2023, 7, 13, 0, 0, 0, DateTimeKind.Utc),
            MailTo = new List<string> { "feng@bacera.com", "jiehe@bacera.com" },
            Items = new List<SymbolReportConfigurationItem>
            {
                new SymbolReportConfigurationItem
                {
                    Name = "#CHN50",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#HKG50",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#Copper",
                    Size = 25000,
                    Code = "*",
                    Currency = "USD",
                    Category = "comm"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#NG",
                    Size = 10000,
                    Code = "*",
                    Currency = "USD",
                    Category = "comm"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#Soybean",
                    Size = 5000,
                    Code = "*",
                    Currency = "USD",
                    Category = "comm"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#Wheat",
                    Size = 5000,
                    Code = "*",
                    Currency = "USD",
                    Category = "comm"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#Corn",
                    Size = 5000,
                    Code = "*",
                    Currency = "USD",
                    Category = "comm"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#CL",
                    Size = 1000,
                    Code = "*",
                    Currency = "USD",
                    Category = "comm"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "XNGUSD",
                    Size = 10000,
                    Code = "*",
                    Currency = "USD",
                    Category = "comm"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "XTIUSD",
                    Size = 1000,
                    Code = "*",
                    Currency = "USD",
                    Category = "comm"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "XBRUSD",
                    Size = 1000,
                    Code = "*",
                    Currency = "USD",
                    Category = "comm"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#BRN",
                    Size = 1000,
                    Code = "*",
                    Currency = "USD",
                    Category = "comm"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#IBM",
                    Size = 100,
                    Code = "IBM",
                    Currency = "USD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#C",
                    Size = 100,
                    Code = "C",
                    Currency = "USD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#DIS",
                    Size = 100,
                    Code = "DIS",
                    Currency = "USD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#AAPL",
                    Size = 100,
                    Code = "AAPL",
                    Currency = "USD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#AXP",
                    Size = 100,
                    Code = "AXP",
                    Currency = "USD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#BAC",
                    Size = 100,
                    Code = "BAC",
                    Currency = "USD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#INTC",
                    Size = 100,
                    Code = "INTC",
                    Currency = "USD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#KO",
                    Size = 100,
                    Code = "KO",
                    Currency = "USD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#MCD",
                    Size = 100,
                    Code = "MCD",
                    Currency = "USD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#MSFT",
                    Size = 100,
                    Code = "MSFT",
                    Currency = "USD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#QAN.AX",
                    Size = 100,
                    Code = "#QAN.AX",
                    Currency = "AUD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#APT.AX",
                    Size = 100,
                    Code = "#APT.AX",
                    Currency = "AUD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#CSL.AX",
                    Size = 100,
                    Code = "#CSL.AX",
                    Currency = "AUD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#BHP.AX",
                    Size = 100,
                    Code = "#BHP.AX",
                    Currency = "AUD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#6501.T",
                    Size = 100,
                    Code = "#6501.T",
                    Currency = "JPY",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#6502.T",
                    Size = 100,
                    Code = "#6502.T",
                    Currency = "JPY",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#7201.T",
                    Size = 100,
                    Code = "#7201.T",
                    Currency = "JPY",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#7261.T",
                    Size = 100,
                    Code = "#7261.T",
                    Currency = "JPY",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#8306.T",
                    Size = 100,
                    Code = "#8306.T",
                    Currency = "JPY",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#BA",
                    Size = 100,
                    Code = "#BA",
                    Currency = "USD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#0005.HK",
                    Size = 100,
                    Code = "#0005.HK",
                    Currency = "HKD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#0291.HK",
                    Size = 100,
                    Code = "#0291.HK",
                    Currency = "HKD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#0700.HK",
                    Size = 100,
                    Code = "#0700.HK",
                    Currency = "HKD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#0728.HK",
                    Size = 100,
                    Code = "#0728.HK",
                    Currency = "HKD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#0941.HK",
                    Size = 100,
                    Code = "#0941.HK",
                    Currency = "HKD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#1088.HK",
                    Size = 100,
                    Code = "#1088.HK",
                    Currency = "HKD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#1810.HK",
                    Size = 100,
                    Code = "#1810.HK",
                    Currency = "HKD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#1928.HK",
                    Size = 100,
                    Code = "#1928.HK",
                    Currency = "HKD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#2628.HK",
                    Size = 100,
                    Code = "#2628.HK",
                    Currency = "HKD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#3328.HK",
                    Size = 100,
                    Code = "#3328.HK",
                    Currency = "HKD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#3988.HK",
                    Size = 100,
                    Code = "#3988.HK",
                    Currency = "HKD",
                    Category = "stock"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "AUDCAD",
                    Size = 100000,
                    Code = "AUDCAD",
                    Currency = "CAD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "AUDUSD",
                    Size = 100000,
                    Code = "AUDUSD",
                    Currency = "USD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "AUDCHF",
                    Size = 100000,
                    Code = "AUDCHF",
                    Currency = "CHF",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "AUDNZD",
                    Size = 100000,
                    Code = "AUDNZD",
                    Currency = "NZD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "CADCHF",
                    Size = 100000,
                    Code = "CADCHF",
                    Currency = "CHF",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "AUDJPY",
                    Size = 100000,
                    Code = "AUDJPY",
                    Currency = "JPY",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "CADJPY",
                    Size = 100000,
                    Code = "CADJPY",
                    Currency = "JPY",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "CHFJPY",
                    Size = 100000,
                    Code = "CHFJPY",
                    Currency = "JPY",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "EURCAD",
                    Size = 100000,
                    Code = "EURCAD",
                    Currency = "CAD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "EURAUD",
                    Size = 100000,
                    Code = "EURAUD",
                    Currency = "AUD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "EURGBP",
                    Size = 100000,
                    Code = "EURGBP",
                    Currency = "GBP",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "EURJPY",
                    Size = 100000,
                    Code = "EURJPY",
                    Currency = "JPY",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "EURUSD",
                    Size = 100000,
                    Code = "EURUSD",
                    Currency = "USD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "GBPCHF",
                    Size = 100000,
                    Code = "GBPCHF",
                    Currency = "CHF",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "GBPAUD",
                    Size = 100000,
                    Code = "GBPAUD",
                    Currency = "AUD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "GBPCAD",
                    Size = 100000,
                    Code = "GBPCAD",
                    Currency = "CAD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "GBPNZD",
                    Size = 100000,
                    Code = "GBPNZD",
                    Currency = "NZD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "GBPJPY",
                    Size = 100000,
                    Code = "GBPJPY",
                    Currency = "JPY",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "GBPUSD",
                    Size = 100000,
                    Code = "GBPUSD",
                    Currency = "USD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "EURCHF",
                    Size = 100000,
                    Code = "EURCHF",
                    Currency = "CHF",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "NZDCAD",
                    Size = 100000,
                    Code = "NZDCAD",
                    Currency = "CAD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "NZDCHF",
                    Size = 100000,
                    Code = "NZDCHF",
                    Currency = "CHF",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "NZDJPY",
                    Size = 100000,
                    Code = "NZDJPY",
                    Currency = "JPY",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "NZDUSD",
                    Size = 100000,
                    Code = "NZDUSD",
                    Currency = "USD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "USDCAD",
                    Size = 100000,
                    Code = "USDCAD",
                    Currency = "CAD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "USDCHF",
                    Size = 100000,
                    Code = "USDCHF",
                    Currency = "CHF",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "USDJPY",
                    Size = 100000,
                    Code = "USDJPY",
                    Currency = "JPY",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "USDHKD",
                    Size = 100000,
                    Code = "USDHKD",
                    Currency = "HKD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "USDCNH",
                    Size = 100000,
                    Code = "USDCNY",
                    Currency = "CNY",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "EURNZD",
                    Size = 100000,
                    Code = "EURNZD",
                    Currency = "NZD",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "USDMXN",
                    Size = 100000,
                    Code = "USDMXN",
                    Currency = "MXN",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "USDNOK",
                    Size = 100000,
                    Code = "USDNOK",
                    Currency = "NOK",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "USDSEK",
                    Size = 100000,
                    Code = "USDSEK",
                    Currency = "SEK",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "USDTRY",
                    Size = 100000,
                    Code = "USDTRY",
                    Currency = "TRY",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "USDZAR",
                    Size = 100000,
                    Code = "USDZAR",
                    Currency = "ZAR",
                    Category = "fx"
                },

                new SymbolReportConfigurationItem
                {
                    Name = "EURNOK",
                    Size = 100000,
                    Code = "EURNOK",
                    Currency = "NOK",
                    Category = "fx"
                },

                new SymbolReportConfigurationItem
                {
                    Name = "EURSEK",
                    Size = 100000,
                    Code = "EURSEK",
                    Currency = "SEK",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "EURTRY",
                    Size = 100000,
                    Code = "EURTRY",
                    Currency = "TRY",
                    Category = "fx"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "XAGUSD",
                    Size = 100,
                    Code = "XAUUSD",
                    Currency = "USD",
                    Category = "pm"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "XAGUSD",
                    Size = 5000,
                    Code = "XAGUSD",
                    Currency = "USD",
                    Category = "pm"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "XPTUSD",
                    Size = 100,
                    Code = "XPTUSD",
                    Currency = "USD",
                    Category = "pm"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "XPDUSD",
                    Size = 100,
                    Code = "XPDUSD",
                    Currency = "USD",
                    Category = "pm"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#CN300",
                    Size = 300,
                    Code = "*",
                    Currency = "CHN",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#NKD",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#YM",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#US30",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#US100",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#NQ",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#US500",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#ES",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#GER40",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#AUS200",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#EUSTX50",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#FRA40",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#ESP35",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                },
                new SymbolReportConfigurationItem
                {
                    Name = "#UK100",
                    Size = 1,
                    Code = "*",
                    Currency = "USD",
                    Category = "index"
                }
            }
        };

    private string _testJson = """
                               {
                                 "To": "2023-07-14T00:00:00Z",
                                 "From": "2023-07-13T00:00:00Z",
                                 "Name": "BVI BCR",
                                 "Items": [
                                   {
                                     "Name": "SH",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "SH0%"
                                     ],
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [
                                       52191681,
                                       52190004,
                                       52190133,
                                       59990738,
                                       52191029,
                                       52191055,
                                       59992098,
                                       52192601,
                                       52190449,
                                       52191226,
                                       52191121,
                                       59991093,
                                       52190206,
                                       52193019,
                                       52191533,
                                       52191853,
                                       59990815,
                                       59990832,
                                       59991251,
                                       52191166,
                                       59990729,
                                       52191370,
                                       59991257,
                                       59992155,
                                       52192162,
                                       59991123,
                                       52192393,
                                       52191793,
                                       59991613,
                                       59991565,
                                       59990980,
                                       52191716,
                                       52291287,
                                       52190523,
                                       59991237,
                                       59990932,
                                       52190629,
                                       59991111,
                                       52190221,
                                       52190226,
                                       59991723,
                                       52190952,
                                       52191212,
                                       52190378,
                                       52191999,
                                       59991331,
                                       52190238,
                                       59990928,
                                       52190972,
                                       52190047,
                                       52191595,
                                       52191277,
                                       52191268,
                                       52191768,
                                       52190459,
                                       59990925,
                                       59990636,
                                       59990495,
                                       59991309,
                                       59991091,
                                       52191628,
                                       59991267,
                                       59991028,
                                       52190054,
                                       52191369,
                                       52190616,
                                       52192113,
                                       59991056,
                                       59990835,
                                       59991030,
                                       52190246,
                                       52191783,
                                       59991299,
                                       52191858,
                                       59990461,
                                       52190001,
                                       59991698,
                                       59991135,
                                       52191757,
                                       59991689,
                                       59991616,
                                       59990927,
                                       59991298,
                                       59991160,
                                       59991051,
                                       59991256,
                                       59991589,
                                       59990730,
                                       59991272,
                                       59990930,
                                       59990462,
                                       59990631,
                                       52190950,
                                       59992338,
                                       59991380,
                                       59990933,
                                       52190294,
                                       52191909,
                                       59991678,
                                       52190087,
                                       52190107,
                                       52190218,
                                       52191528,
                                       52191981,
                                       59990476,
                                       59990615,
                                       59990660,
                                       59990737,
                                       59990963,
                                       59991376,
                                       59992100
                                     ],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "BJ",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "BJ%"
                                     ],
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [
                                       51091986,
                                       53003364,
                                       53003352,
                                       53003340,
                                       53003183
                                     ],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "BCR",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [
                                       "BA0002"
                                     ],
                                     "IncludeSalesCode": [
                                       "BA%",
                                       "A59"
                                     ],
                                     
                                     "ExcludeAccountNumber": [
                                       53335275,
                                       58890367,
                                       58890377,
                                       58890447,
                                       58891872,
                                       58893177,
                                       55002326,
                                       55002325,
                                       55002324,
                                       59999999,
                                       53003110,
                                       53003994,
                                       53477444,
                                       53987536,
                                       53751167,
                                       53107522,
                                       53674503,
                                       53692092,
                                       53204282,
                                       53730443,
                                       53707701,
                                       53138699,
                                       55475592,
                                       53990459,
                                       53294783,
                                       55557651,
                                       53975715,
                                       53238009,
                                       53989459,
                                       53057331,
                                       55561513,
                                       33916410,
                                       33625826,
                                       33463275,
                                       33853291,
                                       33958552,
                                       33900070,
                                       33896530,
                                       33907649,
                                       33822788,
                                       33339168,
                                       63400773,
                                       63357786,
                                       33778088,
                                       33386297,
                                       33918757,
                                       33363409,
                                       33548085,
                                       33897459,
                                       33177134,
                                       33534404,
                                       33205638,
                                       33399037,
                                       33849809,
                                       33150000,
                                       53220608,
                                       53401792,
                                       53310053,
                                       33389066,
                                       33965180,
                                       33934396,
                                       33988344,
                                       33286712,
                                       33853129,
                                       33607695,
                                       33144723,
                                       55587016,
                                       33420425,
                                       33870105,
                                       33517132,
                                       33041393,
                                       33852345,
                                       33040469,
                                       33004567,
                                       33840342,
                                       33197738,
                                       33925050,
                                       33949447,
                                       33403100,
                                       33438213,
                                       33948457,
                                       33765364
                                     ],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "BKFT",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "AU0001"
                                     ],
                                     
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [
                                       "BKFT",
                                       "BLHK"
                                     ],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "BKRT",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "AU0000"
                                     ],
                                     
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [
                                       "BKRT",
                                       "BKKEa",
                                       "BART"
                                     ],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "AU_USD",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "AU21%"
                                     ],
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "GL",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "GL%"
                                     ],
                                     
                                     "ExcludeAccountNumber": [
                                       53003364,
                                       53003352,
                                       53003340,
                                       53003183,
                                       53759111,
                                       53209807
                                     ],
                                     "IncludeAccountNumber": [
                                       53950515
                                     ],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "DA",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "DA%"
                                     ],
                                     
                                     "ExcludeAccountNumber": [
                                       53679038
                                     ],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "GLB",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "BA0002"
                                     ],
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [
                                       53759111,
                                       53209807,
                                       58801457
                                     ],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "NSY_USD",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [
                                       "AU0042",
                                       "AU0046",
                                       "AU0049",
                                       "AU0055"
                                     ],
                                     "IncludeSalesCode": [
                                       "AU0%",
                                       "AU1%",
                                       "AUSZ%",
                                       "AUWH%",
                                       "AU3002",
                                       "AU2000",
                                       "AUDA%"
                                     ],
                                     
                                     "ExcludeAccountNumber": [
                                       33821114,
                                       33399375,
                                       33753331,
                                       33061767,
                                       33875167,
                                       33849621,
                                       33823657,
                                       33243371,
                                       33525041,
                                       33298693,
                                       33366394,
                                       33153159,
                                       33249782,
                                       33285606,
                                       33397432,
                                       33323827,
                                       33349233,
                                       33361441,
                                       33955706,
                                       33008151,
                                       33758585,
                                       33103973,
                                       33123517,
                                       33307690,
                                       33966788,
                                       33255763,
                                       33717397,
                                       33428542,
                                       33396161,
                                       33364279,
                                       33133179,
                                       33537068,
                                       33807341,
                                       33236697,
                                       33510757,
                                       33419826,
                                       33469014,
                                       33934041,
                                       33925139,
                                       33261471,
                                       33104182,
                                       33555741,
                                       33525214,
                                       33584042,
                                       33337385,
                                       33656603,
                                       33755337,
                                       33364990,
                                       33343927,
                                       33246893,
                                       33528872,
                                       33293179,
                                       33880153,
                                       33938568,
                                       33265274,
                                       33482727,
                                       33210304,
                                       33765767,
                                       33327330,
                                       33641321,
                                       33596372,
                                       33151919,
                                       33733988,
                                       33161797,
                                       33560755,
                                       33085910,
                                       33428529,
                                       33264213,
                                       33580490,
                                       33599662,
                                       33010626,
                                       33625141,
                                       33692522,
                                       33149785,
                                       33486998,
                                       33752860,
                                       33070125,
                                       33252741,
                                       33855855,
                                       33068163,
                                       33422795,
                                       33848249,
                                       33037905,
                                       33764931,
                                       33239361,
                                       33002333,
                                       33720529,
                                       53405321,
                                       53502972,
                                       53528124,
                                       55587016,
                                       33420425,
                                       33870105,
                                       33517132,
                                       33041393,
                                       33852345,
                                       33040469,
                                       33004567,
                                       33840342,
                                       33197738,
                                       33925050,
                                       33897459,
                                       33488359,
                                       33031472,
                                       33107045,
                                       33622883,
                                       33042508,
                                       33430700,
                                       33187168,
                                       33285094,
                                       33305614,
                                       33488106
                                     ],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [
                                       "BKFT",
                                       "BKKE",
                                       "BKRT",
                                       "BART",
                                       "BLHK"
                                     ],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "NSY_MEL",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "AU0042",
                                       "AU0046",
                                       "AU0049",
                                       "AU0055"
                                     ],
                                     
                                     "ExcludeAccountNumber": [
                                       33395562
                                     ],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "NSY_DM",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "NOTHING%"
                                     ],
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [
                                       53405321,
                                       53502972,
                                       53528124
                                     ],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "NSH",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "SH1%",
                                       "SH2%"
                                     ],
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "MY",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "MY0002",
                                       "MY0010",
                                       "MY0011",
                                       "MY0012",
                                       "MY0013",
                                       "MY0014",
                                       "MY0015",
                                       "MY0016",
                                       "MY0017",
                                       "MY0000",
                                       "MY0019",
                                       "MY0018",
                                       "MY0020",
                                       "MY0021",
                                       "MY0022",
                                       "MY0023",
                                       "MY0024",
                                       "MY0001",
                                       "MY0028",
                                       "MY0029"
                                     ],
                                     
                                     "ExcludeAccountNumber": [
                                       33821114,
                                       33604449,
                                       33713182
                                     ],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "VTFX",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "AU3001",
                                       "AU3003"
                                     ],
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "CD",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "CD0%"
                                     ],
                                     
                                     "ExcludeAccountNumber": [
                                       33065877
                                     ],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "RV",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "AU0000"
                                     ],
                                     
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [
                                       "BKKEa",
                                       "BKKE"
                                     ],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "BC",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "BC%"
                                     ],
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "VN",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "VN%"
                                     ],
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "EA",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "EA%"
                                     ],
                                     
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [
                                       "OFF_UTKTD1M",
                                       "OFF_UTKBDM",
                                       "BLGT"
                                     ],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "EA_TW",
                                     "Group": "USD",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "EA%"
                                     ],
                                     
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [
                                       "OFF_UTKTD1M",
                                       "OFF_UTKBDM",
                                       "BLGT"
                                     ],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "NSY_AUD",
                                     "Group": "AUD",
                                     "CurrencyId": 36,
                                     "Description": "",
                                     "ExcludeSalesCode": [
                                       "AU0042",
                                       "AU0046",
                                       "AU0049",
                                       "AU0055"
                                     ],
                                     "IncludeSalesCode": [
                                       "AU0%",
                                       "AU1%",
                                       "AUWH%"
                                     ],
                                     
                                     "ExcludeAccountNumber": [
                                       35099135,
                                       35073693,
                                       35722069,
                                       35882891,
                                       35232611,
                                       35688788,
                                       35620884,
                                       35992490,
                                       35961166,
                                       35338646,
                                       33488359,
                                       33031472,
                                       33107045,
                                       33622883,
                                       33042508,
                                       33430700,
                                       33187168,
                                       33285094
                                     ],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [
                                       "BKKE",
                                       "BKRT",
                                       "BART"
                                     ],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "NSY_MEL",
                                     "Group": "AUD",
                                     "CurrencyId": 36,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "AU0042",
                                       "AU0046",
                                       "AU0049",
                                       "AU0055"
                                     ],
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "AU_AUD",
                                     "Group": "AUD",
                                     "CurrencyId": 36,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "AU21%"
                                     ],
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [
                                       56192296
                                     ],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "BA-AUD",
                                     "Group": "AUD",
                                     "CurrencyId": 36,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "BA1000"
                                     ],
                                     
                                     "ExcludeAccountNumber": [
                                       55165283,
                                       33965180
                                     ],
                                     "IncludeAccountNumber": [
                                       55717340,
                                       35737352,
                                       35900443,
                                       35389149,
                                       35284695,
                                       35124669,
                                       35563946,
                                       55830635,
                                       55735110,
                                       55991653,
                                       55536622,
                                       55687413
                                     ],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "HK",
                                     "Group": "EUR",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "BA1000"
                                     ],
                                     
                                     "ExcludeAccountNumber": [
                                       33965180
                                     ],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "NSY EUR",
                                     "Group": "EUR",
                                     "CurrencyId": 840,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "AU0%",
                                       "AU1%"
                                     ],
                                     
                                     "ExcludeAccountNumber": [
                                       33488359,
                                       33031472,
                                       33107045,
                                       33622883,
                                       33042508,
                                       33430700,
                                       33187168,
                                       33285094
                                     ],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "RV",
                                     "Group": "AUD",
                                     "CurrencyId": 36,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "AU0000"
                                     ],
                                     
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [
                                       "BKKEa",
                                       "BKKE"
                                     ],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "BKRT",
                                     "Group": "AUD",
                                     "CurrencyId": 36,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "AU0000"
                                     ],
                                     
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [
                                       "BKRT",
                                       "BKKEa",
                                       "BART"
                                     ],
                                     "IncludeGroupStartWith": []
                                   },
                                   {
                                     "Name": "EA",
                                     "Group": "AUD",
                                     "CurrencyId": 36,
                                     "Description": "",
                                     "ExcludeSalesCode": [],
                                     "IncludeSalesCode": [
                                       "EA%"
                                     ],
                                     
                                     "ExcludeAccountNumber": [],
                                     "IncludeAccountNumber": [],
                                     "ExcludeGroupStartWith": [
                                       "OFF_UTKTD1M",
                                       "OFF_UTKBDM"
                                     ],
                                     "IncludeGroupStartWith": []
                                   }
                                 ],
                                 "MailCc": [],
                                 "MailTo": ["jiehe@bacera.com"],
                                 "Description": "",
                                 "TimezoneOffset": "00:00:00"
                               }
                               """;
}