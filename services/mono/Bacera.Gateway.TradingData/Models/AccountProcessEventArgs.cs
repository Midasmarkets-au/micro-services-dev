namespace Bacera.Gateway.TradingData;

[Serializable]
public class AccountProcessEventArgs : EventArgs
{
    public int TaskId { get; set; }
    public long Login { get; set; }
    public long ServiceId { get; set; }
    public int CurrentRecords { get; set; }
    public int TotalRecords { get; set; }
    public string AdditionalInfo { get; set; }

    public AccountProcessEventArgs(long serviceId, long login,
        int currentRecords, int totalRecords, int taskId = 0,
        string additionalInfo = "")
    {
        TaskId = taskId;
        Login = login;
        CurrentRecords = currentRecords;
        TotalRecords = totalRecords;
        ServiceId = serviceId;
        AdditionalInfo = additionalInfo;
    }
}