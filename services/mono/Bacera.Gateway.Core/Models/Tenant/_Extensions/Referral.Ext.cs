namespace Bacera.Gateway;

partial class Referral
{
    public static Referral Build(long codeId, long referrerPartyId, long referredPartyId, string code,
        string module = "", long rowId = 0)
        => new()
        {
            ReferralCodeId = codeId,
            ReferrerPartyId = referrerPartyId,
            ReferredPartyId = referredPartyId,
            Code = code,
            Module = module,
            RowId = rowId,
        };

    public sealed class CreateSpec
    {
        public long ReferrerPartyId { get; set; }
        public long ReferredPartyId { get; set; }
        public string Code { get; set; } = null!;
        public long ReferralCodeId { get; set; }

        public static CreateSpec Build(long codeId, string code, long referrerPartyId, long referredPartyId)
            => new()
            {
                Code = code,
                ReferralCodeId = codeId,
                ReferrerPartyId = referrerPartyId,
                ReferredPartyId = referredPartyId,
            };
    }
}