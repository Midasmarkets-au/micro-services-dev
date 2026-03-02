using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Bacera.Gateway.Web.Request;

public class CreateDepositRequestModel
{
    public long Amount { get; set; }
    [Required] public string HashId { get; set; } = null!;
    public long PaymentMethodId => PaymentMethod.HashDecode(HashId);
    public Dictionary<string, string> Request { get; set; } = null!;
    public string Note { get; set; } = string.Empty;


    public (bool, string) Validate()
    {
        if (Amount < 100) return (false, "amount must be greater than 1");

        if (Amount % 100 != 0) return (false, "amount can not have decimal");

        if (string.IsNullOrEmpty(HashId)) return (false, "hashId must not be empty");


        // if (Request.returnUrl == null) return (false, "returnUrl must not be null");
        if (!Request.ContainsKey("returnUrl")) return (false, "returnUrl must not be null");

        return (true, string.Empty);
    }
}
