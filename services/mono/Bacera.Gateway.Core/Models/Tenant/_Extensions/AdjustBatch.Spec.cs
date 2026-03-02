using Bacera.Gateway.Core.Types;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class AdjustBatch
{
    public class CreateSpec
    {
        public int ServiceId { get; set; }
        public AdjustTypes Type { get; set; }

        public string Note { get; set; } = "";

        public IFormFile File { get; set; } = null!;
    }

    public class TenantResponseModel
    {
        public long Id { get; set; }
        public int ServiceId { get; set; }
        public AdjustTypes Type { get; set; }
        public string OperatorName { get; set; } = "";
        public string Note { get; set; } = "";

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public string ResultJson { get; set; } = "{}";

        public dynamic Result => JsonConvert.DeserializeObject<dynamic>(ResultJson) ?? new { };

        public long TotalAccounts { get; set; }

        public AdjustBatchStatusTypes Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}

public static class AdjustBatchExtension
{
    public static IQueryable<AdjustBatch.TenantResponseModel> ToTenantResponseModel(this IQueryable<AdjustBatch> query)
        => query.Select(x => new AdjustBatch.TenantResponseModel
        {
            Id = x.Id,
            ServiceId = x.ServiceId,
            Type = (AdjustTypes)x.Type,
            OperatorName = x.OperatorParty.NativeName,
            Note = x.Note,
            ResultJson = x.Result,
            TotalAccounts = x.AdjustRecords.Count,
            Status = (AdjustBatchStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn
        });
}