using FluentValidation;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public abstract class TradeAccountApplicationSupplement
{
    [Serializable]
    public class ChangeLeverageDTO : IApplicationSupplement
    {
        public long AccountUid { get; set; }
        public long AccountNumber { get; set; }
        public int Leverage { get; set; }

        public string ToJson() => JsonConvert.SerializeObject(this);
        public bool IsValidated() => AccountUid > 0 && Leverage > 0;
    }

    public sealed class ChangeLeverageSpec
    {
        public int Leverage { get; set; }
        public bool IsValid() => Leverage > 0;
    }

    public class WholesaleDTO : IApplicationSupplement
    {
        public long AccountUid { get; set; }
        public long AccountNumber { get; set; }
        public dynamic Request { get; set; } = null!;

        public bool IsValidated() => AccountUid > 0;
        public string ToJson() => JsonConvert.SerializeObject(this);
    }

    [Serializable]
    public class ChangePasswordDTO : IApplicationSupplement
    {
        public long AccountUid { get; set; }
        public long AccountNumber { get; set; }

        // public string Password { get; set; } = "";
        public string CallbackUrl { get; set; } = null!;

        public string ToJson() => JsonConvert.SerializeObject(this);
        public bool IsValidated() => AccountUid > 0 && !string.IsNullOrEmpty(CallbackUrl);
    }

    public class ChangePasswordSpec
    {
        public string CallbackUrl { get; set; } = null!;
        public bool IsValid() => !string.IsNullOrEmpty(CallbackUrl);
    }

    public class ChangeLeverageDTOValidator : AbstractValidator<ChangeLeverageDTO>
    {
        public ChangeLeverageDTOValidator()
        {
            RuleFor(x => x.AccountUid).NotEmpty().GreaterThan(0);
            RuleFor(p => p.Leverage).NotEmpty().GreaterThanOrEqualTo(1);
        }
    }

    public class ChangePasswordDTOValidator : AbstractValidator<ChangePasswordDTO>
    {
        public ChangePasswordDTOValidator()
        {
            RuleFor(x => x.AccountUid).NotEmpty().GreaterThan(0);
            RuleFor(x => x.CallbackUrl).NotEmpty().Must(x => x.StartsWith("http://") || x.StartsWith("https://"));
            // RuleFor(p => p.Password).NotEmpty().WithMessage("Your password cannot be empty")
            //     .MinimumLength(6).WithMessage("Your password length must be at least 6.")
            //     .MaximumLength(16).WithMessage("Your password length must not exceed 16.")
            //     .Matches(@"[A-Z]+").WithMessage("Your password must contain at least one uppercase letter.")
            //     .Matches(@"[a-z]+").WithMessage("Your password must contain at least one lowercase letter.")
            //     .Matches(@"[0-9]+").WithMessage("Your password must contain at least one number.")
            //     //.Matches(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]").WithMessage("Your password must contain at least one special character.")
            //     ;
        }
    }
    
    public class WholesaleReferralDTO : IApplicationSupplement
    {
        public long UserUid { get; set; }
        public string Email { get; set; } = null!;
        public long AccountNumber { get; set; }

        public bool IsValidated() => UserUid > 0;
        public string ToJson() => JsonConvert.SerializeObject(this);
    }
    
}