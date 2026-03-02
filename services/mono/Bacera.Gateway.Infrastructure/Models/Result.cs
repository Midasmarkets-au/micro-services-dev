using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public class Result : Result<object, object>
{
    public new static Result Error(string message)
        => new() { Message = message, Status = ResultStatus.Error };

    public static Result Error(string message, object data)
        => new() { Message = message, Data = data, Status = ResultStatus.Error };

    public static object IsSuccessTure(string message = "") => new { Message = message, IsSuccess = true };
    public static object IsSuccessFalse(string message = "") => new { Message = message, IsSuccess = false };
    
    public static Result Success(string message)
        => new() { Message = message, Status = ResultStatus.Success };

    public static Result Success(string message, object data)
        => new() { Message = message, Data = data, Status = ResultStatus.Success };

    // public static Result Errors(string message, string error)
    //     => new() { Message = message, Data = new List<string> { error }, Status = ResultStatus.Error };
    //
    // public static Result Errors(string message, IEnumerable<string> errors)
    //     => new() { Message = message, Data = errors, Status = ResultStatus.Error };

    public static Result Of(object data) => new() { Data = data, Status = ResultStatus.Success };
}

public class Result<T> : Result<T, object>
    where T : new()
{
    public new static Result<T> Error(string message) => new() { Message = message, Status = ResultStatus.Error };

    public static Result<T> Error(string message, T data) =>
        new() { Message = message, Data = data, Status = ResultStatus.Error };

    public static Result<T> Of(T data) => new() { Data = data, Status = ResultStatus.Success };
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class Result<T, TC>
    where T : new()
    where TC : new()
{
    [JsonProperty("status"), JsonPropertyName("status")]
    public ResultStatus Status { get; protected set; }

    [JsonProperty("data"), JsonPropertyName("data")]
    public T Data { get; protected set; } = default!;

    [JsonProperty("criteria"), JsonPropertyName("criteria")]
    public TC Criteria { get; protected set; } = default!;

    [JsonProperty("message"), JsonPropertyName("message")]
    public string? Message { get; protected set; }

    [JsonProperty("stat"), JsonPropertyName("stat")]
    public object? Stats { get; protected set; }

    public static Result<T, TC> Error(string message)
        => Error(message, new T(), new TC());

    public static Result<T, TC> Error(string message, T data, TC criteria)
        => new() { Message = message, Data = data, Criteria = criteria, Status = ResultStatus.Error };

    public static Result<T, TC> Of(T data, TC criteria, string? message = "", object? stats = null) => new()
    {
        Data = data,
        Message = message,
        Stats = stats,
        Criteria = criteria,
        Status = ResultStatus.Success,
    };

    public Result<T, TC> SetData(T data)
    {
        Data = data;
        return this;
    }

    public bool IsSuccess() => Status == ResultStatus.Success;
    public bool IsError() => Status == ResultStatus.Error;
}

public enum ResultStatus
{
    Success = 1,
    Error = 2,
}