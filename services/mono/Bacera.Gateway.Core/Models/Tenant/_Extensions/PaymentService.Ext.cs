using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = PaymentService;

partial class PaymentService
{
    public bool IsEmpty() => Id == default;

    public class UpdateSpec
    {
        public CurrencyTypes CurrencyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [MaxLength(6)] public string CommentCode { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public bool IsActivated { get; set; }
        public bool CanDeposit { get; set; }
        public bool CanWithdraw { get; set; }
        public long InitialValue { get; set; }
        public long MinValue { get; set; }
        public long MaxValue { get; set; }
        public bool IsHighDollarEnabled { get; set; }
        public bool IsAutoDepositEnabled { get; set; }
    }

    public sealed class Accesses
    {
        public CurrencyTypes CurrencyId { get; set; }
        public FundTypes FundType { get; set; }
        public long PaymentServiceId { get; set; }
        public bool CanDeposit { get; set; }
        public bool CanWithdraw { get; set; }
    }

    public sealed class BatchSwitchSpec
    {
        [Required] public CurrencyTypes CurrencyId { get; set; }
        [Required] public FundTypes FundType { get; set; }

        [Required] public bool IncludeInactivated { get; set; }
        // [Required(ErrorMessage = "The switch state is required.")]
        // [RegularExpression("(On|Off)", ErrorMessage = "The switch state must be either 'On' or 'Off'.")]
        // public string Switch { get; set; } = null!;
    }

    public class ResponseModel
    {
        public CurrencyTypes CurrencyId { get; set; }
        public long Id { get; set; }
        public int Platform { get; set; }
        public int Sequence { get; set; }
        public List<FundType.ResponseModel> FundTypes { get; set; } = new();
        public short IsActivated { get; set; }
        public short CanDeposit { get; set; }
        public short CanWithdraw { get; set; }
        public string Name { get; set; } = null!;
        public string CategoryName { get; set; } = string.Empty;
        public long InitialValue { get; set; }
        public long MinValue { get; set; }
        public long MaxValue { get; set; }
        public string Description { get; set; } = null!;
    }

    public class AccessResponseModel
    {
        public List<ResponseModel> Deposit { get; set; } = new();
        public List<ResponseModel> Withdrawal { get; set; } = new();
    }

    public class CallbackSetting
    {
        public int CallbackExpiredTimeInMinutes { get; set; }

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}

public static class PaymentServiceExtensions
{
    public static IQueryable<M.ResponseModel> ToResponse(this IQueryable<M> queryable) =>
        queryable.Select(x => new M.ResponseModel
        {
            Id = x.Id,
            Name = x.Name,
            CurrencyId = (CurrencyTypes)x.CurrencyId,
            CategoryName = x.CategoryName,
            Platform = x.Platform,
            Sequence = x.Sequence,
            IsActivated = x.IsActivated,
            CanDeposit = x.CanDeposit,
            CanWithdraw = x.CanWithdraw,
            Description = x.Description,
            InitialValue = x.InitialValue,
            MinValue = x.MinValue,
            MaxValue = x.MaxValue,
            FundTypes = x.FundTypes.Select(t => new FundType.ResponseModel { Id = t.Id, Name = t.Name }).ToList(),
        });

    // Remove sensitive Configuration
    public static IQueryable<M> SelectBaseInfo(this IQueryable<M> queryable) =>
        queryable.Select(x => new M
        {
            Id = x.Id,
            Name = x.Name,
            CurrencyId = x.CurrencyId,
            CategoryName = x.CategoryName,
            Platform = x.Platform,
            Sequence = x.Sequence,
            Description = x.Description,
            IsActivated = x.IsActivated,
            CanDeposit = x.CanDeposit,
            CanWithdraw = x.CanWithdraw,
            InitialValue = x.InitialValue,
            MinValue = x.MinValue,
            MaxValue = x.MaxValue,
            IsHighDollarEnabled = x.IsHighDollarEnabled,
            IsAutoDepositEnabled = x.IsAutoDepositEnabled,
            CommentCode = x.CommentCode,
            FundTypes = x.FundTypes.Select(t => new FundType { Id = t.Id, Name = t.Name }).ToList(),
        });
}