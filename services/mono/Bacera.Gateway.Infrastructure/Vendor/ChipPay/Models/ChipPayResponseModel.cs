using System.Text.Json.Serialization;
using System.Web;
using Bacera.Gateway;
using Bacera.Gateway.Vendor.UniotcPay;

public class ChipPayResponseModel
{
    public bool IsSuccess { get; private set; }
    public object? Response { get; private set; }
    public string ReferenceNumber { get; set; } = string.Empty;

    public string GetReferenceNumber() => ReferenceNumber;
    public string? RedirectUrl { get; set; }

    public string? Message { get; set; }

    public static ChipPayResponseModel FromDynamic(dynamic response)
    {
        if (response.success != true)
        {
            return new ChipPayResponseModel
            {
                Response = response,
                RedirectUrl = null,
                ReferenceNumber = string.Empty,
                Message = response?.msg?.ToString() ?? null,
            }.SetFail();
        }

        var result = new ChipPayResponseModel
        {
            Response = response,
            Message = response?.msg?.ToString() ?? null,
            ReferenceNumber = response?.data?.intentOrderNo?.ToString()
                              ?? response?.data?.orderNo?.ToString()
                              ?? string.Empty,
            RedirectUrl = response?.data?.link?.ToString() ?? string.Empty
        }.SetSuccess();

        if (!string.IsNullOrEmpty(result.ReferenceNumber)) return result;
        if (string.IsNullOrEmpty(result.RedirectUrl)) return result;

        // try to parse reference number from redirect url when VND
        var uri = new Uri(result.RedirectUrl);
        var fragment = uri.Fragment;
        // Removing the '#' character and the preceding path
        var query = fragment[fragment.IndexOf('?')..];
        var queryParams = HttpUtility.ParseQueryString(query);
        result.ReferenceNumber = queryParams["orderNo"] ?? string.Empty;
        return result;
    }

    public ChipPayResponseModel SetSuccess()
    {
        IsSuccess = true;
        return this;
    }

    public ChipPayResponseModel SetFail()
    {
        IsSuccess = false;
        return this;
    }
}