using System.Text;
using Bacera.Gateway.Core.Types;
using HashidsNet;

namespace Bacera.Gateway;

partial class Party
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.BCRPartyId, 8, HashIdSaltTypes.Dictionary[HashIdSaltTypes.BCRPartyId]);
    public string HashId => HashEncode(Id);
    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();
    public string GuessNativeName() => string.IsNullOrWhiteSpace(NativeName) ? $"{FirstName} {LastName}" : NativeName;
    public static Party Create(string name, int? id = 0, string? code = null, long? pid = null, string? note = null)
        => new()
        {
            Id = id ?? 0,
            Pid = pid,
            Name = name,
            Note = note ?? "",
            Code = code ?? Guid.NewGuid().ToString()[..10],
        };

    public Party UpdateSearchText()
    {
        SearchText = new StringBuilder().Append($"{Id}")
            .Append($",{Uid}")
            .Append($",{Name}")
            .Append($",{Code}")
            .Append($",{Email}")
            .Append($",{NativeName}")
            .Append($",{Id}")
            .Append($",{Uid}")
            .Append($",{FirstName} {LastName}")
            .Append($",{LastName} {FirstName}")
            .Append($",{NativeName}")
            .Append($",{Email}")
            .Append($",{CCC}")
            .Append($",{CountryCode}")
            .Append($",{IdNumber}")
            .Append($",{LastLoginIp}")
            .Append($",{RegisteredIp}")
            .Append($",{PhoneNumber}").ToString();
        return this;
    }
}