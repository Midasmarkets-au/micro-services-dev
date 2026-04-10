using HashidsNet;

namespace Bacera.Gateway.Auth.Security;

/// <summary>
/// Mirrors hashids.rs — encodes Party/User IDs using the same salts and alphabets as mono.
/// </summary>
public static class HashIds
{
    private const string PartySalt = "BCRPartyId";
    private const string PartyAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ0123456789";
    private const string UserSalt = "BCRUserId";
    private const string UserAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ0123456789";
    private const int MinLength = 8;

    private static readonly Hashids PartyHashids = new(PartySalt, MinLength, PartyAlphabet);
    private static readonly Hashids UserHashids = new(UserSalt, MinLength, UserAlphabet);

    public static string EncodePartyId(long id) => PartyHashids.EncodeLong(id);
    public static long DecodePartyId(string hash) => PartyHashids.DecodeLong(hash).FirstOrDefault();

    public static string EncodeUserId(long id) => UserHashids.EncodeLong(id);
    public static long DecodeUserId(string hash) => UserHashids.DecodeLong(hash).FirstOrDefault();
}
