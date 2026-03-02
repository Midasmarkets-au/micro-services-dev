using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Bacera.Gateway.Web.Request;

public class CreateTransactionRequestModel
{
    [Required] public long Amount { get; set; }
    [Required] public string WalletHashId { get; set; } = null!;
    [Required] public long TradeAccountUid { get; set; }
}
