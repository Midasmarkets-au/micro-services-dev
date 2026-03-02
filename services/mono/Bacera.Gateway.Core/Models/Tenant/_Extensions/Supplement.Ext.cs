using System.Reflection;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class Supplement
{
    public bool IsEmpty() => Id == 0;

    public Supplement()
    {
        Data = "{}";
    }

    public static Supplement Build(SupplementTypes type, long rowId, string data = "{}")
        => new()
        {
            Type = (short)type,
            RowId = rowId,
            Data = data,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow
        };

    /// <summary>
    /// Set data as json string
    /// </summary>
    /// <param name="obj"></param>
    public void SetDataObject(object obj)
    {
        Data = JsonConvert.SerializeObject(obj);
        UpdatedOn = DateTime.UtcNow;
    }

    public sealed class DepositSupplement
    {
        public long Amount { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public long PaymentServiceId { get; set; }
        public long TargetTradeAccountUid { get; set; }
        public string Note { get; set; } = string.Empty;
        public dynamic Request { get; set; } = new object();
        public string RequestJson => JsonConvert.SerializeObject(Request);

        public static DepositSupplement Build(long amount, CurrencyTypes currencyId, long paymentServiceId = 0,
            long targetAccountUid = 0, string note = "", dynamic? request = null)
            => new()
            {
                Note = note,
                Amount = amount,
                CurrencyId = currencyId,
                Request = request ?? new object(),
                PaymentServiceId = paymentServiceId,
                TargetTradeAccountUid = targetAccountUid,
            };
    }

    public sealed class DepositSupplementV2
    {
        public long Amount { get; set; }
        public long PaymentMethodId { get; set; }
        public long AccountId { get; set; }
        public string Note { get; set; } = string.Empty;
        public dynamic Request { get; set; } = new object();
        public string RequestJson => JsonConvert.SerializeObject(Request);

        public static DepositSupplementV2 Build(long methodId, long amount, long accountId = 0, string? note = "",
            dynamic? request = null)
            => new()
            {
                Note = note,
                Amount = amount,
                Request = request ?? new object(),
                PaymentMethodId = methodId,
                AccountId = accountId,
            };
    }

    [Serializable]
    public sealed class WithdrawalSupplement
    {
        public long SourcePartyId { get; set; }
        public decimal ExchangeRate { get; set; }
        public CurrencyTypes TargetCurrencyId { get; set; }
        public object Reference { get; set; } = new();
        public string ToJson() => JsonConvert.SerializeObject(this);

        public static WithdrawalSupplement Build(long sourcePartyId, object? reference = null)
            => new()
            {
                SourcePartyId = sourcePartyId,
                Reference = reference ?? new object(),
            };
    }

    public sealed class WithdrawalSupplementV2
    {
        public long Amount { get; set; }
        public long PaymentMethodId { get; set; }
        public long AccountId { get; set; }
        public string Note { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public CurrencyTypes TargetCurrencyId { get; set; }
        public dynamic Request { get; set; } = new object();
        public string ToJson() => JsonConvert.SerializeObject(this);
    }

    public sealed class BankWithdrawalRequest
    {
        public string City { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Holder { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string BankCountry { get; set; } = string.Empty;
        public string ConfirmAccountNo { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public CurrencyTypes CurrencyId { get; set; }

        public void TrimStrings() => Utils.TrimStrings(this);
    }

    public sealed class TradeServiceSyncOn
    {
        public DateTime SyncedOn { get; set; }
    }

    public sealed class PaymentSupplement
    {
        public string Note { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public List<Activity> Activities { get; set; } = new();

        public static PaymentSupplement FromJson(string json)
            => JsonConvert.DeserializeObject<PaymentSupplement>(json) ?? new PaymentSupplement();

        public string ToJson()
            => JsonConvert.SerializeObject(this);

        public void AddActivity(string content)
            => Activities.Add(Activity.Of(content));

        public class Activity
        {
            public DateTime Time { get; set; }
            public string Content { get; set; } = string.Empty;
            public static Activity Of(string content) => new() { Time = DateTime.UtcNow, Content = content };
        }
    }

    public sealed class PaymentServicesSupplement
    {
        [JsonProperty("withdrawal"), JsonPropertyName("withdrawal")]
        public IList<long> Withdrawal { get; private init; } = new List<long>();

        [JsonProperty("deposit"), JsonPropertyName("deposit")]
        public IList<long> Deposit { get; private init; } = new List<long>();

        public IList<long> GetAllIds() => Deposit.Concat(Withdrawal).Where(x => x > 0).Distinct().ToList();

        public static PaymentServicesSupplement Build(IList<long> depositPaymentServiceIds,
            IList<long> withdrawalPaymentServiceIds)
            => new() { Deposit = depositPaymentServiceIds, Withdrawal = withdrawalPaymentServiceIds };

        public string ToJson() => JsonConvert.SerializeObject(this);

        public static PaymentServicesSupplement FromJson(string content)
        {
            return JsonConvert.DeserializeObject<PaymentServicesSupplement>(content) ?? new PaymentServicesSupplement();
        }
    }

    public class ReferralCodeSupplement
    {
        public List<AccountTypes> AccountTypes { get; set; } = new();
        public List<RebateLevelSchema> Schema { get; set; } = new();
        public object UserPreferences { get; set; } = new();

        public string GetSchemaJson() => Utils.JsonSerializeObject(Schema);
        public string GetUserPreferencesJson() => JsonConvert.SerializeObject(UserPreferences);

        public string ToJson() => JsonConvert.SerializeObject(this);

        public static ReferralCodeSupplement FromJson(string content)
        {
            try
            {
                return JsonConvert.DeserializeObject<ReferralCodeSupplement>(content) ?? new ReferralCodeSupplement();
            }
            catch (Exception)
            {
                return new ReferralCodeSupplement();
            }
        }
    }

    public class ReferralCodeBrokerSupplement
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; } = string.Empty;
        public List<RebateLevelSchema> Schema { get; set; } = null!;

        public string ToJson() => Utils.JsonSerializeObject(this);

        public static ReferralCodeBrokerSupplement FromJson(string content)
        {
            return JsonConvert.DeserializeObject<ReferralCodeBrokerSupplement>(content) ??
                   new ReferralCodeBrokerSupplement();
        }
    }

    public class AccountWizard
    {
        public bool KycFormCompleted { get; set; }
        public bool PaymentAccessGranted { get; set; }
        public bool WelcomeEmailSent { get; set; }
        public bool NoticeEmailSent { get; set; }
        public void SetKycFormCompleted() => KycFormCompleted = true;
        public void SetPaymentAccessGranted() => PaymentAccessGranted = true;
        public void SetWelcomeEmailSent() => WelcomeEmailSent = true;
        public void SetNoticeEmailSent() => NoticeEmailSent = true;
        public string ToJson() => Utils.JsonSerializeObject(this);

        public static AccountWizard FromJson(string content)
            => JsonConvert.DeserializeObject<AccountWizard>(content) ?? new AccountWizard();
    }

    public class UserWizard
    {
        public bool EmailConfirmed { get; set; }
        public bool VerificationCompleted { get; set; }
        public bool PaymentAccessGranted { get; set; }
        public bool AccountCreated { get; set; }
        public bool TradeAccountCreated { get; set; }
        public void SetVerificationCompleted() => VerificationCompleted = true;
        public void SetEmailConfirmed() => EmailConfirmed = true;
        public void SetPaymentAccessGranted() => PaymentAccessGranted = true;
        public void SetAccountCreated() => AccountCreated = true;
        public void SetTradeAccountCreated() => TradeAccountCreated = true;
        public string ToJson() => Utils.JsonSerializeObject(this);

        public static UserWizard FromJson(string content)
            => JsonConvert.DeserializeObject<UserWizard>(content) ?? new UserWizard();
    }
}