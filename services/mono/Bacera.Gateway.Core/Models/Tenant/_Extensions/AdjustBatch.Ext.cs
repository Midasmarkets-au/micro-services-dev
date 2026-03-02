using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class AdjustBatch
{
    public string GetFileDirInS3(long tenantId) => $"t_{tenantId}/adjust/";

    public string GetFileNameInS3() =>
        $"{Id}-{CreatedOn:yyyy-MM-dd}-{Enum.GetName(typeof(AdjustTypes), Type) ?? ""}.csv";

    public string GetFileFullPathInS3(long tenantId) => $"{GetFileDirInS3(tenantId)}{GetFileNameInS3()}";

    public AdjustBatchResult GetResult()
    {
        try
        {
            return JsonConvert.DeserializeObject<AdjustBatchResult>(Result) ?? new AdjustBatchResult();
        }
        catch
        {
            return new AdjustBatchResult();
        }
    }

    public class AdjustBatchResult
    {
        public long AdjustBatchId { get; set; }
        public long TotalAccounts { get; set; }
        public long AccountsInOurSystem { get; set; }
        public long TotalAmount { get; set; }
        public long SuccessCount { get; set; }

        public static AdjustBatchResult Build(long adjustBatchId, long totalAccount, long accountsInOurSystem,
            long totalAmount) =>
            new()
            {
                AdjustBatchId = adjustBatchId,
                TotalAccounts = totalAccount,
                AccountsInOurSystem = accountsInOurSystem,
                TotalAmount = totalAmount
            };
    }
}