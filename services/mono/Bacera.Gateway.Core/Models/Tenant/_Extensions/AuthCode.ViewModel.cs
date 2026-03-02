using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

using M = AuthCode;

public partial class AuthCode
{
    public sealed class TenantPageModel
    {
        public long Id { get; set; }
        public long PartyId { get; set; }
        public string Code { get; set; } = "";
        public string Event { get; set; } = "";
        public string MethodValue { get; set; } = "";
        public short Method { get; set; }
        public short Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ExpireOn { get; set; }
        public TenantUserBasicModel User { get; set; } = new();
    }

    public sealed class ClientPageModel
    {
        public string Code { get; set; } = "";
        public string Event { get; set; } = "";
        public string MethodValue { get; set; } = "";
        public short Method { get; set; }
        public short Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ExpireOn { get; set; }
    }
}

public static class AuthCodeViewModelExtension
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> query, bool hideEmail = false)
        => query
            .Include(x => x.Party.PartyComments)
            .Include(x => x.Party.Tags)
            .Select(x => new M.TenantPageModel
            {
                Id = x.Id,
                PartyId = x.PartyId,
                Code = x.Code,
                Event = x.Event,
                MethodValue = x.MethodValue,
                Method = x.Method,
                Status = x.Status,
                CreatedOn = x.CreatedOn,
                ExpireOn = x.ExpireOn,
                User = x.Party.ToTenantBasicViewModel(hideEmail)
            });

    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> query)
        => query.Select(x => new M.ClientPageModel
        {
            Code = x.Code,
            Event = x.Event,
            MethodValue = x.MethodValue,
            Method = x.Method,
            Status = x.Status,
            CreatedOn = x.CreatedOn,
            ExpireOn = x.ExpireOn
        });
}