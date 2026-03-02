using Bacera.Gateway.Interfaces;

namespace Bacera.Gateway.ViewModels.Tenant;

public class AuditAccountBalanceViewModel
{
    public long AccountId { get; set; }
    public long PartyId { get; init; }
    public AuditTypes Type { get; set; }
    public DateTime CreatedOn { get; set; }

    [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public string DataJson { get; set; } = string.Empty;

    public TradeAccount.ChangeBalanceSpec Data =>
        Newtonsoft.Json.JsonConvert.DeserializeObject<TradeAccount.ChangeBalanceSpec>(DataJson)
        ?? new TradeAccount.ChangeBalanceSpec();

    public AccountBasicViewModel Account { get; set; } = new();
    public static AuditAccountBalanceViewModel Empty() => new();

    public TenantUserBasicModel User { get; set; } = TenantUserBasicModel.Empty();
}

public static class AuditAccountBalanceViewModelExt
{
    public static IQueryable<AuditAccountBalanceViewModel> ToTenantResponseModel(this IQueryable<Audit> query)
        => query.Select(x => new AuditAccountBalanceViewModel
        {
            DataJson = x.Data,
            AccountId = x.RowId,
            PartyId = x.PartyId,
            CreatedOn = x.CreatedOn,
            Type = (AuditTypes)x.Type,
        });
}