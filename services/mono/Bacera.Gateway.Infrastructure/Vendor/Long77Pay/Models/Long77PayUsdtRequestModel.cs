namespace Bacera.Gateway.Vendor.Long77Pay;

using FluentValidation;

public class Long77PayUsdtRequestModel
{
    private Long77PayUsdtOptions _options = new();
    private readonly DateTime _currentDateTime = DateTime.UtcNow;
    private readonly string _random = Guid.NewGuid().ToString();

    public long Amount { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public string PaymentNumber { get; set; } = string.Empty;
    private string paymentNumberFormatted => PaymentNumber.Replace("-", "_");
    public long UserId { get; set; }

    public void ApplyOptions(Long77PayUsdtOptions options)
    {
        _options = options;
    }

    public static Long77PayUsdtRequestModel FromDynamic(Supplement.DepositSupplement supplement) =>
        new()
        {
            Amount = (long)Math.Round((supplement.Amount / 100m), 0),
            ReturnUrl = supplement.Request?.ReturnUrl ?? string.Empty,
        };

    private Dictionary<string, string> BuildForm() =>
        new()
        {
            { "partner_id", _options.MerchantId },
            { "timestamp", Utils.ToTimestamp(_currentDateTime).ToString() },
            { "random", _random },
            { "partner_order_code", paymentNumberFormatted },
            { "amount", $"{Amount:0.00}" },
            { "return_url", ReturnUrl },
            { "notify_url", _options.CallbackUri },
            { "order_currency", "0" },
            { "order_language", "en_ww" },

            { "guest_id", Utils.Md5Hash("BCR" + UserId) },
            { "extra_data", string.Empty }
        };

    public Dictionary<string, string> Sign()
    {
        var form = BuildForm();
        var url = form["partner_id"] + ":"
                                     + form["timestamp"] + ":"
                                     + form["random"] + ":"
                                     + form["partner_order_code"] + ":"
                                     + form["order_currency"] + ":"
                                     + form["order_language"] + ":"
                                     + form["guest_id"] + ":"
                                     + form["amount"] + ":"
                                     + form["notify_url"] + ":"
                                     + form["return_url"] + ":"
                                     + form["extra_data"] + ":"
                                     + _options.MerchantSecret;

        var signature = Utils.Md5Hash(url);
        form.Add("sign", signature.ToLower());
        return form;
    }
}

public class Long77PayUsdtRequestModelValidator : AbstractValidator<Long77PayUsdtRequestModel>
{
    public Long77PayUsdtRequestModelValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(1);
    }
}