using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = AccountCheck;

public partial class AccountCheck
{
    public sealed class CreateAndUpdateSpec
    {
        [Required] public string Name { get; set; } = string.Empty;

        [Required] public List<long> AccountNumbers { get; set; } = [];

        public AccountCheckTypes Type { get; set; }
        public AccountCheckStatusTypes Status { get; set; }

        public M ToEntity() => new()
        {
            Name = Name,
            AccountNumberContent = Utils.JsonSerializeObject(AccountNumbers),
            Type = (short)Type,
            Status = (short)Status,
        };

        public void ApplyToEntity(M entity)
        {
            entity.Name = Name;
            entity.AccountNumberContent = Utils.JsonSerializeObject(AccountNumbers);
            entity.Type = (short)Type;
            entity.Status = (short)Status;
        }
    }

    public sealed class SendEmailSpec
    {
        [Required] public long AccountNumber { get; set; }
        [Required] public string Email { get; set; } = null!;
        [Required] public string Language { get; set; } = string.Empty;

        public List<string> BccEmails { get; set; } = [];
        public DateOnly Date { get; set; } = default;
    }
}