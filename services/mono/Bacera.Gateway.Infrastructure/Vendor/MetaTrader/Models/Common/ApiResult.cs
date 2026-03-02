namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public abstract class ApiResult : ApiResult<BaseResponse>
{
    public static ApiResult<T> Build<T>() where T : new()
        => new() { Data = new T() };
}

public class ApiResult<T> where T : new()
{
    public ApiResultStatus Status { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public T Data { get; set; } = new();
    public string RequestId { get; private set; } = string.Empty;

    public ApiResult<T> SetRequestId(string requestId)
    {
        RequestId = requestId;
        return this;
    }

    public ApiResult<T> SetData(T data)
    {
        Data = data;
        return this;
    }

    public ApiResult<T> SetStatus(ApiResultStatus status, string message = "")
    {
        Status = status;
        Message = message;
        return this;
    }

    public static ApiResult<T> Error(ApiResultStatus status, string message = "")
        => new() { Data = new T(), Message = message, Status = status };

    public static ApiResult<T> Build()
        => new() { Data = new T() };

    public bool IsSuccessStatus() => Status == ApiResultStatus.Success;
}

public enum ApiResultStatus
{
    Success = 1,
    InvalidActionType = 0,
    ConnectionIssue = -1,
    InvalidHash = -2,
    InvalidIncomingParameters = -3,
    NoValidDataOnServer = -4,
    SystemError = -5,
}