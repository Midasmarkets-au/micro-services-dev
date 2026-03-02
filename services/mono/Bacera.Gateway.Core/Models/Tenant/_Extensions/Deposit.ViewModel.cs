using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = Deposit;

partial class Deposit
{
    public sealed class ClientPageModel
    {
        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; init; }

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]

        public long PaymentMethodId { get; init; }

        public string HashId => HashEncode(Id);

        public string PaymentMethodHashId => PaymentMethod.HashEncode(PaymentMethodId);

        public long Amount { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public string PaymentMethodGroupName { get; set; } = string.Empty;
        public string PaymentMethodName { get; set; } = string.Empty;
        public PaymentStatusTypes PaymentStatus { get; set; }
        public StateTypes StateId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public sealed class StateChangeModel
    {
        [JsonIgnore] public int StateRaw { get; set; }
        public string State => Enum.GetName(typeof(StateTypes), StateRaw) ?? string.Empty;
        public string Info { get; set; } = string.Empty;
        public DateTime PerformedOn { get; set; }
        public string Operator { get; set; } = string.Empty;
    }

    public sealed class StateDetailModel
    {
        public List<StateChangeModel> StateChanges { get; set; } = [];

        [JsonIgnore] public string CallbackBodyRaw { get; set; } = "{}";
        public Payment.CallbackBodyModel CallbackBody => Payment.CallbackBodyModel.FromJson(CallbackBodyRaw);
    }
}

public static class DepositViewModelExtensions
{
    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> query)
        => query.Select(x => new M.ClientPageModel
        {
            Id = x.Id,
            Amount = x.Amount,
            CurrencyId = (CurrencyTypes)x.CurrencyId,
            PaymentMethodId = x.Payment.PaymentServiceId,
            PaymentMethodGroupName = x.Payment.PaymentMethod.Group,
            PaymentMethodName = x.Payment.PaymentMethod.Name,
            PaymentStatus = (PaymentStatusTypes)x.Payment.Status,
            StateId = (StateTypes)x.IdNavigation.StateId,
            CreatedOn = x.IdNavigation.PostedOn,
            UpdatedOn = x.IdNavigation.StatedOn,
        });

    public static IQueryable<M.StateDetailModel> ToStateDetailModel(this IQueryable<M> query)
        => query.Select(x => new M.StateDetailModel
        {
            StateChanges = x.IdNavigation.Activities
                .OrderBy(a => a.PerformedOn)
                .Select(a => new M.StateChangeModel
                {
                    StateRaw = a.ToStateId,
                    Info = a.Data,
                    PerformedOn = a.PerformedOn,
                    Operator = a.Party.Email,
                })
                .ToList(),
            CallbackBodyRaw = x.Payment.CallbackBody,
        });
}