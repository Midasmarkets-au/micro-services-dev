using System.Diagnostics;
using System.Globalization;
using System.Text;
using Bacera.Gateway.TradingData;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Console.TransactionImporter;

public static class Program
{
    [Serializable]
    public class Options
    {
        [Option('t', "tenant", Required = false, Default = null, HelpText = "Tenant Id")]
        public long? TenantId { get; set; }

        [Option('s', "service", Required = false, Default = null, HelpText = "Trade Service Id")]
        public int? TradeServiceId { get; set; }

        [Option('l', "login", Required = false, Default = null, HelpText = "Account Login")]
        public IEnumerable<long>? AccountNumbers { get; set; }

        [Option('i', "id", Required = false, Default = null, HelpText = "Account Id")]
        public IEnumerable<long>? AccountIds { get; set; }

        [Option('m', "thread", Required = false, Default = null, HelpText = "Multi-thread count")]
        public int? Thread { get; set; }

        [Option('d', "datetime", Required = false, Default = null, HelpText = "Import since date time")]
        public string? DateTime { get; set; }

        [Option("sync", Required = false, Default = null,
            HelpText = "Import since last sync or assign date time with -d parameter")]
        public bool? Sync { get; set; }

        [Option('u', "no-ui", Required = false, Default = false, HelpText = "Without UI")]
        public bool? NoUi { get; set; }

        public TradingDataImporter.Options ToServiceOptions()
            => new()
            {
                TenantId = TenantId,
                TradeServiceId = TradeServiceId,
                AccountNumbers = AccountNumbers,
                AccountIds = AccountIds,
                Thread = Thread,
            };
    }

    static async Task Main(string[] args)
    {
        // init app

        // init config
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        IConfiguration config = builder.Build();

        // init logger
        using var loggerFactory = LoggerFactory.Create(options =>
        {
            options
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
                // .AddConsole();
                ;
        });

        // configure services
        var centralDbConnectionString = config.GetConnectionString("DefaultConnection");
        var tenantDbConnectionTemplateString = config.GetConnectionString("TenantConnectionTemplate");

        if (string.IsNullOrEmpty(centralDbConnectionString))
        {
            throw new Exception($"Central database connection string is not set");
        }

        if (string.IsNullOrEmpty(tenantDbConnectionTemplateString))
        {
            throw new Exception($"Tenant's database connection string is not set");
        }

        // init console
        System.Console.OutputEncoding = Encoding.UTF8;

        // parse args
        var options = Parser.Default.ParseArguments<Options>(args)
            .WithParsed(runOptions)
            .WithNotParsed(handleParseError);

        // init services
        var svc = new TradingDataImporter(centralDbConnectionString, tenantDbConnectionTemplateString,
            options.Value.ToServiceOptions());

        if (options.Value.NoUi == true)
        {
        }
        else
        {
            var eventHandler = new AppEventHandler(svc.GetThreadCount());
            svc.AccountProcessCompleted += (_, arg) => eventHandler.OnAccountProcessCompleted(arg);
            svc.AccountProcessStarted += (_, arg) => eventHandler.OnAccountProcessStarted(arg);
            svc.AccountRemoved += (_, arg) => eventHandler.OnAccountRemoved(arg);
            svc.TransactionProcessed += (_, arg) => eventHandler.OnTransactionProcessed(arg);
            svc.ProgressChanged += (_, arg) => eventHandler.OnProgressChanged(arg);
            // /    eventHandler.UpdateSpinner(arg.TaskId, arg.ProcessedRecords, arg.TotalRecords);
            svc.TenantChanged += (_, arg) => AppEventHandler.OnTenantChanged(arg);
            svc.TradeServerChanged += (_, arg) => eventHandler.OnTradeServerChanged(arg);
        }


        // run
        System.Console.CursorVisible = false;
        var stopwatch = new Stopwatch();

        stopwatch.Start();

        if (options.Value.Sync == true)
        {
            if (options.Value.DateTime != null)
            {
                var since = DateTime.Parse(options.Value.DateTime);
                await svc.ImportSinceAsync(since);
            }
            else
            {
                await svc.ImportSinceAsync();
            }
        }
        else
        {
            await svc.Import();
        }

        //await svc.ImportByDaysAsync();
        // await svc.ImportByDaysAsync(DateTime.UtcNow.AddHours(-8));
        //await svc.Import();
        stopwatch.Stop();
        var elapsed = stopwatch.Elapsed;
        System.Console.CursorVisible = true;
        AppEventHandler.OnCompleted(elapsed);
    }

    private static void runOptions(Options opts)
    {
        //handle options
        ConsoleUtility.PrintWithColor(
            $"Start importing trades " + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture), ConsoleColor.Green);
        System.Console.WriteLine();
        if (opts.TenantId.HasValue)
        {
            ConsoleUtility.PrintWithColor("  Tenant: ".PadRight(12));
            ConsoleUtility.PrintWithColor($"{opts.TenantId}".PadLeft(10), ConsoleColor.Green);
            System.Console.WriteLine();
        }

        if (opts.TradeServiceId.HasValue)
        {
            ConsoleUtility.PrintWithColor("  Service: ".PadRight(12));
            ConsoleUtility.PrintWithColor($"{opts.TradeServiceId}".PadLeft(10), ConsoleColor.Green);
            System.Console.WriteLine();
        }

        if (opts.AccountNumbers != null && opts.AccountNumbers.Any())
        {
            ConsoleUtility.PrintWithColor("  Login: ".PadRight(12));
            ConsoleUtility.PrintWithColor(
                opts.AccountNumbers.Count() < 6
                    ? $"{string.Join(",", opts.AccountNumbers)}".PadLeft(10)
                    : $" Total ({opts.AccountNumbers.Count()})".PadLeft(10),
                ConsoleColor.Green);
            System.Console.WriteLine();
        }

        if (opts.AccountIds == null || !opts.AccountIds.Any()) return;

        ConsoleUtility.PrintWithColor($"  Account: ".PadRight(12));
        ConsoleUtility.PrintWithColor(
            opts.AccountIds.Count() < 6
                ? $"{string.Join(",", opts.AccountIds)}".PadLeft(10)
                : $" Total ({opts.AccountIds.Count()})".PadLeft(10), ConsoleColor.Green);
        System.Console.WriteLine();
    }

    private static void handleParseError(IEnumerable<Error> errs)
    {
        //handle errors
    }
}