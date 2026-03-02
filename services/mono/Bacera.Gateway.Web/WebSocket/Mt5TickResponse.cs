using Newtonsoft.Json;

namespace Bacera.Gateway.Web.WebSocket
{
    public class Mt5TickResponse
    {
        [JsonProperty("retcode")]
        public string RetCode { get; set; } = string.Empty;

        [JsonProperty("trans_id")]
        public string TransId { get; set; } = string.Empty;

        [JsonProperty("answer")]
        public List<Mt5Tick> Answer { get; set; } = new List<Mt5Tick>();
    }

    public class Mt5Tick
    {
        public string Symbol { get; set; } = string.Empty;
        public string Digits { get; set; } = string.Empty;
        public string Datetime { get; set; } = string.Empty;
        public string DatetimeMsc { get; set; } = string.Empty;
        public string Bid { get; set; } = string.Empty;
        public string Ask { get; set; } = string.Empty;
        public string Last { get; set; } = string.Empty;
        public string Volume { get; set; } = string.Empty;
        public string VolumeReal { get; set; } = string.Empty;
    }
}
