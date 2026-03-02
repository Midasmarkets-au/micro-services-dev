using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = TradeService;

partial class TradeService : IEntity<int>
{
    public bool IsEmpty() => Id == default;

    public sealed class ClientResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Platform { get; set; }
        public int Priority { get; set; }

        public short IsAllowAccountCreation { get; set; }
        public string Description { get; set; } = string.Empty;
        public ICollection<string> Groups { get; set; } = new List<string>();
    }

    /// <summary>
    /// Get Options from Configuration
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetOptions<T>() where T : class, new() =>
        JsonConvert.DeserializeObject<T>(Configuration)
        ?? new T();

    public class UpdateGroupSpec
    {
        public List<string> groups = new();
    }

    public class AddOrDeleteGroupSpec
    {
        [Required] public string Group = null!;
    }

    public sealed class BasicModel
    {
        public int Id { get; set; }
        public short Platform { get; set; }
        public string Configuration { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public sealed class ClientTradingPlatformAvailableModel
    {
        public int ServiceId { get; set; }
        public PlatformTypes Platform { get; set; }
    }

    /// <summary>
    /// Configuration structure (JSON data in table) for TradeService (MT5)
    /// </summary>
    public sealed class ConfigurationModel
    {
        [JsonProperty("Api")]
        public ApiConfig? Api { get; set; }

        [JsonProperty("Database")]
        public DatabaseConfig? Database { get; set; }

        [JsonProperty("Groups")]
        public List<string>? Groups { get; set; }

        [JsonProperty("Leverages")]
        public object? Leverages { get; set; }

        [JsonProperty("AccountPrefixes")]
        public Dictionary<string, long>? AccountPrefixes { get; set; }

        [JsonProperty("DefaultGroup")]
        public Dictionary<string, string>? DefaultGroup { get; set; }

        [JsonProperty("ConnectionString")]
        public string? ConnectionString { get; set; }

        public sealed class ApiConfig
        {
            [JsonProperty("Host")]
            public string? Host { get; set; }

            [JsonProperty("Port")]
            public int Port { get; set; }

            [JsonProperty("User")]
            public string? User { get; set; }

            [JsonProperty("Password")]
            public string? Password { get; set; }
        }

        public sealed class DatabaseConfig
        {
            [JsonProperty("UserTableName")]
            public string? UserTableName { get; set; }

            [JsonProperty("TradeTableName")]
            public string? TradeTableName { get; set; }

            [JsonProperty("Username")]
            public string? Username { get; set; }

            [JsonProperty("Password")]
            public string? Password { get; set; }

            [JsonProperty("Host")]
            public string? Host { get; set; }

            [JsonProperty("Port")]
            public int Port { get; set; }

            [JsonProperty("Database")]
            public string? Database { get; set; }

            [JsonProperty("ConvertZeroDateTime")]
            public bool? ConvertZeroDateTime { get; set; }

            [JsonProperty("ConnectionString")]
            public string? ConnectionString { get; set; }
        }
    }

    /// <summary>
    /// Response model for Account Prefixes
    /// </summary>
    public sealed class AccountPrefixesResponse
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public Dictionary<string, long> AccountPrefixes { get; set; } = new();
    }
}

public static class TradeServiceExtension
{
    public static IQueryable<M.BasicModel> ToBasicModel(this IQueryable<CentralTradeService> query) =>
        query.Select(x => new M.BasicModel
        {
            Id = x.Id,
            Platform = x.Platform,
            Name = x.Name,
            Configuration = x.Configuration
        });
}