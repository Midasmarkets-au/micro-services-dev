namespace Bacera.Gateway.TradingData;

partial class TradingDataImporter
{
    public event EventHandler<ProgressChangedEventArgs> ProgressChanged = delegate { };

    private void onProgressChanged(ProgressChangedEventArgs args)
    {
        ProgressChanged.Invoke(this, args);
    }

    public event EventHandler<TradeServerChangedEventArgs> TradeServerChanged = delegate { };

    private void onTradeServerChanged(TradeServerChangedEventArgs args)
    {
        TradeServerChanged.Invoke(this, args);
    }

    public event EventHandler<TenantChangedEventArgs> TenantChanged = delegate { };

    private void onTenantChanged(TenantChangedEventArgs args)
    {
        TenantChanged.Invoke(this, args);
    }

    public event EventHandler<AccountProcessEventArgs> AccountProcessStarted = delegate { };

    private void onAccountProcessStarted(AccountProcessEventArgs args)
    {
        AccountProcessStarted.Invoke(this, args);
    }

    public event EventHandler<AccountProcessEventArgs> AccountProcessCompleted = delegate { };

    private void onAccountProcessCompleted(AccountProcessEventArgs args)
    {
        AccountProcessCompleted.Invoke(this, args);
    }

    public event EventHandler<AccountRemovedEventArgs> AccountRemoved = delegate { };

    private void onAccountRemoved(AccountRemovedEventArgs args)
    {
        AccountRemoved.Invoke(this, args);
    }

    public event EventHandler<TransactionProcessedEventArgs> TransactionProcessed = delegate { };

    private void onTransactionProcessed(TransactionProcessedEventArgs args)
    {
        TransactionProcessed.Invoke(this, args);
    }
}