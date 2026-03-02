using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class RebateDirectRule
{
    public bool IsEmpty() => Id == 0;

    public class CreateSpec
    {
        [Required] public long SourceAccountUid { get; set; }
        [Required] public long TargetAccountUid { get; set; }
        [Required] public long RebateRuleId { get; set; }
    }

    public class UpdateSpec
    {
        [Required] public long RebateRuleId { get; set; }
    }

    public class ClientResponseModel
    {
        public long Id { get; set; }
        public long CreatedByPartyId { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public long ConfirmedByPartyId { get; set; }
        public string ConfirmedByName { get; set; } = string.Empty;
        public long SourceAccountUid { get; set; }
        public long TargetAccountUid { get; set; }
        public Account.SummaryResponseModel SourceAccount { get; set; } = null!;
        public Account.SummaryResponseModel TargetAccount { get; set; } = null!;

        public long RebateRuleId { get; set; }
        public string RebateRuleName { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
        public bool IsEmpty() => Id == default;
    }
}

public static class RebateRuleDirectExtension
{
    public static IQueryable<RebateDirectRule.ClientResponseModel> ToClientResponseModel(
        this IQueryable<RebateDirectRule> query)
        => query.Select(x => new RebateDirectRule.ClientResponseModel
        {
            Id = x.Id,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            CreatedByPartyId = x.CreatedByNavigation.Id,
            CreatedByName = x.CreatedByNavigation.Name,
            ConfirmedByPartyId = x.ConfirmedByNavigation != null ? x.ConfirmedByNavigation.Id : 0,
            ConfirmedByName = x.ConfirmedByNavigation != null ? x.ConfirmedByNavigation.Name : string.Empty,
            SourceAccountUid = x.SourceTradeAccount.IdNavigation.Uid,
            TargetAccountUid = x.TargetAccount.Uid,
            SourceAccount = new Account.SummaryResponseModel
            {
                Id = x.SourceTradeAccount.Id,
                Uid = x.SourceTradeAccount.IdNavigation.Uid,
                Name = x.SourceTradeAccount.IdNavigation.Name,
                AccountNumber = x.SourceTradeAccount.AccountNumber,
                Type = (AccountTypes)x.SourceTradeAccount.IdNavigation.Type,
                Role = (AccountRoleTypes)x.SourceTradeAccount.IdNavigation.Role,
                FundType = (FundTypes)x.SourceTradeAccount.IdNavigation.FundType,
                HasTradeAccount = true,
            },
            TargetAccount = new Account.SummaryResponseModel
            {
                Id = x.TargetAccount.Id,
                Uid = x.TargetAccount.Uid,
                Name = x.TargetAccount.Name,
                Type = (AccountTypes)x.TargetAccount.Type,
                Role = (AccountRoleTypes)x.TargetAccount.Role,
                FundType = (FundTypes)x.TargetAccount.FundType,
                HasTradeAccount = x.TargetAccount.HasTradeAccount,
            },
            RebateRuleId = x.RebateDirectSchemaId,
            RebateRuleName = x.RebateDirectSchema.Name,
        });
}