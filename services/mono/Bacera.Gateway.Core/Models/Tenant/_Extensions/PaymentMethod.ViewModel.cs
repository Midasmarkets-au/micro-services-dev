using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;
using M = Bacera.Gateway.PaymentMethod;

namespace Bacera.Gateway;

public partial class PaymentMethod
{
    public sealed class TenantPageModel
    {
        public long Id { get; set; }
        public int Platform { get; set; }
        public int CurrencyId { get; set; }
        public int Percentage { get; set; }
        public long InitialValue { get; set; }
        public long MinValue { get; set; }
        public long MaxValue { get; set; }
        [JsonIgnore] public long OperatorPartyId { get; set; }
        public string Name { get; set; } = null!;
        public string CommentCode { get; set; } = null!;
        public string OperatorName { get; set; } = "";
        public short IsHighDollarEnabled { get; set; }
        public short IsAutoDepositEnabled { get; set; }
        public string Group { get; set; } = null!;
        public string Logo { get; set; } = null!;
        public string Note { get; set; } = null!;
        public PaymentMethodStatusTypes Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int Sort { get; set; }

        [JsonIgnore] public string AvailableCurrenciesRaw { get; set; } = "";

        public HashSet<CurrencyTypes> AvailableCurrencies
            => AvailableCurrenciesSpec.FromJson(AvailableCurrenciesRaw).Currencies;
    }

    public class TenantDetailModel
    {
        public long Id { get; set; }

        public int Platform { get; set; }

        public string MethodType { get; set; } = null!;

        public int CurrencyId { get; set; }

        public int Percentage { get; set; }

        public long InitialValue { get; set; }

        public long MinValue { get; set; }

        public long MaxValue { get; set; }

        public string Name { get; set; } = null!;

        // public string Configuration { get; set; } = null!;
        [JsonIgnore] public string AvailableCurrenciesRaw { get; set; } = null!;

        public HashSet<CurrencyTypes> AvailableCurrencies
            => AvailableCurrenciesSpec.FromJson(AvailableCurrenciesRaw).Currencies;

        public string CommentCode { get; set; } = null!;

        public short IsHighDollarEnabled { get; set; }

        public short IsAutoDepositEnabled { get; set; }

        public string Group { get; set; } = null!;

        public string Logo { get; set; } = null!;

        public string Note { get; set; } = null!;

        public PaymentMethodStatusTypes Status { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public long OperatorPartyId { get; set; }
    }

    public sealed class ClientGroupModel
    {
        public string Group { get; set; } = null!;
        public string Logo { get; set; } = null!;

        public long[] Range { get; set; } = [0, 0];

        public long InitialValue { get; set; } = 0;

        public HashSet<CurrencyTypes> AvailableCurrencies { get; set; } = [];

        public string PaymentMethodName { get; set; } = null!;
    }

    public ClientGroupModel ToClientGroupModel() => new()
    {
        Group = Group,
        Logo = Logo,
        Range = [MinValue, MaxValue],
        InitialValue = InitialValue,
        AvailableCurrencies = GetAvailableCurrencies(),
        PaymentMethodName = Name
    };

    public sealed class ClientNameModel
    {
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; set; }

        public string HashId => HashEncode(Id);
        public string Name { get; set; } = null!;
        public string Logo { get; set; } = null!;
        public long[] Range { get; set; } = [0, 0];
        public long InitialValue { get; set; } = 0;
    }
}

public static class PaymentMethodViewModelExtension
{
    public static IEnumerable<M.TenantPageModel> ToTenantPageModel(this IEnumerable<M> items)
        => items.Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            Platform = x.Platform,
            CurrencyId = x.CurrencyId,
            Percentage = x.Percentage,
            InitialValue = x.InitialValue,
            MinValue = x.MinValue,
            MaxValue = x.MaxValue,
            Name = x.Name,
            CommentCode = x.CommentCode,
            IsHighDollarEnabled = x.IsHighDollarEnabled,
            IsAutoDepositEnabled = x.IsAutoDepositEnabled,
            Group = x.Group,
            Logo = x.Logo,
            Note = x.Note,
            Status = (PaymentMethodStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            Sort = x.Sort,
            AvailableCurrenciesRaw = x.AvailableCurrencies,
            OperatorPartyId = x.OperatorPartyId
        });


    public static M.TenantDetailModel ToTenantDetailModel(this M x)
        => new()
        {
            Id = x.Id,
            Platform = x.Platform,
            CurrencyId = x.CurrencyId,
            Percentage = x.Percentage,
            InitialValue = x.InitialValue,
            MinValue = x.MinValue,
            MaxValue = x.MaxValue,
            Name = x.Name,
            CommentCode = x.CommentCode,
            IsHighDollarEnabled = x.IsHighDollarEnabled,
            IsAutoDepositEnabled = x.IsAutoDepositEnabled,
            Group = x.Group,
            Logo = x.Logo,
            Note = x.Note,
            Status = (PaymentMethodStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            AvailableCurrenciesRaw = x.AvailableCurrencies,
            OperatorPartyId = x.OperatorPartyId
        };
}