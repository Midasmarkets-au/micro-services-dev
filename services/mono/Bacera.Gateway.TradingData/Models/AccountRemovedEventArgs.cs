namespace Bacera.Gateway.TradingData;

[Serializable]
public class AccountRemovedEventArgs : EventArgs
{
    public int TaskId { get; set; }
    public long ServiceId { get; set; }
    public long Login { get; set; }
    public string AdditionalInfo { get; set; }

    public AccountRemovedEventArgs(long serviceId, long login, int taskId, string additionalInfo = "")
    {
        ServiceId = serviceId;
        Login = login;
        TaskId = taskId;
        AdditionalInfo = additionalInfo;
    }
}