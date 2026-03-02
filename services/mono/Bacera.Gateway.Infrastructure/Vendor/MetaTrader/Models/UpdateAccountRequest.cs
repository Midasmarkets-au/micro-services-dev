using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader.Models;

public class UpdateAccountRequest : BaseRequest
{
    [JsonProperty("name")] public string Name { get; set; } = string.Empty; // Required
    [JsonProperty("group")] public string Group { get; set; } = string.Empty; // Required
    [JsonProperty("leverage")] public int Leverage { get; set; } // Required
    [JsonProperty("password")] public string Password { get; set; } = string.Empty; // Required for Client login
    [JsonProperty("agent")] public long Agent { get; set; }
    [JsonProperty("email")] public string Email { get; set; } = string.Empty;
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;
    [JsonProperty("address")] public string Address { get; set; } = string.Empty;
    [JsonProperty("city")] public string City { get; set; } = string.Empty;
    [JsonProperty("state")] public string State { get; set; } = string.Empty;
    [JsonProperty("zipcode")] public string Zipcode { get; set; } = string.Empty;
    [JsonProperty("country")] public string Country { get; set; } = string.Empty;
    [JsonProperty("phone")] public string Phone { get; set; } = string.Empty;
    [JsonProperty("password_phone")] public string PasswordPhone { get; set; } = string.Empty;
    [JsonProperty("password_investor")] public string PasswordInvestor { get; set; } = string.Empty;
    [JsonProperty("comment")] public string Comment { get; set; } = string.Empty;
    [JsonProperty("enable")] public int Enable { get; set; } = 1;
    [JsonProperty("enable_read_only")] public string EnableReadOnly { get; set; } = string.Empty;
    [JsonProperty("send_reports")] public string SendReports { get; set; } = string.Empty;
    [JsonProperty("status")] public string Status { get; set; } = string.Empty;

    [JsonProperty("enable_change_password")]
    public string EnableChangePassword { get; set; } = "1";


    // ReSharper disable once StringLiteralTypo
    public override string RequestQuery() =>
        $"action=createaccount&request_id={Guid.NewGuid()}&URLEncode=1&" + this.ToQueryString();
}