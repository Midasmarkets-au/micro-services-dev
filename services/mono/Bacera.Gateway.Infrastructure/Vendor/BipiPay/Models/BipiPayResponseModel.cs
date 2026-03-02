using Bacera.Gateway;

public class BipiPayResponseModel
{
    public bool IsSuccess { get; private set; }
    public object? Response { get; private set; }
    public string ReferenceNumber { get; set; } = string.Empty;

    public string GetReferenceNumber() => ReferenceNumber;
    public string? RedirectUrl { get; set; }
    public string? Message { get; set; }

    public static BipiPayResponseModel FromDynamic(dynamic response) =>
        new()
        {
            Response = response,
            ReferenceNumber = response.paytxno?.ToString() ?? string.Empty
        };

    public BipiPayResponseModel SetSuccess()
    {
        IsSuccess = true;
        return this;
    }

    public BipiPayResponseModel SetFail()
    {
        IsSuccess = false;
        return this;
    }

    public BipiPayResponseModel SetResponse(object response)
    {
        Response = response;
        return this;
    }
}