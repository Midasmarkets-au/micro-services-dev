using System.Globalization;
using System.Text.RegularExpressions;
using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = Wallet;

partial class Wallet
{
    public sealed class ClientPageModel
    {
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; init; }

        public string HashId => HashEncode(Id);

        public long Balance { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public FundTypes FundType { get; set; }
        public DateTime TalliedOn { get; set; }
        public bool IsPrimary { get; set; }
    }
    
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class WalletTransactionViewModel
    {
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; init; }


        public long Source { get; set; }
        public long Target { get; set; }

        public decimal Amount { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long MatterId { get; set; }

        public MatterTypes MatterType { get; set; }

        public StateTypes StateId { get; set; }
        public DateTime CreatedOn { get; set; }

        public string? PaymentMethod { get; set; }

        public string SourceName =>
            MatterType switch
            {
                MatterTypes.InternalTransfer => Source != 0 ? $"Account No. {Source}" : "Wallet",
                MatterTypes.Rebate => $"Ticket No. {Source}",
                MatterTypes.Withdrawal => Source == 0 ? "Wallet" : "",
                MatterTypes.Deposit => "Deposit Source",
                MatterTypes.WalletAdjust => Amount > 0
                    ? Enum.GetName((WalletAdjustSourceTypes)Source) ?? Source.ToString(CultureInfo.InvariantCulture)
                    : "",
                MatterTypes.System => "System",
                _ => string.Empty
            };

        public string TargetName =>
            MatterType switch
            {
                MatterTypes.InternalTransfer => Target != 0 ? $"Account No. {Target}" : "Wallet",
                MatterTypes.Withdrawal => Source != 0 ? "Withdrawal Target" : "",
                MatterTypes.WalletAdjust => "",
                MatterTypes.System => "System",
                _ => string.Empty
            };

        private static Regex MyRegex() => new("(?<!^)(?=[A-Z])", RegexOptions.Compiled);
    }
}

public static class WalletTransactionViewModelExtension
{
    public static IQueryable<M.WalletTransactionViewModel> ToClientViewModel(this IQueryable<WalletTransaction> query)
        => query.Select(x => new M.WalletTransactionViewModel
        {
            Id = x.Id,
            Amount = x.Amount,
            MatterId = x.MatterId,
            MatterType = (MatterTypes)x.Matter.Type,
            StateId = (StateTypes)x.Matter.StateId,
            CreatedOn = x.CreatedOn
        });

    public static IQueryable<M.WalletTransactionViewModel> ToClientWalletPageModel(this IQueryable<Withdrawal> query)
        => query.Where(x => x.SourceAccountId == null)
            .Select(x => new M.WalletTransactionViewModel
            {
                Amount = x.Amount,
                MatterId = x.Id,
                MatterType = MatterTypes.Withdrawal,
                StateId = (StateTypes)x.IdNavigation.StateId,
                CreatedOn = x.IdNavigation.PostedOn,
                PaymentMethod = x.Payment.PaymentMethod.Name
            });

    public static IQueryable<M.WalletTransactionViewModel> ToClientWalletPageModel(this IQueryable<Transaction> query)
        => query.Select(x => new M.WalletTransactionViewModel
        {
            Amount = x.Amount,
            MatterId = x.Id,
            MatterType = MatterTypes.InternalTransfer,
            StateId = (StateTypes)x.IdNavigation.StateId,
            Source = x.SourceAccountType == (int)TransactionAccountTypes.Account ? x.SourceAccountId : 0,
            Target = x.TargetAccountType == (int)TransactionAccountTypes.Account ? x.TargetAccountId : 0,
            CreatedOn = x.IdNavigation.PostedOn
        });

    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> query)
        => query.Select(x => new M.ClientPageModel
        {
            Id = x.Id,
            Balance = x.Balance,
            CurrencyId = (CurrencyTypes)x.CurrencyId,
            FundType = (FundTypes)x.FundType,
            TalliedOn = x.TalliedOn,
            IsPrimary = x.IsPrimary == 1 ? true : false,
        });
}