using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.ViewModels.Tenant;

public class RefundViewModel
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public long Amount { get; set; }
    public CurrencyTypes CurrencyId { get; set; }
    public long TargetId { get; set; }
    public RefundTargetTypes TargetType { get; set; }
    public DateTime CreatedOn { get; set; }
    public string Comment { get; set; } = string.Empty;
    public TenantUserBasicModel User { get; set; } = new();
}

public static class RefundViewModelExt
{
    public static IQueryable<RefundViewModel> ToResponseModel(this IQueryable<Refund> query, bool hideEmail = false)
        => query
            .Include(x => x.Party.PartyComments)
            .Include(x => x.Party.Tags)
            .Select(x => new RefundViewModel
            {
                Id = x.Id,
                PartyId = x.PartyId,
                Amount = x.Amount,
                CurrencyId = (CurrencyTypes)x.CurrencyId,
                TargetId = x.TargetId,
                TargetType = (RefundTargetTypes)x.TargetType,
                CreatedOn = x.CreatedOn,
                Comment = x.Comment,
                User = x.Party.ToTenantBasicViewModel(hideEmail)
            });
}