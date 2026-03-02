using Bacera.Gateway.Interfaces;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Response;

public class AuditResponseModel : IUserInfoAppendable
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public string Data { get; set; } = string.Empty;
    public int Type { get; set; }
    public long RowId { get; set; }
    public object Values { get; set; } = new();
    public DateTime CreatedOn { get; set; }

    public UserInfo User { get; set; } = new();

    public AuditResponseModel SetUser(UserInfo user)
    {
        User = user;
        return this;
    }

    public AuditResponseModel SetValue(string data)
    {
        Values = JsonConvert.DeserializeObject(data) ?? new object();
        Data = string.Empty;
        return this;
    }
}

public static class AuditResponseModelExtension
{
    public static IQueryable<AuditResponseModel> ToResponseModel(this IQueryable<IEntityAudit> audits)
        => audits.Select(x => new AuditResponseModel
        {
            Id = x.Id,
            Data = x.Data,
            Type = x.Type,
            RowId = x.RowId,
            PartyId = x.PartyId,
            CreatedOn = x.CreatedOn,
        });
}