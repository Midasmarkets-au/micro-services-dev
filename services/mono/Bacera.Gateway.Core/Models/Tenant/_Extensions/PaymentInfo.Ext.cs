using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Bacera.Gateway.Core.Types;
using HashidsNet;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class PaymentInfo
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.BCRPaymentInfo, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.BCRPaymentInfo]);

    public string HashId => HashEncode(Id);
    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();

    public sealed class BankInfo
    {
        public string BankName { get; set; } = string.Empty;
        public string SwiftCode { get; set; } = string.Empty;
        public string RoutingNumber { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public sealed class UpdateSpec
    {
        public string Name { get; set; } = null!;

        public dynamic Info { get; set; } = null!;
    }

    public class ClientResponseModel
    {
        public long Id { get; set; }
        public PaymentPlatformTypes PaymentPlatform { get; set; }

        public long? PaymentServiceId { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public string Name { get; set; } = null!;

        [Newtonsoft.Json.JsonIgnore] public string InfoJson { get; set; } = null!;

        public object Info => JsonConvert.DeserializeObject(string.IsNullOrEmpty(InfoJson) ? "{}" : InfoJson) ??
                              new object();
    }

    public class TenantResponseModel
    {
        public long Id { get; set; }

        public long PartyId { get; set; }

        public PaymentPlatformTypes PaymentPlatform { get; set; }

        public long? PaymentServiceId { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public string Name { get; set; } = null!;
        public string PartyName { get; set; } = null!;
        public string Email { get; set; } = null!;

        [Newtonsoft.Json.JsonIgnore] public string InfoJson { get; set; } = null!;

        public object Info
        {
            get
            {
                if (PaymentPlatform == PaymentPlatformTypes.Wire)
                    return Utils.JsonDeserializeObjectWithDefault<BankWireInfo>(InfoJson);

                if (PaymentPlatform == PaymentPlatformTypes.USDT)
                    return Utils.JsonDeserializeObjectWithDefault<UsdtInfo>(InfoJson);

                return new object();
            }
        }
    }

    public sealed class CreateAndUpdateSpec
    {
        public PaymentPlatformTypes PaymentPlatform { get; set; }

        public string Name { get; set; } = null!;

        public dynamic Info { get; set; } = null!;

        public bool IsValid()
        {
            string json = JsonConvert.SerializeObject(Info);

            if (PaymentPlatform == PaymentPlatformTypes.Wire)
            {
                try
                {
                    JsonConvert.DeserializeObject<BankWireInfo>(json);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            if (PaymentPlatform == PaymentPlatformTypes.USDT)
            {
                try
                {
                    var info = JsonConvert.DeserializeObject<UsdtInfo>(json);
                    info!.WalletAddress = info.WalletAddress.Trim();
                    Info = info;
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
    }

    public sealed class BankWireInfo
    {
        [Required, JsonProperty("name"), JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required, JsonProperty("holder"), JsonPropertyName("holder")]
        public string Holder { get; set; } = string.Empty;

        [Required, JsonProperty("bankCountry"), JsonPropertyName("bankCountry")]
        public string BankCountry { get; set; } = string.Empty;

        [Required, JsonProperty("swiftCode"), JsonPropertyName("swiftCode")]
        public string SwiftCode { get; set; } = string.Empty;

        [Required, JsonProperty("bankName"), JsonPropertyName("bankName")]
        public string BankName { get; set; } = string.Empty;

        [Required, JsonProperty("branchName"), JsonPropertyName("branchName")]
        public string BranchName { get; set; } = string.Empty;

        [Required, JsonProperty("state"), JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        [Required, JsonProperty("city"), JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [Required, JsonProperty("accountNo"), JsonPropertyName("accountNo")]
        [RegularExpression(@"\d{16,19}", ErrorMessage = "Account number must be between 16 and 19 digits.")]
        public string AccountNo { get; set; } = string.Empty;

        [Required, JsonProperty("confirmAccountNo"), JsonPropertyName("confirmAccountNo")]
        [Compare("AccountNo", ErrorMessage = "Account numbers do not match.")]
        public string ConfirmAccountNo { get; set; } = string.Empty;
    }

    public sealed class UsdtInfo
    {
        [Required, JsonProperty("name"), JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required, JsonProperty("walletAddress"), JsonPropertyName("walletAddress")]
        public string WalletAddress { get; set; } = string.Empty;

        public static UsdtInfo FromJson(string json) => Utils.JsonDeserializeObjectWithDefault<UsdtInfo>(json);
    }
}

public static class PaymentInfoExtension
{
    public static IQueryable<PaymentInfo.ClientResponseModel> ToClientResponseModels(this IQueryable<PaymentInfo> query)
        => query.Select(x => new PaymentInfo.ClientResponseModel
        {
            Id = x.Id,
            Name = x.Name,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            PaymentServiceId = x.PaymentServiceId,
            PaymentPlatform = (PaymentPlatformTypes)x.PaymentPlatform,
            InfoJson = x.Info
        });

    public static IQueryable<PaymentInfo.TenantResponseModel> ToTenantResponseModels(this IQueryable<PaymentInfo> query)
        => query.Select(x => new PaymentInfo.TenantResponseModel
        {
            Id = x.Id,
            Name = x.Name,
            PartyId = x.PartyId,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            PaymentServiceId = x.PaymentServiceId,
            PaymentPlatform = (PaymentPlatformTypes)x.PaymentPlatform,
            PartyName = x.Party.NativeName,
            Email = x.Party.Email,
            InfoJson = x.Info
        });

    public static PaymentInfo.ClientResponseModel ToResponseModel(this PaymentInfo x)
        => new PaymentInfo.ClientResponseModel
        {
            Name = x.Name,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            PaymentServiceId = x.PaymentServiceId,
            PaymentPlatform = (PaymentPlatformTypes)x.PaymentPlatform,
            InfoJson = x.Info
        };
}