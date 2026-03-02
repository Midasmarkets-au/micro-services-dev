using System.Globalization;
using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = Transaction;

partial class Transaction
{
    public sealed class ClientPageModel
    {
        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; init; }

        public string HashId => HashEncode(Id);

        public long Amount { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public TransactionAccountTypes SourceType { get; set; }

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public long SourceId { get; set; }

        public long SourceAccountNumber { get; set; }

        public TransactionAccountTypes TargetType { get; set; }

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public long TargetId { get; set; }
        public long TargetAccountNumber { get; set; }
        public StateTypes StateId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string FlowType { get; set; } = "out"; // "in" or "out"
    }
}

public static class TransactionViewModelExtensions
{
    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> query)
        => query.Select(x => new M.ClientPageModel
        {
            Id = x.Id,
            Amount = x.Amount,
            CurrencyId = (CurrencyTypes)x.CurrencyId,
            StateId = (StateTypes)x.IdNavigation.StateId,
            SourceType = (TransactionAccountTypes)x.SourceAccountType,
            SourceId = x.SourceAccountId,
            TargetType = (TransactionAccountTypes)x.TargetAccountType,
            TargetId = x.TargetAccountId,
            CreatedOn = x.IdNavigation.PostedOn,
            UpdatedOn = x.IdNavigation.StatedOn,
        });
}