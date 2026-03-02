using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = AccountCheck;

public partial class AccountCheck
{
    public sealed class TenantPageModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [JsonIgnore] public string AccountNumberContentRaw { get; set; } = "[]";

        public List<long> AccountNumbers => Utils.JsonDeserializeObjectWithDefault<List<long>>(AccountNumberContentRaw);

        public AccountCheckTypes Type { get; set; }
        public AccountCheckStatusTypes Status { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string OperatorName { get; set; } = string.Empty;
    }

    public sealed class CacheModel
    {
        public long TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public AccountCheckTypes Type { get; set; }
        public AccountCheckStatusTypes Status { get; set; }
    }
}

public static class AccountCheckViewModelExtension
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> q) => q.Select(x => new M.TenantPageModel
    {
        Id = x.Id,
        Name = x.Name,
        AccountNumberContentRaw = x.AccountNumberContent,
        Type = (AccountCheckTypes)x.Type,
        Status = (AccountCheckStatusTypes)x.Status,
        CreatedOn = x.CreatedOn,
        UpdatedOn = x.UpdatedOn,
        OperatorName = x.OperatorParty.NativeName,
    });
}