using System.ComponentModel.DataAnnotations.Schema;
using Bacera.Gateway.Core.Types;
using HashidsNet;

namespace Bacera.Gateway;

partial class Verification : IUserInfoAppendable, ILeadable
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.Verification, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.Verification]);

    public string HashId => HashEncode(Id);

    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();
    
    [NotMapped] public virtual UserInfo User { get; set; } = new();

    public bool TryChangeToAwaitingReview()
    {
        Status = (int)VerificationStatusTypes.AwaitingReview;
        UpdatedOn = DateTime.UtcNow;
        return true;
    }

    public bool TryChangeToUnderReview()
    {
        Status = (int)VerificationStatusTypes.UnderReview;
        UpdatedOn = DateTime.UtcNow;
        return true;
    }

    public bool TryChangeToAwaitingApprove()
    {
        Status = (int)VerificationStatusTypes.AwaitingApprove;
        UpdatedOn = DateTime.UtcNow;
        return true;
    }
}