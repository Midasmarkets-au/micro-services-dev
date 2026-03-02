
namespace Bacera.Gateway.Vendor.BigPay.Models;

public class BigPayRequestModel
{
    public long AccountId { get; set; }
    public long Amount { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;


    public static BigPayRequestModel FromDynamic(Supplement.DepositSupplement supplement) =>
        new()
        {
            Amount = (long)Math.Round(supplement.Amount / 100m, 0),
            ReturnUrl = supplement.Request?.ReturnUrl ?? string.Empty,
            CancelUrl = supplement.Request?.CancelUrl ?? supplement.Request?.ReturnUrl ?? string.Empty,
        };


    /**
     *   $form['ref'] = $deposit->ref_id;  // ”ref” is OrderId in Partner system
        $form['mt5Id'] = $this->mt5Id;  // ”mt5Id” is mt5 Id of user
        $form['amount'] = $amount;
        $form['callback'] = $this->callback;  // callback
     */
    public object BuildRequestBody() => new
    {
        @ref = $"{PaymentNumber}",
        mt5Id = $"{AccountId}",
        amount = Amount,
        callback = $"{_options.CallbackUri}",
    };

    private Dictionary<string, string> BuildForm() =>
        new()
        {
            { "ref", $"{PaymentNumber}" },
            { "mt5Id", $"{AccountId}" },
            { "amount", $"{Amount}" },
            { "callback", $"{_options.CallbackUri}" },
            // { "returnUrl", $"{ReturnUrl}" },
            // { "cancelUrl", $"{CancelUrl}" },
        };

    public Dictionary<string, string> Sign()
    {
        var form = BuildForm();
        // var url = form
        //     .OrderBy(x => x.Key)
        //     .Select(x => $"{x.Key}={x.Value}").Aggregate((x, y) => $"{x}&{y}");
        //
        // url += _options.MerchantSecret;
        // var signature = Utils.Md5Hash(url);
        // form.Add("signed", signature);
        return form;
    }

    private BigPayOptions _options = new();

    public void ApplyOptions(BigPayOptions options)
    {
        _options = options;
    }
}