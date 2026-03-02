using FluentValidation;

namespace Bacera.Gateway.Vendor.Long77Pay;

public class Long77PayVARequestModel
{
    private Long77PayOptions _options = new();
    private readonly DateTime _currentDateTime = DateTime.UtcNow;
    private readonly string _random = Guid.NewGuid().ToString();

    public long Amount { get; set; } // In VND (not cents)
    public string ReturnUrl { get; set; } = string.Empty;
    public string PaymentNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PayeeName { get; set; } = string.Empty;
    public string ExtraData { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty; // Client IP address (required by API)

    public void ApplyOptions(Long77PayOptions options)
    {
        _options = options;
    }

    public Dictionary<string, string> Sign()
    {
        var form = BuildForm();
        var url = $"{form["partner_id"]}:{form["timestamp"]}:{form["random"]}:" +
                  $"{form["partner_order_code"]}:{form["amount"]}:{form["customer_name"]}:" +
                  $"{form["payee_name"]}:{form["notify_url"]}:{form["return_url"]}:" +
                  $"{form["extra_data"]}:{_options.MerchantSecret}";

        var signature = Utils.Md5Hash(url);
        form.Add("sign", signature.ToLower());
        return form;
    }

    private Dictionary<string, string> BuildForm() =>
        new()
        {
            { "partner_id", _options.MerchantId },
            { "timestamp", Utils.ToTimestamp(_currentDateTime).ToString() },
            { "random", _random },
            { "partner_order_code", PaymentNumber },
            { "amount", Amount.ToString() },
            { "notify_url", _options.VACallbackUri },
            { "return_url", ReturnUrl },
            { "customer_name", CustomerName },
            { "payee_name", PayeeName },
            { "extra_data", ExtraData },
            { "ip", Ip } // IP address field (required by API, not included in signature)
        };
}

public class Long77PayVARequestModelValidator : AbstractValidator<Long77PayVARequestModel>
{
    public Long77PayVARequestModelValidator()
    {
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(50000).LessThanOrEqualTo(4000000000)
            .WithMessage("Amount must be between 50,000 and 4,000,000,000 VND");
        RuleFor(x => x.PaymentNumber).NotEmpty();
    }
}

