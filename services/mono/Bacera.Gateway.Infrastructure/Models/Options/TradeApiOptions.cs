namespace Bacera.Gateway
{
    public abstract class TradeApiOptions
    {
        // Must be HTTPS protocol
        public string Endpoint { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
