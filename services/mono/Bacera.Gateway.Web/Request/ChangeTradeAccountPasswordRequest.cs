using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway.Web.Request;

public class ChangeTradeAccountPasswordRequest : ApplicationToken
{
    [Required, MinLength(6)] public string Password { get; set; } = null!;
}