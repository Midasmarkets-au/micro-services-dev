using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Bacera.Gateway.Web.Request;

public class DepositCreateRequestModel
{
    [Required] public FundTypes FundType { get; set; }
    public long Amount { get; set; }
    [Required] public CurrencyTypes CurrencyId { get; set; }
    public long PaymentServiceId { get; set; }
    public long TargetTradeAccountUid { get; set; } = 0;
    public string Note { get; set; } = string.Empty;
    public dynamic Request { get; set; } = null!;

    public Supplement.DepositSupplement ToSupplement()
        => Supplement.DepositSupplement.Build(Amount, CurrencyId, PaymentServiceId, TargetTradeAccountUid, Note,
            Request);
}

public class DepositCreateRequestModelValidator : AbstractValidator<DepositCreateRequestModel>
{
    public DepositCreateRequestModelValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(100);
        RuleFor(x => x.PaymentServiceId).GreaterThan(1);
        RuleFor(x => x.FundType).NotNull();
        RuleFor(x => x.FundType).Must(x => x.IsValid());
        RuleFor(x => x.CurrencyId).NotNull();
        RuleFor(x => x.CurrencyId).Must(x => x.IsValid());
    }
}