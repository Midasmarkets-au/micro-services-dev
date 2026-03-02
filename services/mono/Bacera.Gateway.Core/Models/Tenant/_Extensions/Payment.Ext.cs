using System.Globalization;
using Bacera.Gateway.Core.Types;
using HashidsNet;

namespace Bacera.Gateway;

partial class Payment
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.BCRPayment, 8, HashIdSaltTypes.Dictionary[HashIdSaltTypes.BCRPayment]);
    public string HashId => HashEncode(Id);
    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();
    public bool IsEmpty() => Id == default;
    public bool CanExecute() => Status == (short)PaymentStatusTypes.Pending;
    public bool CanCancel() => Status != (short)PaymentStatusTypes.Completed;
    public bool CanComplete() => Status == (short)PaymentStatusTypes.Executing;
    public bool CanFail() => Status == (short)PaymentStatusTypes.Executing;
    public static string GetPaymentNumberTenantIdKey(string pm) => $"Bacera.Gateway.Payment_pm2tid:{pm}";
    public static string GetReferenceNumberTenantIdKey(string rf) => $"Bacera.Gateway.Payment_rf2tid:{rf}";

    public static Payment Build(CreateSpec spec)
        => Build(spec.PartyId, LedgerSideTypes.Debit,
            spec.ServiceId,
            spec.Amount, spec.CurrencyId);

    public static Payment Build(
        long partyId, LedgerSideTypes ledgerSide,
        long serviceId,
        long amount,
        CurrencyTypes currencyId = CurrencyTypes.USD,
        string? number = null,
        string? referenceNumber = default
    ) => new()
    {
        PartyId = partyId,
        Amount = amount,
        CurrencyId = (int)currencyId,
        PaymentServiceId = (int)serviceId,
        LedgerSide = (short)ledgerSide,
        CreatedOn = DateTime.UtcNow,
        UpdatedOn = DateTime.UtcNow,
        Number = number ?? GenerateNumber(),
        ReferenceNumber = referenceNumber ?? string.Empty,
    };

    public static Payment BuildForDeposit(long partyId, long paymentServiceId, long amount, CurrencyTypes currencyId,
        string number, string? referenceNumber = null)
        => new()
        {
            PartyId = partyId,
            PaymentServiceId = paymentServiceId,
            Amount = amount,
            CurrencyId = (int)currencyId,
            LedgerSide = (short)LedgerSideTypes.Debit,
            Number = number,
            Status = (short)PaymentStatusTypes.Executing,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            ReferenceNumber = referenceNumber ?? string.Empty,
        };

    public static string GenerateNumber() => "pm-" + DateTime.UtcNow.ToString("yy") +
                                             ISOWeek.GetWeekOfYear(DateTime.UtcNow.Date) +
                                             Guid.NewGuid().ToString()[..8].ToLower();

    public sealed class CreateSpec
    {
        public long ServiceId { get; set; }
        public long PartyId { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public long Amount { get; set; }
    }

    public CallbackBodyModel GetCallbackBodyModel()
        => CallbackBodyModel.FromJson(CallbackBody);

    public sealed class CallbackBodyModel
    {
        public string Status { get; set; } = string.Empty;
        public object Body { get; set; } = new();

        public DateTime UpdatedOn { get; set; } = DateTime.MinValue;

        public static CallbackBodyModel FromJson(string json)
        {
            var body = Utils.JsonDeserializeObjectWithDefault<CallbackBodyModel>(json);
            return new CallbackBodyModel
            {
                Status = string.IsNullOrEmpty(body.Status) ? "Not Received" : body.Status,
                Body = body.Body
            };
        }

        public string ToJson() => Utils.JsonSerializeObject(this);
    }

    public sealed class ClientResponseModel
    {
        public long Id { get; set; }

        public long? Pid { get; set; }

        public short LedgerSide { get; set; }

        public long PaymentServiceId { get; set; }

        public string Number { get; set; } = null!;

        public int CurrencyId { get; set; }

        public long Amount { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public short Status { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
    }
}

public static class PaymentExtensions
{
    public static IQueryable<Payment> ScopeCanExecute(this IQueryable<Payment> me)
        => me.Where(x => x.Status == (short)PaymentStatusTypes.Pending);

    public static IQueryable<Payment> ScopeCanComplete(this IQueryable<Payment> me)
        => me.Where(x => x.Status == (short)PaymentStatusTypes.Executing)
            .Where(x => x.Amount > 0);

    public static IQueryable<Payment> ScopeCanCancel(this IQueryable<Payment> me)
        => me.Where(x => x.Status == (short)PaymentStatusTypes.Failed || x.Status == (short)PaymentStatusTypes.Pending)
            .Where(x => x.Amount > 0);

    public static IQueryable<Payment> ScopeCanFail(this IQueryable<Payment> me)
        => me.Where(x => x.Status == (short)PaymentStatusTypes.Executing)
            .Where(x => x.Amount > 0);

    public static IQueryable<Payment.ClientResponseModel> ToClientResponseModel(this IQueryable<Payment> me)
        => me.Select(x => new Payment.ClientResponseModel
        {
            Id = x.Id,
            Pid = x.Pid,
            LedgerSide = x.LedgerSide,
            PaymentServiceId = x.PaymentServiceId,
            Number = x.Number,
            CurrencyId = x.CurrencyId,
            Amount = x.Amount,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            Status = x.Status,
            ReferenceNumber = x.ReferenceNumber,
        });
}