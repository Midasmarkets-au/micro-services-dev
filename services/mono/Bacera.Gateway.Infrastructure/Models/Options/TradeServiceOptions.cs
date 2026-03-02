using Bacera.Gateway.Vendor.MetaTrader;
using Newtonsoft.Json;

namespace Bacera.Gateway
{
    [Serializable]
    public class TradeServiceOptions
    {
        public ApiOptions? Api { get; set; }
        public DatabaseWithTablesOptions? Database { get; set; }
        public List<string>? Groups { get; set; }
        public List<int>? Leverages { get; set; }
        public Dictionary<string, long> AccountPrefixes { get; set; } = new();
        public Dictionary<string, string> DefaultGroup { get; set; } = new();
        public string ConnectionString { get; set; } = null!;

        public bool IsDatabaseValidated()
            => Database != null
               && Database.IsValidated()
               && !string.IsNullOrEmpty(Database.UserTableName)
               && !string.IsNullOrEmpty(Database.TradeTableName);

        public static bool TryParse(string json, out TradeServiceOptions options)
        {
            try
            {
                options = JsonConvert.DeserializeObject<TradeServiceOptions>(json)!;
                return true;
            }
            catch
            {
                options = new TradeServiceOptions();
                return false;
            }
        }
    }
}