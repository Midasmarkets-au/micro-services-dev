using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = Refund;

partial class Refund : ICloneable, IHasMatter
{
    public object Clone()
    {
        return MemberwiseClone();
    }

    public M DeepCopy()
    {
        var copyObj = (M)MemberwiseClone();
        return copyObj;
    }

    public bool IsEmpty() => Id == 0;

    public static Refund Build(long targetPartyId
        , RefundTargetTypes targetType
        , long targetId
        , long amount
        , FundTypes fundType
        , CurrencyTypes currency = CurrencyTypes.USD
        , string comment = ""
    )
        => new()
        {
            PartyId = targetPartyId,
            Amount = amount,
            CurrencyId = (int)currency,
            TargetId = targetId,
            TargetType = (short)targetType,
            CreatedOn = DateTime.UtcNow,
            FundType = (int)fundType,
            Comment = comment,
            IdNavigation = Matter.Build().Refund(),
        };

    public class ResponseModel
    {
        public long PartyId { get; set; }
        public long Id { get; set; }
        public long Amount { get; set; }
        public int CurrencyId { get; set; }
        public long TargetId { get; set; }
        public short TargetType { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}

public static class RefundExt
{
}