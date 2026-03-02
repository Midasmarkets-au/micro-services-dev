using Bacera.Gateway.Core.Types;
using HashidsNet;

namespace Bacera.Gateway;

using M = PayoutRecord;

public partial class PayoutRecord
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.PayoutRecord, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.PayoutRecord]);

    public string HashId => HashEncode(Id);
    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();

    public InfoModel GetInfoModel()
    {
        var result = Utils.JsonDeserializeObjectWithDefault<InfoModel>(Info);
        return result;
    }
}