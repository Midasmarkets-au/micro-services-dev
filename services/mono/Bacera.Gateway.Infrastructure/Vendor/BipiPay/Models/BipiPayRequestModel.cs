using FluentValidation;

namespace Bacera.Gateway.Vendor.BipiPay;

public class BipiPayRequestModel
{
    public decimal Amount { get; set; }
    public decimal UsdAmount { get; set; }
    public string CurrencyName { get; set; } = "RMB";
    public string RedirectUrl { get; set; } = string.Empty;
    public string PaymentNumber { get; set; } = string.Empty;
    public long PartyId { get; set; }

    private BipiPayOptions _options = new();

    public void ApplyOptions(BipiPayOptions options)
    {
        _options = options;
    }

    public static BipiPayRequestModel FromDynamic(Supplement.DepositSupplement supplement) =>
        new()
        {
            Amount = (int)Math.Round((supplement.Amount / 100m), 0),
        };

    private Dictionary<string, string> BuildForm() =>
        new()
        {
            { "txcode", PaymentNumber },
            { "merchant", _options.MerchantId },
            { "customer", PartyId.ToString() },
            { "currency", CurrencyName },
            { "amount", $"{Amount:0.00}" },
            { "usdamount", $"{UsdAmount:0.00}" }
        };

    public Dictionary<string, string> Sign()
    {
        var form = BuildForm();
        var url = form
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}={x.Value}").Aggregate((x, y) => $"{x}&{y}");

        url += _options.MerchantSecret;
        var signature = Utils.Sha512Hash(url);
        form.Add("signed", signature);
        return form;
    }
}

public class BipiPayModelValidator : AbstractValidator<BipiPayRequestModel>
{
    public BipiPayModelValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(1000);
    }
}