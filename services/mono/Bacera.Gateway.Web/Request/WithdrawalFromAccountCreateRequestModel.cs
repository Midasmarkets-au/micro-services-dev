using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Bacera.Gateway.Web.Request;

public class WithdrawalFromAccountCreateRequestModel
{
    [Required] public long Amount { get; set; }
    [Required] public long AccountUid { get; set; }
    [Required] public long PaymentServiceId { get; set; }
    [Required] public dynamic Request { get; set; } = null!;
}

public class WithdrawalFromAccountCreateRequestModelValidator : AbstractValidator<WithdrawalFromAccountCreateRequestModel>
{
    public WithdrawalFromAccountCreateRequestModelValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(100);
        RuleFor(x => x.PaymentServiceId).GreaterThan(1);
        RuleFor(x => x.AccountUid).GreaterThan(1);
    }
}