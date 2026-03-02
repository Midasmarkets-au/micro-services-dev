namespace Bacera.Gateway.Services;

public class ResetTradeAccountPasswordViewModel : EmailViewModel
{
    public override string TemplateTitle => Platform switch
    {
        PlatformTypes.MetaTrader5 => EmailTemplateTypes.ResetMt5TradeAccountPassword,
        PlatformTypes.MetaTrader4 => EmailTemplateTypes.ResetMt4TradeAccountPassword,
        _ => EmailTemplateTypes.ResetMt4TradeAccountPassword,
    };

    public string UserName { get; }
    public long TenantId { get; set; }
    public long PartyId { get; set; }
    public long AccountUid { get; }
    public long AccountNumber { get; }
    public string Token { get; }
    public string CallbackUrl { get; }
    public PlatformTypes Platform { get; }
    public string Link => GenerateLink();

    public ResetTradeAccountPasswordViewModel(long tenantId, string email, long partyId, long accountUid,
        long accountNumber,
        string username,
        string callbackUrl,
        string token, PlatformTypes platform)
    {
        TenantId = tenantId;
        PartyId = partyId;
        Email = email;
        Token = token;
        Platform = platform;
        UserName = username;
        AccountUid = accountUid;
        CallbackUrl = callbackUrl;
        AccountNumber = accountNumber;
    }

    public string GenerateLink() =>
        $"{CallbackUrl}/{TenantId}?pid={PartyId}&uid={AccountUid}&token={Token}&an={AccountNumber}";
}