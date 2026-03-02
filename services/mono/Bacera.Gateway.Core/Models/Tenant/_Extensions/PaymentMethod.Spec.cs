using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = PaymentMethod;

public partial class PaymentMethod
{
    public sealed class UpdateAccessGroupSpec
    {
        [Required] public string Group { get; set; } = null!;
    }

    public sealed class UpdateSpec
    {
        public int Percentage { get; set; }

        public long InitialValue { get; set; }

        public long MinValue { get; set; }

        public long MaxValue { get; set; }

        public string Name { get; set; } = string.Empty;

        public string CommentCode { get; set; } = string.Empty;

        public short IsHighDollarEnabled { get; set; }

        public short IsAutoDepositEnabled { get; set; }

        public string Group { get; set; } = string.Empty;

        public string Logo { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;

        public short Status { get; set; }
        public HashSet<CurrencyTypes> AvailableCurrencies { get; set; } = [];

        public void ApplyToEntity(ref M entity, long operatorPartyId = 1)
        {
            entity.Percentage = Percentage;
            entity.InitialValue = InitialValue;
            entity.MinValue = MinValue;
            entity.MaxValue = MaxValue;
            entity.Name = Name;
            entity.CommentCode = CommentCode;
            entity.IsHighDollarEnabled = IsHighDollarEnabled;
            entity.IsAutoDepositEnabled = IsAutoDepositEnabled;
            entity.Group = Group;
            entity.Logo = Logo;
            entity.Note = Note;
            entity.Status = Status;
            entity.OperatorPartyId = operatorPartyId;

            if (AvailableCurrencies.Count == 0) return;

            var currency = AvailableCurrenciesSpec.FromJson(entity.AvailableCurrencies);
            currency.Currencies = AvailableCurrencies;
            entity.AvailableCurrencies = currency.ToJson();
        }
    }

    public sealed class CreateSpec
    {
        public PaymentPlatformTypes Platform { get; set; }
        public string MethodType { get; set; } = string.Empty;
        public CurrencyTypes CurrencyId { get; set; }
        public int Percentage { get; set; }

        public long InitialValue { get; set; }

        public long MinValue { get; set; }

        public long MaxValue { get; set; }

        public string Name { get; set; } = string.Empty;

        public string CommentCode { get; set; } = string.Empty;

        public short IsHighDollarEnabled { get; set; }

        public short IsAutoDepositEnabled { get; set; }

        public string Group { get; set; } = string.Empty;

        public string Logo { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;

        public PaymentMethodStatusTypes Status { get; set; }

        public M ToEntity() => new()
        {
            Platform = (int)Platform,
            MethodType = MethodType,
            CurrencyId = (int)CurrencyId,
            Percentage = Percentage,
            InitialValue = InitialValue,
            MinValue = MinValue,
            MaxValue = MaxValue,
            Name = Name,
            Configuration = "{}",
            CommentCode = CommentCode,
            IsHighDollarEnabled = IsHighDollarEnabled,
            IsAutoDepositEnabled = IsAutoDepositEnabled,
            Group = Group,
            Logo = Logo,
            Note = Note,
            Status = (short)Status,
        };
    }

    public sealed class GroupEnableByExisting
    {
        public List<long> OpenedMethodIds { get; set; } = [];
        public List<long> NewOpenMethodIds { get; set; } = [];
    }

    public sealed class UpdateSortSpec
    {
        [Required]
        public long Id { get; set; }
        
        [Required]
        public int Sort { get; set; }
    }

    public sealed class BatchUpdateSortSpec
    {
        [Required]
        public List<UpdateSortSpec> Items { get; set; } = [];
    }

    public sealed class AvailableCurrenciesSpec
    {
        public bool ApplyAccountCurrency { get; set; }
        public HashSet<CurrencyTypes> Currencies { get; set; } = [];

        public string ToJson() => JsonConvert.SerializeObject(this);

        public static AvailableCurrenciesSpec FromJson(string json) =>
            JsonConvert.DeserializeObject<AvailableCurrenciesSpec>(json)!;
    }
}