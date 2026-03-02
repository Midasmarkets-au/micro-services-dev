using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = Refund;

partial class Refund
{
    public sealed class ClientPageModel
    {
        public long Amount { get; set; }
        public string Comment { get; set; } = string.Empty;
        public StateTypes StateId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int CurrencyId { get; set; }
}
}

public static class RefundViewModelExtension
{
    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> q) => q.Select(x => new M.ClientPageModel
    {
        Amount = x.Amount,
        Comment = x.Comment,
        CreatedOn = x.CreatedOn,
        StateId = (StateTypes)x.IdNavigation.StateId,
        UpdatedOn = x.IdNavigation.StatedOn,
        CurrencyId = x.CurrencyId
    });
}