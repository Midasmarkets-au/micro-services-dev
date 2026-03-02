namespace Bacera.Gateway;

public class ApplicationToken
{
    public long PartyId { get; set; }
    public long ReferenceId { get; set; }
    public TokenTypes ReferenceType { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public ApplicationToken SetToken(string token)
    {
        Token = token;
        return this;
    }

    public ApplicationToken Clone() => new()
    {
        PartyId = PartyId,
        ReferenceId = ReferenceId,
        ReferenceType = ReferenceType,
        Token = Token,
        Created = Created
    };

    public bool IsEmpty() => ReferenceId == 0 || string.IsNullOrEmpty(Token);

    public static ApplicationToken Empty => new();

    public static ApplicationToken Build(long partyId, TokenTypes tokenType, long referenceId, string token = "") =>
        new()
        {
            Token = token,
            PartyId = partyId,
            ReferenceType = tokenType,
            ReferenceId = referenceId,
        };
}