namespace Bacera.Gateway.Vendor.Long77Pay;

using FluentValidation;

public class Long77PayRequestModel
{
    private Long77PayOptions _options = new();
    private readonly DateTime _currentDateTime = DateTime.UtcNow;
    private readonly string _random = Guid.NewGuid().ToString();

    public long Amount { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public string PaymentNumber { get; set; } = string.Empty;

    public void ApplyOptions(Long77PayOptions options)
    {
        _options = options;
    }

    public static Long77PayRequestModel FromDynamic(Supplement.DepositSupplement supplement) =>
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
            { "partner_order_code", PaymentNumber },
            { "amount", $"{Amount:0.00}" },
            { "return_url", ReturnUrl },
            { "notify_url", _options.CallbackUri },

            { "customer_name", string.Empty },
            { "payee_name", string.Empty },
            { "extra_data", string.Empty }
        };

    public Dictionary<string, string> Sign()
    {
        var form = BuildForm();
        var url = form["partner_id"] + ":" + form["timestamp"] + ":" + form["random"] + ":" +
                  form["partner_order_code"] + ":" + form["amount"] + ":" + form["customer_name"] + ":" +
                  form["payee_name"] + ":" + form["notify_url"] + ":" + form["return_url"] + ":" + form["extra_data"] +
                  ":" + _options.MerchantSecret;

        var signature = Utils.Md5Hash(url);
        form.Add("sign", signature.ToLower());
        return form;
    }
}

public class Long77PayRequestModelValidator : AbstractValidator<Long77PayRequestModel>
{
    public Long77PayRequestModelValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(1000);
    }
}