using FluentValidation;

namespace Bacera.Gateway.Vendor.UniotcPay;

public class UniotcPayRequestModel
{
    private UniotcPayOptions _options = new();
    public string PaymentNumber { get; set; } = string.Empty;
    public long PaymentId { get; set; }
    public long Amount { get; set; }

    public void ApplyOptions(UniotcPayOptions options)
    {
        _options = options;
    }

    private Dictionary<string, string> BuildForm() =>
        new()
        {
            { "return_type", _options.AppType },
            { "appid", _options.AppId.ToString() },
            { "pay_id", _options.MethodId.ToString() },
            { "amount", $"{Amount:0.00}" },

            { "success_url", _options.SuccessCallbackUri },
            { "error_url", _options.ErrorCallbackUri },

            { "out_uid", PaymentId.ToString() },
            { "out_trade_no", PaymentNumber },
            { "version", _options.Version },
            { "sign_type", _options.SignType },
            { "show_type", _options.ShowMode }
        };

    private Dictionary<string, string> BuildDebugForm() =>
        new()
        {
            { "uid", _options.AppId.ToString() },
            { "money", $"{Amount:0.00}" },
            { "uniqueCode", PaymentNumber },
            { "payType", "1" },
            { "orderId", PaymentId.ToString() },
        };

    public static UniotcPayRequestModel FromDynamic(Supplement.DepositSupplement request) =>
        new()
        {
            Amount = (long)Math.Floor(request.Amount / 100m),
        };

    public Dictionary<string, string> Sign(bool isDebug = false)
    {
        var form = isDebug ? BuildDebugForm() : BuildForm();
        form.Where(x => string.IsNullOrEmpty(x.Value)).ToList().ForEach(x => form.Remove(x.Key));
        var url = form
            .OrderBy(x => x.Key)
            // .Where(x => !string.IsNullOrEmpty(x.Value))
            .Select(x => $"{x.Key}={x.Value}").Aggregate((x, y) => $"{x}&{y}");

        url = url.Trim('&') + $"&key={_options.SecurityKey}";
        var signature = Utils.Md5Hash(url);
        if (isDebug)
            form.Add("signature", signature.ToLower());
        else
            form.Add("sign", signature.ToUpper());
        return form;
    }

    public static bool VerifySign(Dictionary<string, string> form, string callbackSecret, bool isDebug = false)
    {
        var signature = isDebug ? form["signature"] : form["sign"];

        form.Where(x => x.Key == (isDebug ? "signature" : "sign"))
            .ToList()
            .ForEach(x => form.Remove(x.Key));

        var url = form
            .OrderBy(x => x.Key)
            // .Where(x => !string.IsNullOrEmpty(x.Value))
            .Select(x => $"{x.Key}={x.Value}").Aggregate((x, y) => $"{x}&{y}");

        url = url.Trim('&') + $"&key={callbackSecret}";
        var verifySign = Utils.Md5Hash(url);
        verifySign = isDebug ? verifySign.ToLower() : verifySign.ToUpper();
        return verifySign == signature;
    }
}

public class UniotcPayModelValidator : AbstractValidator<UniotcPayRequestModel>
{
    public UniotcPayModelValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(1);
    }
}