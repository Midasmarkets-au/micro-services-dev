using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Newtonsoft.Json;

namespace Bacera.Gateway.ViewModels.Tenant;

public class LeadBasicViewModel
{
    public long PartyId { get; set; }
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public LeadSourceTypes SourceType { get; set; }
    public LeadStatusTypes Status { get; set; }
    public ParentUserBasicModel User { get; set; } = new();

    public bool HasAssignedToSales { get; set; }
    // public string Utm { get; set; } = string.Empty;

    [JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public string Supplement { get; set; } = string.Empty;

    public string Utm => Utils.JsonDeserializeObjectWithDefault<Dictionary<string, LeadItem>>(Supplement)
        .GetValueOrDefault("utm")?.Data ?? string.Empty;
}

public class LeadDetailViewModel
{
    public long PartyId { get; set; }
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public LeadStatusTypes Status { get; set; }
    public LeadSourceTypes SourceType { get; set; }

    public bool HasAssignedToSales { get; set; }
    public List<AccountBasicViewModel> AssignedAccounts { get; set; } = new();
    public List<Comment> UpdateLogs { get; set; } = new();
    public bool IsEmpty() => Id == 0;
}

public static class LeadViewModelForRepExtension
{
    public static IQueryable<LeadBasicViewModel> ToResponseModel(this IQueryable<Lead> query)
        => query.Select(x => new LeadBasicViewModel
        {
            Id = x.Id,
            PartyId = x.PartyId ?? 0,
            Email = x.Email,
            Name = x.Name,
            PhoneNumber = x.PhoneNumber,
            SourceType = (LeadSourceTypes)x.SourceType,
            Status = (LeadStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            HasAssignedToSales = x.Accounts.Any(),
            Supplement = x.Supplement,
        });

    public static IQueryable<LeadDetailViewModel> ToRepDetailResponseModel(this IQueryable<Lead> query)
        => query.Select(x => new LeadDetailViewModel
        {
            Id = x.Id,
            PartyId = x.PartyId ?? 0,
            Email = x.Email,
            Name = x.Name,
            PhoneNumber = x.PhoneNumber,
            SourceType = (LeadSourceTypes)x.SourceType,
            Status = (LeadStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            HasAssignedToSales = x.Accounts.Any(),
            AssignedAccounts = x.Accounts.Select(a => new AccountBasicViewModel
            {
                Id = a.Id,
                Uid = a.Uid,
                PartyId = a.PartyId,
                Role = (AccountRoleTypes)a.Role,
                Name = a.Name,
                Code = a.Role == (short)AccountRoleTypes.Sales
                    ? a.Code
                    : a.SalesAccount != null
                        ? a.SalesAccount.Code
                        : string.Empty,
                Group = a.Role == (short)AccountRoleTypes.Agent
                    ? a.Group
                    : a.AgentAccount != null
                        ? a.AgentAccount.Group
                        : string.Empty,
                AccountNumber = a.TradeAccount != null ? a.TradeAccount.AccountNumber : 0,
            }).ToList()
        });
}