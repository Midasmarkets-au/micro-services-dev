namespace Bacera.Gateway.Vendor.UniotcPay;

public class UniotcPayResponseModel
{
    public bool IsSuccess { get; private set; }
    public object? Response { get; private set; }

    public string QrCode { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;

    public string GetReferenceNumber() => Url;
    public string? RedirectUrl => Url;
    public string? Message { get; set; }

    public static UniotcPayResponseModel FromDynamic(dynamic response)
    {
        if (response == null)
            return new UniotcPayResponseModel();
        return new UniotcPayResponseModel
        {
            Response = response,
            Url = response.url?.ToString() ?? string.Empty,
            Message = response.message?.ToString() ?? null,
            QrCode = response.qrcode?.ToString() ?? string.Empty,
            ReferenceNumber = response.order_no?.ToString() ?? string.Empty,
        };
    }

    public UniotcPayResponseModel SetSuccess()
    {
        IsSuccess = true;
        return this;
    }

    public UniotcPayResponseModel SetFail()
    {
        IsSuccess = false;
        return this;
    }
}