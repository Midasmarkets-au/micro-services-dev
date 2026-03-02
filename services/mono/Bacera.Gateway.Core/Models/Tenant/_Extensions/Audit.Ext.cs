using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class Audit : IEntity, IEntityAudit
{
    public Audit()
    {
        Data = string.Empty;
        Environment = string.Empty;
    }

    public sealed class TenantPageModel
    {
        public long Id { get; set; }

        public int Type { get; set; }

        public long RowId { get; set; }

        public int Action { get; set; }

        public long PartyId { get; set; }

        public DateTime CreatedOn { get; set; }
        public string OperatorName { get; set; } = "";

        public string Environment { get; set; } = "";

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string Data { get; set; } = "";

        public object Changes => Utils.JsonDeserializeObjectWithDefault<object>(Data);

        public TenantPageModel ApplyOperatorName(string name)
        {
            OperatorName = name;
            return this;
        }
    }

    public class EntityChanges
    {
        [JsonProperty("originalValues"), JsonPropertyName("originalValues")]
        public Dictionary<string, object> OriginalValues { get; set; } = new();

        [JsonProperty("currentValues"), JsonPropertyName("currentValues")]
        public Dictionary<string, object> CurrentValues { get; set; } = new();

        public static EntityChanges Create(Dictionary<string, object> originalValues,
            Dictionary<string, object> currentValues)
            => new()
            {
                OriginalValues = originalValues,
                CurrentValues = currentValues
            };

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}

public static class AuditExtensions
{
    public static IQueryable<Audit.TenantPageModel> ToTenantPageModel(this IQueryable<Audit> query)
        => query
            .Select(x => new Audit.TenantPageModel
            {
                Id = x.Id,
                Type = x.Type,
                RowId = x.RowId,
                Action = x.Action,
                PartyId = x.PartyId,
                CreatedOn = x.CreatedOn,
                Environment = x.Environment,
                Data = x.Data
            });
}