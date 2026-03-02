using System.Collections.Concurrent;
using Bacera.Gateway.TradingData;
using Konsole;

namespace Bacera.Gateway.Console.TransactionImporter;

public class AppEventHandler
{
    private const int TextWide = 64;
    private readonly int _progressBarCount = 32;
    private readonly ConcurrentDictionary<int, ProgressBar> _bars = new();
    private readonly ConcurrentDictionary<long, int> _totalRecord = new();
    private readonly ConcurrentDictionary<int, int> _batchProgress = new();
    private readonly ConcurrentDictionary<int, int> _spinnerIndexes = new();
    private readonly ConcurrentDictionary<long, int> _inactivatedAccount = new();
    private readonly ConcurrentDictionary<long, int> _insertedTransactions = new();
    private readonly ConcurrentDictionary<long, int> _updatedTransactions = new();

    private readonly string _logFileForRemoved;

    public AppEventHandler(int threadCount = -1)
    {
        if (threadCount > 0) _progressBarCount = threadCount;
        _logFileForRemoved =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"removed-{DateTime.Now:yyyyMMdd}.log");
    }

    public void OnTradeServerChanged(TradeServerChangedEventArgs args)
    {
        var hasValue = _totalRecord.ContainsKey(args.ServerId);
        if (!hasValue)
        {
            _totalRecord.TryAdd(args.ServerId, args.TotalAccount);
        }

        foreach (var batchProgress in _batchProgress)
        {
            _batchProgress.TryRemove(batchProgress.Key, out _);
        }

        _inactivatedAccount.TryAdd(args.ServerId, 0);
        _insertedTransactions.TryAdd(args.ServerId, 0);
        _updatedTransactions.TryAdd(args.ServerId, 0);

        System.Console.WriteLine();
        ConsoleUtility.PrintWithColor(
            $"    {args.Platform} Service [{args.ServerId}] : {args.ServerId} [{args.AdditionalInfo}]",
            ConsoleUtility.UpdateTextColor);
        System.Console.WriteLine();
    }

    public static void OnTenantChanged(TenantChangedEventArgs args)
    {
        System.Console.WriteLine();
        ConsoleUtility.PrintWithColor($"  Tenant [{args.TenantId}] :{args.Name} [{args.AdditionalInfo}]",
            ConsoleColor.Yellow);
        System.Console.WriteLine();
    }

    public void OnTransactionProcessed(TransactionProcessedEventArgs args)
    {
        _updatedTransactions[args.ServiceId] += args.Updated;
        _insertedTransactions[args.ServiceId] += args.Inserted;
    }

    public void OnAccountProcessStarted(AccountProcessEventArgs args)
    {
        var barId = args.TaskId % _progressBarCount;

        var exists = _bars.ContainsKey(barId);
        if (!exists)
        {
            _bars.TryAdd(barId, new ProgressBar(args.TotalRecords,
                TextWide, '-'));
        }

        var existsBatch = _batchProgress.ContainsKey(args.TaskId);
        if (!existsBatch)
        {
            _batchProgress.TryAdd(args.TaskId, args.CurrentRecords);
        }
        else
        {
            _batchProgress[args.TaskId] = args.CurrentRecords;
        }


        _bars[barId].Refresh(args.CurrentRecords, $"{args.TaskId,3} " +
                                                  $"[{args.CurrentRecords,3}/{args.TotalRecords,3}] " +
                                                  $"Login: {args.Login} " + args.AdditionalInfo);
    }

    public void OnAccountProcessCompleted(AccountProcessEventArgs args)
    {
        var barId = args.TaskId % _progressBarCount;
        var exists = _bars.ContainsKey(barId);
        if (!exists)
        {
            var newProgressBar = new ProgressBar(args.TotalRecords,
                TextWide, '-');
            _bars.TryAdd(barId, newProgressBar);
        }

        if (_bars[barId].Max != args.TotalRecords)
        {
            _bars[barId].Max = Math.Max(_bars[barId].Max, args.TotalRecords);
        }

        var existsBatch = _batchProgress.ContainsKey(args.TaskId);
        if (!existsBatch)
        {
            _batchProgress.TryAdd(args.TaskId, args.CurrentRecords);
        }
        else
        {
            _batchProgress[args.TaskId] = args.CurrentRecords;
        }

        _totalRecord.TryGetValue(args.ServiceId, out var total);

        _bars[barId].Refresh(args.CurrentRecords, $"{args.TaskId,3} " +
                                                  $"[{args.CurrentRecords,3}/{args.TotalRecords,3}] " +
                                                  $"Login: {args.Login} " +
                                                  $"{(args.CurrentRecords == args.TotalRecords ? "✓" : "")}");

        if (_bars.Count == _progressBarCount)
        {
            _bars.TryAdd(_progressBarCount + 1, new ProgressBar(total, TextWide, '-'));
        }

        if (_bars.Count <= _progressBarCount)
            return;

        var processed = _batchProgress.ToList().Sum(x => x.Value);
        _bars[_progressBarCount + 1].Refresh(processed, $"[{processed,4} / {total,4}] " +
                                                        $"Trans [I:{_insertedTransactions[args.ServiceId]}" +
                                                        " / " +
                                                        $"U:{_updatedTransactions[args.ServiceId]}] " +
                                                        (_inactivatedAccount[args.ServiceId] > 0
                                                            ? $"Not found: {_inactivatedAccount[args.ServiceId]}"
                                                            : "")
        );
    }

    public void OnAccountRemoved(AccountRemovedEventArgs args)
    {
        _inactivatedAccount[args.ServiceId]++;
        File.AppendAllText(_logFileForRemoved, $"{args.ServiceId},{args.Login},{args.AdditionalInfo}\n");
    }

    public static void OnCompleted(TimeSpan elapsed)
    {
        ConsoleUtility.PrintWithColor("All done!", ConsoleColor.Green);
        System.Console.WriteLine();
        System.Console.WriteLine("Elapsed time: {0}", elapsed);
        System.Console.WriteLine();
    }

    private static readonly char[] _spinnerChars = { '⠋', '⠙', '⠹', '⠼', '⠴', '⠦', '⠧', '⠏' };

    private string updateSpinner(int taskId)
    {
        if (!_spinnerIndexes.ContainsKey(taskId))
        {
            _spinnerIndexes.TryAdd(taskId, 0);
        }

        var spinner = _spinnerChars[_spinnerIndexes[taskId]];
        _spinnerIndexes[taskId] = (_spinnerIndexes[taskId] + 1) % _spinnerChars.Length;
        return spinner.ToString();
    }

    public void OnProgressChanged(ProgressChangedEventArgs args)
    {
        var newArgs = args.CurrentEventArgs;
        newArgs.AdditionalInfo = updateSpinner(args.CurrentEventArgs.TaskId) + " " + args.AdditionalInfo;
        OnAccountProcessStarted(newArgs);
    }
}

public static class ConsoleUtility
{
    // init console
    public const ConsoleColor NormalTextStyle = ConsoleColor.Gray;
    public const ConsoleColor SuccessTextColor = ConsoleColor.DarkGreen;
    public const ConsoleColor InfoTextColor = ConsoleColor.DarkGray;
    public const ConsoleColor UpdateTextColor = ConsoleColor.DarkMagenta;

    public static void PrintWithColor(string text, ConsoleColor foregroundColor = NormalTextStyle)
    {
        System.Console.ForegroundColor = foregroundColor;
        System.Console.Write(text);
        System.Console.ResetColor();
    }

    public static void PrintWithColor(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
    {
        System.Console.ForegroundColor = foregroundColor;
        System.Console.BackgroundColor = backgroundColor;
        System.Console.Write(text);
        System.Console.ResetColor();
    }
}