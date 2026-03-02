using Bacera.Gateway.Core.Types;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

using M = Crypto;

public partial class Crypto
{
    public sealed class Setting
    {
        public int PayExpiredTimeInMinutes { get; set; }

        public int GetPayExpiredTimeInMinutes() => PayExpiredTimeInMinutes > 0 ? PayExpiredTimeInMinutes : 1;
    }

    public bool IsOnHold() => Status == (int)CryptoStatusTypes.InUse || InUsePaymentId != null;

    public M Release()
    {
        Status = (int)CryptoStatusTypes.Idle;
        InUsePaymentId = null;
        UpdatedOn = DateTime.UtcNow;
        return this;
    }
}