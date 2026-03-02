using Bacera.Gateway.Core.Types;
using HashidsNet;

namespace Bacera.Gateway;

public partial class Post
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.BCRDiscoverPost, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.BCRDiscoverPost]);

    public string HashId => HashEncode(Id);

    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();
}