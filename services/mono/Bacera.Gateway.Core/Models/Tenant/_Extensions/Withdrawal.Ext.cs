using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using FluentValidation;
using HashidsNet;

namespace Bacera.Gateway;

partial class Withdrawal : IHasMatter
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.Withdrawal, 8, HashIdSaltTypes.Dictionary[HashIdSaltTypes.Withdrawal]);
    public string HashId => HashEncode(Id);
    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();
    
    public Withdrawal()
    {
        ReferenceNumber = string.Empty;
    }

    public bool IsEmpty() => Id == 0;

    public static Withdrawal Build(long partyId, FundTypes fundType, long amount, CurrencyTypes currencyId)
        => new()
        {
            PartyId = partyId,
            Amount = amount,
            FundType = (int)fundType,
            CurrencyId = (int)currencyId,
            IdNavigation = Matter.Build().Withdrawal()
        };

    public sealed class UpdateSpec
    {
        public long PaymentServiceId { get; set; }
        public long Amount { get; set; }
    }

    public class ClientResponseModel
    {
        public long Id { get; set; }
        public FundTypes FundType { get; set; }
        public long PaymentId { get; set; }
        public long Amount { get; set; }
        public int StateId { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime StatedOn { get; set; }
        public int PaymentStateId { get; set; } = -1;
        public long PaymentServiceId { get; set; } = -1;
        public string PaymentServiceName { get; set; } = string.Empty;
        public Wallet.ResponseModel? SourceWallet { get; set; }

        public bool IsEmpty() => Id == 0;

        public static ClientResponseModel FromEntity(Withdrawal entity)
            => new()
            {
                Id = entity.Id,
                FundType = (FundTypes)entity.FundType,
                PaymentId = entity.PaymentId,
                Amount = entity.Amount,
                CurrencyId = (CurrencyTypes)entity.CurrencyId,
                StateId = entity.IdNavigation.StateId,
                StatedOn = entity.IdNavigation.StatedOn,
                PaymentStateId = entity?.Payment?.Status ?? 0,
                PaymentServiceId = entity?.Payment?.PaymentServiceId ?? 0,
                PaymentServiceName = entity?.Payment?.PaymentMethod.Name ?? string.Empty,
            };
    }

    public sealed class CreateByTenantSpec
    {
        [Required] public long PartyId { get; set; }
        [Required] public long Amount { get; set; }
        [Required] public FundTypes FundType { get; set; }
        [Required] public CurrencyTypes CurrencyId { get; set; }
        [Required] public long PaymentServiceId { get; set; }
        public dynamic Request { get; set; } = null!;
    }

    public sealed class CreateFromAccountByTenantSpec
    {
        [Required] public long PartyId { get; set; }
        [Required] public long Amount { get; set; }
        [Required] public long AccountUid { get; set; }
        [Required] public long PaymentServiceId { get; set; }
        [Required] public object Request { get; set; } = null!;
    }
}

public static class WithdrawalExtensions
{
    public static IQueryable<Withdrawal.ClientResponseModel> ToClientResponseModel(this IQueryable<Withdrawal> query)
        => query.Select(x => new Withdrawal.ClientResponseModel
        {
            Id = x.Id,
            Amount = x.Amount,
            PaymentId = x.PaymentId,
            FundType = (FundTypes)x.FundType,
            StateId = x.IdNavigation.StateId,
            PaymentStateId = x.Payment.Status,
            PaymentServiceId = x.Payment.PaymentServiceId,
            PaymentServiceName = x.Payment.PaymentMethod.Name,
            CurrencyId = (CurrencyTypes)x.CurrencyId,
            CreatedOn = x.IdNavigation.PostedOn,
            UpdatedOn = x.IdNavigation.StatedOn,
            StatedOn = x.IdNavigation.StatedOn,
        });

    public static Withdrawal.ClientResponseModel ToClientResponseModel(this Withdrawal entity)
        => Withdrawal.ClientResponseModel.FromEntity(entity);
}

public class WithdrawalCreateByTenantSpecValidator : AbstractValidator<Withdrawal.CreateByTenantSpec>
{
    public WithdrawalCreateByTenantSpecValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(100);
        RuleFor(x => x.CurrencyId).Must(x => x.IsValid());
        RuleFor(x => x.CurrencyId).NotNull();
        RuleFor(x => x.FundType).Must(x => x.IsValid());
        RuleFor(x => x.FundType).NotNull();
        RuleFor(x => x.PartyId).GreaterThan(1);
        RuleFor(x => x.PaymentServiceId).GreaterThan(1);
        RuleFor(x => x.Request).NotNull();
    }
}

public class
    WithdrawalCreateFormAccountByTenantSpecValidator : AbstractValidator<Withdrawal.CreateFromAccountByTenantSpec>
{
    public WithdrawalCreateFormAccountByTenantSpecValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(100);
        RuleFor(x => x.AccountUid).GreaterThan(1);
        RuleFor(x => x.PartyId).GreaterThan(100);
        RuleFor(x => x.PaymentServiceId).GreaterThan(1);
        RuleFor(x => x.Request).NotNull();
    }
}