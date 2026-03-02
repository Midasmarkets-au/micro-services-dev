using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class Matter
{
    public class Builder
    {
        public long? Pid { get; init; }
        public MatterTypes Type { get; set; }
        public StateTypes StateId { get; set; }
    }

    public static Builder Build(long? parentMatterId = null) => new()
    {
        Pid = parentMatterId,
        Type = MatterTypes.System,
        StateId = StateTypes.Initialed,
    };

    public Matter SetState(StateTypes state)
    {
        StateId = (int)state;
        return this;
    }

    public class TransferResponseModel
    {
        public long Id { get; set; }
        public long Amount { get; set; }
        public long PartyId { get; set; }
        public MatterTypes Type { get; set; }
        public LedgerSideTypes LedgerSide { get; set; }
        public StateTypes StateId { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public DateTime PostedOn { get; set; }
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

public static class MatterExt
{
    public static Matter Deposit(this Matter.Builder builder) => new()
    {
        Pid = builder.Pid,
        PostedOn = DateTime.UtcNow,
        StatedOn = DateTime.UtcNow,
        Type = (int)MatterTypes.Deposit,
        StateId = (int)StateTypes.DepositCreated,
    };

    public static Matter Withdrawal(this Matter.Builder builder) => new()
    {
        Pid = builder.Pid,
        PostedOn = DateTime.UtcNow,
        StatedOn = DateTime.UtcNow,
        Type = (int)MatterTypes.Withdrawal,
        StateId = (int)StateTypes.WithdrawalCreated,
    };

    public static Matter Transaction(this Matter.Builder builder) => new()
    {
        Pid = builder.Pid,
        PostedOn = DateTime.UtcNow,
        StatedOn = DateTime.UtcNow,
        Type = (int)MatterTypes.InternalTransfer,
        StateId = (int)StateTypes.TransferAwaitingApproval,
    };

    public static Matter Rebate(this Matter.Builder builder) => new()
    {
        Pid = builder.Pid,
        PostedOn = DateTime.UtcNow,
        StatedOn = DateTime.UtcNow,
        Type = (int)MatterTypes.Rebate,
        StateId = (int)StateTypes.RebateCreated,
    };

    public static Matter Refund(this Matter.Builder builder) => new()
    {
        Pid = builder.Pid,
        PostedOn = DateTime.UtcNow,
        StatedOn = DateTime.UtcNow,
        Type = (int)MatterTypes.Refund,
        StateId = (int)StateTypes.RefundCreated,
    };

    public static Matter WalletAdjustCompleted(this Matter.Builder builder) => new()
    {
        Pid = builder.Pid,
        PostedOn = DateTime.UtcNow,
        StatedOn = DateTime.UtcNow,
        Type = (int)MatterTypes.WalletAdjust,
        StateId = (int)StateTypes.WalletAdjustCompleted,
    };

    // Matter should include the following properties:
    // Deposit, Withdrawal, Rebate, Transaction
    public static List<Matter.TransferResponseModel> ToTransferResponseModel(this IEnumerable<Matter> query)
        => query
            .Select(x => new Matter.TransferResponseModel
            {
                Id = x.Id,
                Amount = x.Deposit?.Amount ?? x.Withdrawal?.Amount ?? x.Rebate?.Amount ?? x.Transaction?.Amount ?? 0,
                PartyId = x.Deposit?.PartyId ?? x.Withdrawal?.PartyId ?? x.Rebate?.PartyId ?? 0,
                Type = (MatterTypes)x.Type,
                StateId = (StateTypes)x.StateId,
            }).ToList();

    public static IQueryable<Matter.StateDetailModel> ToStateDetailModel(this IQueryable<Matter> query)
        => query.Select(x => new Matter.StateDetailModel
        {
            StateChanges = x.Activities
                .OrderBy(a => a.PerformedOn)
                .Select(a => new Matter.StateChangeModel
                {
                    StateRaw = a.ToStateId,
                    Info = a.Data,
                    PerformedOn = a.PerformedOn,
                    Operator = a.Party.Email,
                })
                .ToList(),
            CallbackBodyRaw = x.Deposit != null ? x.Deposit.Payment.CallbackBody : "{}",
        });
}