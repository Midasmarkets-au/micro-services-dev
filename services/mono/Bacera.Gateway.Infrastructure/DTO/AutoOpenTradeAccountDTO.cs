using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Bacera.Gateway.DTO;

public class AutoOpenTradeAccountDTO
{
    public sealed class ClientAccount
    {
        [Required]
        [JsonProperty("currency")]
        [JsonPropertyName("currency")]
        public CurrencyTypes CurrencyId { get; set; }

        [Required]
        [JsonProperty("accountType")]
        [JsonPropertyName("accountType")]
        public AccountTypes AccountType { get; set; }

        [Required]
        [JsonProperty("platform")]
        [JsonPropertyName("platform")]
        public PlatformTypes Platform { get; set; }

        [Required]
        [JsonProperty("serviceId")]
        [JsonPropertyName("serviceId")]
        public int ServiceId { get; set; }

        [Required]
        [JsonProperty("leverage")]
        [JsonPropertyName("leverage")]
        public int Leverage { get; set; }

        [JsonProperty("referral")]
        [JsonPropertyName("referral")]
        public string ReferCode { get; set; } = string.Empty;

        public const FundTypes FundType = FundTypes.Ips;

        public static bool TryParse(string json, out ClientAccount result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<ClientAccount>(json)!;
                return true;
            }
            catch
            {
                result = new ClientAccount();
                return false;
            }
        }

        public static ClientAccount FromApplicationSupplementJson(string json)
        {
            var applicationSupplement = Utils.JsonDeserializeObjectWithDefault<ApplicationSupplement>(json);
            var result = new ClientAccount
            {
                AccountType = applicationSupplement.AccountType!.Value,
                CurrencyId = applicationSupplement.CurrencyIdType,
                Platform = applicationSupplement.Platform!.Value,
                ServiceId = applicationSupplement.ServiceId!.Value,
                Leverage = applicationSupplement.Leverage!.Value,
                ReferCode = applicationSupplement.ReferCode!
            };

            return result;
        }
    }

    public sealed class Mt4GroupAndSchemaConfig
    {
        private Dictionary<string, Dictionary<string, string[]>> Dictionary { get; set; } = new();

        public static Mt4GroupAndSchemaConfig Build(string json) => new()
        {
            Dictionary = Utils.JsonDeserializeObjectWithDefault<Dictionary<string, Dictionary<string, string[]>>>(json)
        };

        public (string?, long?) GetGroupAndSchema(string typeOptionName, decimal pips, decimal commission)
        {
            if (pips != 0 && commission != 0)
                return (null, null);

            if (!Dictionary.TryGetValue(typeOptionName.ToUpper(), out var groupAndSchema))
                return (null, null);

            if (groupAndSchema.TryGetValue("ALL", out var allSchema))
            {
                // Fix double-escaping issue: PostgreSQL stores backslashes as \\ in JSON,
                // but sometimes they get double-escaped. Unescape them here.
                var groupName = allSchema[0]?.Replace("\\\\", "\\") ?? null;
                return (groupName, long.Parse(allSchema[1]));
            }

            var key = $"p{pips:0}-c{commission:0}".ToUpper();

            if (!groupAndSchema.TryGetValue(key, out var groupSchema))
                return (null, null);
            
            // Fix double-escaping issue: PostgreSQL stores backslashes as \\ in JSON,
            // but sometimes they get double-escaped. Unescape them here.
            var unescapedGroupName = groupSchema[0]?.Replace("\\\\", "\\") ?? null;
            return (unescapedGroupName, long.Parse(groupSchema[1]));
        }
    }

    public sealed class Mt5GroupAndSchemaConfig
    {
        // Structure: Dictionary<AccountType, Dictionary<CurrencyCode, Dictionary<OptionKey, string[]>>>
        // Example: Dictionary["STANDARD"]["840"]["P0-C0"] = ["GroupName", "SchemaId", "CurrencyCodes"]
        private Dictionary<string, Dictionary<string, Dictionary<string, string[]>>> Dictionary { get; set; } = new();

        public static Mt5GroupAndSchemaConfig Build(string json)
        {
            var deserialized = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (deserialized == null)
                return new Mt5GroupAndSchemaConfig();

            var result = new Dictionary<string, Dictionary<string, Dictionary<string, string[]>>>();
            
            foreach (var accountTypeEntry in deserialized)
            {
                var accountType = accountTypeEntry.Key;
                var accountTypeValue = accountTypeEntry.Value;
                var accountTypeJson = JsonConvert.SerializeObject(accountTypeValue);
                
                // Try to parse as new format first (with currency level)
                var newFormatDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string[]>>>(accountTypeJson);
                
                if (newFormatDict != null && newFormatDict.Count > 0)
                {
                    // Check if keys are numeric (currency codes) - this indicates new format
                    var firstKey = newFormatDict.Keys.First();
                    if (int.TryParse(firstKey, out _))
                    {
                        // New format: Dictionary<CurrencyCode, Dictionary<OptionKey, string[]>>
                        result[accountType] = newFormatDict;
                        continue;
                    }
                }
                
                // Old format: Dictionary<OptionKey, string[]>
                // Migrate to new format by wrapping in currency code "840" (USD)
                var oldFormatDict = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(accountTypeJson);
                if (oldFormatDict != null)
                {
                    var currencyDict = new Dictionary<string, Dictionary<string, string[]>>
                    {
                        ["840"] = oldFormatDict // Default to USD for backward compatibility
                    };
                    result[accountType] = currencyDict;
                }
            }
            
            return new Mt5GroupAndSchemaConfig { Dictionary = result };
        }

        public (string?, long?) GetGroupAndSchema(string typeOptionName, decimal pips, decimal commission, CurrencyTypes currencyId)
        {
            if (pips != 0 && commission != 0)
                return (null, null);

            if (!Dictionary.TryGetValue(typeOptionName.ToUpper(), out var currencyLevelDict))
                return (null, null);

            // Convert currency enum to string (e.g., CurrencyTypes.USD (840) -> "840")
            var currencyCode = ((int)currencyId).ToString();

            // Try to get the currency-specific configuration
            if (!currencyLevelDict.TryGetValue(currencyCode, out var groupAndSchema))
            {
                // Fallback: if currency not found, try to use the first available currency (for backward compatibility)
                if (currencyLevelDict.Count == 0)
                    return (null, null);
                
                // Use the first currency as fallback
                groupAndSchema = currencyLevelDict.Values.First();
            }

            if (groupAndSchema.TryGetValue("ALL", out var allSchema))
            {
                // Fix double-escaping issue: PostgreSQL stores backslashes as \\ in JSON,
                // but sometimes they get double-escaped. Unescape them here.
                var groupName = allSchema[0]?.Replace("\\\\", "\\") ?? null;
                return (groupName, long.Parse(allSchema[1]));
            }

            var key = $"P{pips:0}-C{commission:0}";

            if (!groupAndSchema.TryGetValue(key, out var groupSchema))
                return (null, null);
            
            // Fix double-escaping issue: PostgreSQL stores backslashes as \\ in JSON,
            // but sometimes they get double-escaped. Unescape them here.
            var unescapedGroupName = groupSchema[0]?.Replace("\\\\", "\\") ?? null;
            return (unescapedGroupName, long.Parse(groupSchema[1]));
        }
    }
}