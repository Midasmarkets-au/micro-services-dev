using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway.DTO;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class PayoutResponseModel
{
    public bool IsSuccess { get; set; }
    public bool ShowMessage { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Message { get; set; }

    public object? Form { get; set; }

    public string ResponseJson { get; set; } = "{}";
    //
    // [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    // public Func<Deposit, Task> CreatedCbHandler { get; set; } = deposit => Task.CompletedTask;

    public static PayoutResponseModel Fail(string? message = null, bool showMessage = false)
        => new()
        {
            IsSuccess = false,
            ShowMessage = showMessage,
            Message = message ?? "Failed to create deposit"
        };

    public static PayoutResponseModel Success() => new()
        { IsSuccess = true, Action = PaymentResponseActionTypes.None };
}