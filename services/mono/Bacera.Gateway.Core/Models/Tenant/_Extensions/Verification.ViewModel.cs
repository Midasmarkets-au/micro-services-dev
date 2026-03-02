namespace Bacera.Gateway;

using M = Verification;

partial class Verification
{
    public sealed class ClientPageModel
    {
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; init; }

        public string HashId => HashEncode(Id);

        public long Balance { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public FundTypes FundType { get; set; }
    }
}