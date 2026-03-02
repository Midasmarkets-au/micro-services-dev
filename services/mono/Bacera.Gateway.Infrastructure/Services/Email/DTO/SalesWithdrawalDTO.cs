namespace Bacera.Gateway.Services.DTO;

public class SalesWithdrawalDTO
{
    public sealed class Config
    {
        public string IncludePartyTag { get; set; } = null!;
        public string Email { get; set; } = null!;
        public List<string> BccEmails { get; set; } = [];
    }
}