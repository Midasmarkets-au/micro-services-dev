using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class Audit : IEntity, IEntityAudit
{
    public class DataDTO
    {
        public AccountBalance OriginalValues { get; set; } = new();
        public AccountBalance CurrentValues { get; set; } = new();
    }

    public class AccountBalance
    {
        public decimal Balance { get; set; }
        public long BalanceInCents => (long)Math.Round(Balance * 100);
        public string Ticket { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }
}