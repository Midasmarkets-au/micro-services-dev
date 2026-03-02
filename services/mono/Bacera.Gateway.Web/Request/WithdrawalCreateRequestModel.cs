using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Bacera.Gateway.Web.Request;

public class WithdrawalCreateRequestModel
{
    public long Amount { get; set; }
    [Required] public FundTypes FundType { get; set; }
    [Required] public CurrencyTypes CurrencyId { get; set; }
    [Required] public long PaymentServiceId { get; set; }
    public dynamic Request { get; set; } = null!;
}

public class WithdrawalCreateRequestModelValidator : AbstractValidator<WithdrawalCreateRequestModel>
{
    public WithdrawalCreateRequestModelValidator()
    {
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Amount).GreaterThan(100);
        RuleFor(x => x.FundType).NotNull();
        RuleFor(x => x.FundType).Must(x => x.IsValid());
        RuleFor(x => x.CurrencyId).NotNull();
        RuleFor(x => x.CurrencyId).Must(x => x.IsValid());
        RuleFor(x => x.PaymentServiceId).GreaterThan(1);
    }
}