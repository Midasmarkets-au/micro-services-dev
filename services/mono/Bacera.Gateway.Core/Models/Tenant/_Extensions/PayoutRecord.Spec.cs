using Microsoft.AspNetCore.Http;

namespace Bacera.Gateway;

using M = PayoutRecord;

public partial class PayoutRecord
{
    public sealed class IdSpec
    {
        public long Id { get; set; }
    }

    public class BatchCreateSpec
    {
        public long PaymentMethodId { get; set; }
        public IFormFile File { get; set; } = null!;
    }

    public class BatchConfirmSpec
    {
        public string BatchUid { get; set; } = null!;
    }
}