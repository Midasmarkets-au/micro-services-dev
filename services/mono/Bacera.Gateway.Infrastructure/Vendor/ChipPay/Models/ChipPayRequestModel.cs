using FluentValidation;

namespace Bacera.Gateway.Vendor.ChipPay;

public class ChipPayRequestModel
{
    public CurrencyTypes CurrencyId { get; set; }
    public int Amount { get; set; }
    public string RedirectUrl { get; set; } = string.Empty;
    public string PaymentNumber { get; set; } = string.Empty;

    private ChipPayOptions _options = new();

    public void ApplyOptions(ChipPayOptions options)
    {
        _options = options;
    }

    public static ChipPayRequestModel FromDynamic(Supplement.DepositSupplement supplement) =>
        new()
        {
            Amount = (int)Math.Round((supplement.Amount / 100m), 0),
        };

    private Dictionary<string, string> BuildForm()
    {
        if (CurrencyId != CurrencyTypes.VND)
            return new Dictionary<string, string>
            {
                { "companyId", _options.MerchantId },
                { "companyOrderNum", PaymentNumber },
                { "totalAmount", Amount.ToString() },
                { "syncUrl", _options.CallbackUri },
                { "asyncUrl", !string.IsNullOrEmpty(RedirectUrl) ? RedirectUrl : _options.CallbackUri }
            };

        return new Dictionary<string, string>
        {
            { "companyId", _options.MerchantId },
            { "kyc", "2" },
            { "total", Amount.ToString() },
            { "phone", _options.Phone },
            {
                "coinAmount", "0"
            }, // BigDecimal USDT下单数字货币数量 coinAmount参数换算后法币 金额若不为整数将无条件进位为 整数显示于收银台 (USDT下单数字货币数量(coinAmount和 total 两个字段二选一，当两个字段都填写的时候，优先处理total)) VND        

            { "orderType", "1" }, // Integer 订单类型1、买单 2、卖单  VND
            { "companyOrderNum", PaymentNumber },
            { "coinSign", "USDT" },
            { "payCoinSign", "VND" },
            {
                "orderPayChannel", _options.PaymentChannel == "BankCard" ? "3" : "1"
            }, // Integer if VND, 1: Momo, 3: BankCard    
            {
                "orderTime",
                ((long)Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds, 0)).ToString()
            }, // 订单时间戳（使用当前时间戳，与当前时间相差5分钟视为无效）
            { "syncUrl", _options.CallbackUri },
            { "asyncUrl", !string.IsNullOrEmpty(RedirectUrl) ? RedirectUrl : _options.CallbackUri }
        };
    }

    public Dictionary<string, string> Sign()
    {
        var form = BuildForm();
        form.Where(x => string.IsNullOrEmpty(x.Value)).ToList().ForEach(x => form.Remove(x.Key));
        var url = form
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}={x.Value}").Aggregate((x, y) => $"{x}&{y}");

        url = url.Trim('&');
        var signature = Utils.SignSha265HashWithPkcs8PrivateKey(url, _options.PrivateKey);
        form.Add("sign", signature);
        return form;
    }
}

public class ChipPayModelValidator : AbstractValidator<ChipPayRequestModel>
{
    public ChipPayModelValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(1);
    }
}