namespace Bacera.Gateway.ViewModels.Tenant;

public class AccountBasicViewModel
{
    public long Id { get; set; }
    public long Uid { get; set; }
    public long PartyId { get; set; }
    public bool? HasComment { get; set; }
    public AccountRoleTypes Role { get; set; }
    public string Code { get; set; } = string.Empty;
    public CurrencyTypes CurrencyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string AccountGroupName { get; set; } = string.Empty;
    public string ReferPath { get; set; } = string.Empty;

    public long AccountNumber { get; set; }
    public int ServiceId { get; set; }
    public TenantUserBasicModel User { get; set; } = TenantUserBasicModel.Empty();
    public static AccountBasicViewModel Empty() => new();
    public bool IsEmpty() => Uid == 0;
}

public class WithAgentAndSalesBasicViewModel : AccountBasicViewModel
{
    public AccountBasicViewModel SalesAccount { get; set; } = Empty();
    public AccountBasicViewModel AgentAccount { get; set; } = Empty();
}

public static class AccountBasicViewModelExt
{
    public static IQueryable<AccountBasicViewModel> ToTenantBasicViewModel(this IQueryable<Account> query)
        => query.Select(x => new AccountBasicViewModel
        {
            Id = x.Id,
            Uid = x.Uid,
            PartyId = x.PartyId,
            Role = (AccountRoleTypes)x.Role,
            Name = x.Name,
            ReferPath = x.ReferPath,
            AccountGroupName = x.Group,
            Code = x.Code,
            Group = x.Group,
            AccountNumber = x.AccountNumber,
            ServiceId = x.ServiceId,
        });

    public static AccountBasicViewModel ToTenantBasicViewModel(this Account query)
        => new()
        {
            Id = query.Id,
            Uid = query.Uid,
            PartyId = query.PartyId,
            Role = (AccountRoleTypes)query.Role,
            Name = query.Name,
            ReferPath = query.ReferPath,
            AccountGroupName = query.Group,
            Code = query.Code,
            Group = query.Group,
            AccountNumber = query.AccountNumber,
            ServiceId = query.ServiceId,
        };
}