using FluentValidation;

namespace Bacera.Gateway;

public class TradeAccountCreateRequestModel
{
    public int Leverage { get; set; }
    public long AccountId { get; set; }
    public int ServiceId { get; set; }
    public int CurrencyId { get; set; }
    public string Group { get; set; } = string.Empty;

    public List<string> ExtraTags { get; set; } = [];
}

public class TradeAccountCreateRequestModelValidator : AbstractValidator<TradeAccountCreateRequestModel>
{
    public TradeAccountCreateRequestModelValidator()
    {
        RuleFor(x => x.Group).NotEmpty().MinimumLength(3).MaximumLength(64);
        RuleFor(x => x.CurrencyId).GreaterThan(0).Must(x => CurrencyUtility.AllCurrencyIds().Contains(x));
        RuleFor(x => x.Leverage).GreaterThan(0);
        RuleFor(x => x.ServiceId).NotEmpty().GreaterThan(0);
        RuleFor(x => x.AccountId).NotEmpty().GreaterThan(0);
        ;
    }
}