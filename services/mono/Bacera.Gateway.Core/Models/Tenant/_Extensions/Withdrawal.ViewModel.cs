namespace Bacera.Gateway;

using M = Withdrawal;

partial class Withdrawal
{
    public sealed class ClientPageModel
    {
        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; init; }

        public string HashId => HashEncode(Id);

        public long Amount { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public PaymentStatusTypes PaymentStatus { get; set; }
        public StateTypes StateId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}

public static class WithdrawalViewModelExtensions
{
    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> query)
        => query.Select(x => new M.ClientPageModel
        {
            Id = x.Id,
            Amount = x.Amount,
            CurrencyId = (CurrencyTypes)x.CurrencyId,
            PaymentMethodName = x.Payment.PaymentMethod.Name,
            PaymentStatus = (PaymentStatusTypes)x.Payment.Status,
            StateId = (StateTypes)x.IdNavigation.StateId,
            CreatedOn = x.IdNavigation.PostedOn,
            UpdatedOn = x.IdNavigation.StatedOn,
        });
}