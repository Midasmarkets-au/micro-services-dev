using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

public partial class AdjustRecord
{
    public class CreateSpec
    {
        [Required] public AdjustTypes Type { get; set; }
        [Required] public long AccountId { get; set; }
        [Required] public decimal Amount { get; set; }
        [Required] public string Comment { get; set; } = string.Empty;
    }

    public class TenantResponseModel
    {
        public long Id { get; set; }
        public long AccountNumber { get; set; }
        public string Name { get; set; } = "";
        public long Amount { get; set; }
        public string Comment { get; set; } = "";
        public long? AdjustBatchId { get; set; }
        public long? AccountId { get; set; }
        public AdjustTypes Type { get; set; }
        public long Ticket { get; set; }
        public string OperatorName { get; set; } = "";
        public AccountRoleTypes Role { get; set; }
        public string Group { get; set; } = "";
        public string Code { get; set; } = "";
        public AdjustRecordStatusTypes Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}

public static class AdjustRecordExtensions
{
    public static IQueryable<AdjustRecord.TenantResponseModel> ToTenantResponseModel(
        this IQueryable<AdjustRecord> query)
        => query.Select(x => new AdjustRecord.TenantResponseModel
        {
            Id = x.Id,
            AccountNumber = x.AccountNumber,
            Amount = x.Amount,
            Name = x.Account == null ? "" : x.Account.Party.Name,
            Role = x.Account == null ? AccountRoleTypes.Unknown : (AccountRoleTypes)x.Account.Role,
            Group = x.Account == null ? "" :
                x.Account.Role == (short)AccountRoleTypes.Agent ? x.Account.Group :
                x.Account.AgentAccount != null ? x.Account.AgentAccount.Code : "",
            Code = x.Account == null ? "" :
                x.Account.Role == (short)AccountRoleTypes.Sales ? x.Account.Code :
                x.Account.SalesAccount != null ? x.Account.SalesAccount.Code : "",
            Comment = x.Comment,
            OperatorName = x.OperatorParty.NativeName,
            AdjustBatchId = x.AdjustBatchId,
            AccountId = x.AccountId,
            Type = (AdjustTypes)x.Type,
            Ticket = x.Ticket,
            Status = (AdjustRecordStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
        });
}