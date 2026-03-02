using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Bacera.Gateway.Web.Request;

public class CreateWithdrawalRequestModel
{
    [Required] public long Amount { get; set; }
    [Required] public string HashId { get; set; } = null!;
    public long PaymentMethodId => PaymentMethod.HashDecode(HashId);
    [Required] public dynamic Request { get; set; } = null!;
    public string? Note { get; set; }
    /// <summary>
    /// Email verification code (required for withdrawal)
    /// </summary>
    public string? VerificationCode { get; set; }

    public (bool, string) Validate()
    {
        if (Amount < 100) return (false, "amount must be greater than 1");

        if (string.IsNullOrEmpty(HashId)) return (false, "hashId must not be empty");

        if (Request == null) return (false, "request must not be null");

        return (true, string.Empty);
    }
}