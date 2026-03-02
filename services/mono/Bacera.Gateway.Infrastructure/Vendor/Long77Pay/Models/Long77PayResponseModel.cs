namespace Bacera.Gateway.Vendor.Long77Pay;

public class Long77PayResponseModel
{
    public bool IsSuccess => Code == 200;
    public int Code { get; set; }
    public object? Response { get; private set; }
    public string ReferenceNumber { get; set; } = string.Empty;

    public string GetReferenceNumber() => ReferenceNumber;
    public string? RedirectUrl { get; set; }
    public string? Message { get; set; }

    public static Long77PayResponseModel FromDynamic(dynamic response)
    {
        return new Long77PayResponseModel
        {
            Response = response,
            Code = response.code != null ? (int)response.code : -1,
            Message = (string)response.SelectToken("msg") ?? null,
            RedirectUrl = (string)response.SelectToken("data.payment_url") ?? null,
            ReferenceNumber = (string)response.SelectToken("data.system_order_code") ?? string.Empty,
        };
    }

    public static Long77PayResponseModel Empty(int code) =>
        new()
        {
            Code = code,
        };

    public Long77PayResponseModel SetSuccess()
    {
        Code = 200;
        return this;
    }
}