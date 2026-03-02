using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using FluentValidation;
using HashidsNet;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class Deposit : IHasMatter
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.Deposit, 8, HashIdSaltTypes.Dictionary[HashIdSaltTypes.Deposit]);
    public string HashId => HashEncode(Id);
    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();
    
    public EventShopPointTransaction.MQSource ToMQSource(long tenantId)
        => new()
        {
            SourceType = EventShopPointTransactionSourceTypes.Deposit,
            RowId = Id,
            TenantId = tenantId
        };

    public Deposit()
    {
        ReferenceNumber = string.Empty;
    }

    public static Deposit Build(long partyId, FundTypes fundType, CurrencyTypes currency, long amount)
        => new()
        {
            PartyId = partyId,
            FundType = (int)fundType,
            Amount = amount,
            CurrencyId = (int)currency,
            Type = 0,
            IdNavigation = Matter.Build().Deposit()
        };

    public sealed class UpdateSpec
    {
        public long PaymentServiceId { get; set; }
        public long Amount { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
    }

    public sealed class CreateByTenantSpec
    {
        public long PartyId { get; set; }
        public long Amount { get; set; }
        [Required] public FundTypes FundType { get; set; }
        [Required] public CurrencyTypes CurrencyId { get; set; }
        public long PaymentServiceId { get; set; }
        public long TargetTradeAccountUid { get; set; } = 0;
        public string Note { get; set; } = string.Empty;

        public string ReferenceNumber { get; set; } = string.Empty;
        public dynamic Request { get; set; } = null!;

        public Supplement.DepositSupplement ToSupplement()
            => Supplement.DepositSupplement.Build(Amount, CurrencyId, PaymentServiceId, TargetTradeAccountUid, Note, Request);
    }

    public sealed class ClientResponseModel
    {
        public long Id { get; set; }
        [JsonIgnore] public long PaymentId { get; set; }

        public long PaymentServiceId { get; set; }

        // public int PaymentServicePlatform { get; set; }
        public int CurrencyId { get; set; }
        public long Amount { get; set; }
        public int StateId { get; set; }
        public int PaymentStateId { get; set; }
        public string PaymentServiceName { get; set; } = string.Empty;
        public string PaymentHashId => Payment.HashEncode(PaymentId);
        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public object? Supplement { get; set; }

        public Account.SummaryResponseModel? TargetAccount { get; set; }

        public static ClientResponseModel From(Deposit deposit)
            => new()
            {
                Id = deposit.Id,
                Amount = deposit.Amount,
                CurrencyId = deposit.CurrencyId,
                PaymentId = deposit.PaymentId,
                StateId = deposit.IdNavigation.StateId,
                PaymentServiceId = deposit.Payment.PaymentServiceId,
                // PaymentServiceName = deposit.Payment.PaymentMethod.Name,
                UpdatedOn = deposit.IdNavigation?.StatedOn ?? DateTime.MinValue,
                CreatedOn = deposit.IdNavigation?.PostedOn ?? DateTime.MinValue,
            };

        public bool IsEmpty() => Id == 0;
    }

    public bool IsEmpty() => Id == 0;
}

public static class DepositExtensions
{
    public static Deposit.ClientResponseModel ToClientResponseModel(this Deposit deposit)
        => Deposit.ClientResponseModel.From(deposit);

    public static IQueryable<Deposit.ClientResponseModel> ToClientResponseModels(this IQueryable<Deposit> deposits)
        => deposits.Select(x => new Deposit.ClientResponseModel
        {
            Id = x.Id,
            Amount = x.Amount,
            CurrencyId = x.CurrencyId,
            StateId = x.IdNavigation.StateId,
            CreatedOn = x.IdNavigation.PostedOn,
            UpdatedOn = x.IdNavigation.StatedOn,
            PaymentId = x.PaymentId,
            PaymentStateId = x.Payment.Status,
            PaymentServiceId = x.Payment.PaymentServiceId,
            PaymentServiceName = x.Payment.PaymentMethod.Group, // client only see the group name
            TargetAccount = x.TargetAccountId > 0
                ? new Account.SummaryResponseModel
                {
                    Uid = x.TargetAccount!.Uid,
                    Name = x.TargetAccount.Name,
                    Type = (AccountTypes)x.TargetAccount.Type,
                    Role = (AccountRoleTypes)x.TargetAccount.Role,
                    Status = (AccountStatusTypes)x.TargetAccount.Status,
                    HasTradeAccount = x.TargetAccount.HasTradeAccount
                }
                : new Account.SummaryResponseModel()
        });
}

public class DepositCreateByTenantSpecValidator : AbstractValidator<Deposit.CreateByTenantSpec>
{
    public DepositCreateByTenantSpecValidator()
    {
        RuleFor(x => x.PartyId).GreaterThan(1);
        RuleFor(x => x.Amount).GreaterThan(100);
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.PaymentServiceId).GreaterThan(1);
        RuleFor(x => x.FundType).NotNull();
        RuleFor(x => x.FundType).Must(x => x.IsValid());
        RuleFor(x => x.CurrencyId).NotNull();
        RuleFor(x => x.CurrencyId).Must(x => x.IsValid());
    }
}