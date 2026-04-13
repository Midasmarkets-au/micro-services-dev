using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway.DTO;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class DepositCreatedResponseModel
{
    public bool IsSuccess { get; set; }
    public bool ShowMessage { get; set; }
    public string Action { get; set; } = string.Empty;

    public string? RedirectUrl { get; set; }
    public string? TextForQrCode { get; set; }
    public string? EndPoint { get; set; }
    public string? Message { get; set; }
    public string? Reference { get; set; }

    /// <summary>3rd-party QR tunnel <c>transactionId</c> (same as <see cref="Reference"/> when present).</summary>
    public string? TransactionId { get; set; }

    public string? Instruction { get; set; }

    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public string PaymentNumber { get; set; } = string.Empty;

    public object? Form { get; set; }

    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public Func<Deposit, Task> CreatedCbHandler { get; set; } = deposit => Task.CompletedTask;

    public object? Info { get; set; }

    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public long DepositId { get; set; }

    public static DepositCreatedResponseModel Fail(string? message = null, bool showMessage = false)
        => new()
        {
            IsSuccess = false,
            ShowMessage = showMessage,
            Message = message ?? "Failed to create deposit"
        };

    public static DepositCreatedResponseModel Success() => new()
        { IsSuccess = true, Action = PaymentResponseActionTypes.None, PaymentNumber = Payment.GenerateNumber() };
}