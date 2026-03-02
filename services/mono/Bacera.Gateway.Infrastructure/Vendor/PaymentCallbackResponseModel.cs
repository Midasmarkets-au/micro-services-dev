namespace Bacera.Gateway.Vendor;

public class PaymentCallbackResponseModel
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object ResponseData { get; set; } = new();

    public static PaymentCallbackResponseModel SetSuccess(object responseData)
    {
        return new PaymentCallbackResponseModel
        {
            Success = true,
            ResponseData = responseData
        };
    }

    public static PaymentCallbackResponseModel SetFailed(string message)
    {
        return new PaymentCallbackResponseModel
        {
            Success = false,
            Message = message
        };
    }
}