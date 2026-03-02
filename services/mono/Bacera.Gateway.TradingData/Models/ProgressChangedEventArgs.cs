namespace Bacera.Gateway.TradingData;

[Serializable]
public class ProgressChangedEventArgs : EventArgs
{
    public AccountProcessEventArgs CurrentEventArgs { get; set; }
    public string AdditionalInfo { get; set; }

    public ProgressChangedEventArgs(AccountProcessEventArgs currentEventArgs, string additionalInfo = "")
    {
        AdditionalInfo = additionalInfo;
        CurrentEventArgs = currentEventArgs;
    }
}