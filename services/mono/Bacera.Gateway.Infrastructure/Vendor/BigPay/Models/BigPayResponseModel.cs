using System.Globalization;
using Bacera.Gateway;

public class BigPayResponseModel
{
    public string Id { get; set; } = string.Empty;
    public string HashId { get; set; } = string.Empty;
    public string Ref { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

    public DateTime CreatedOn => DateTime.Parse(CreatedAt);

    public bool IsSuccess { get; private set; }
    public object? Response { get; private set; }
    public string ReferenceNumber { get; set; } = string.Empty;

    public string GetReferenceNumber() => ReferenceNumber;
    public string? RedirectUrl { get; set; }
    public string? Message { get; set; }


    public static BigPayResponseModel FromDynamic(dynamic response)
    {
        if (response == null)
            return new BigPayResponseModel();
        return new BigPayResponseModel
        {
            Id = response.id ?? string.Empty,
            Ref = response.@ref ?? string.Empty,
            Code = response.code ?? string.Empty,
            HashId = response.hashId ?? string.Empty,
            Amount = response.amount ?? 0,
            Status = response.status ?? string.Empty,
            RedirectUrl = response.redirectUrl ?? string.Empty,
            CreatedAt = response.createAt ?? DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
            Response = response,
            IsSuccess = response.status == "PENDING",
            ReferenceNumber = response.code ?? string.Empty,
        };
    }


    public BigPayResponseModel SetFail()
    {
        IsSuccess = false;
        return this;
    }


    public BigPayResponseModel SetSuccess()
    {
        IsSuccess = true;
        return this;
    }


    public BigPayResponseModel SetResponse(object response)
    {
        Response = response;
        return this;
    }
}