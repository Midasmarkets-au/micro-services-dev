using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = AccountReportGroup;

public partial class AccountReportGroup
{
    public sealed class TenantPageModel
    {
        public long Id { get; set; }
        public string Title => MetaData.Title;
        public string Group { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        [JsonIgnore] public string MetaDataRaw { get; set; } = "{}";

        [JsonIgnore]
        private MetaDataModel MetaData => Utils.JsonDeserializeObjectWithDefault<MetaDataModel>(MetaDataRaw);
    }

    public sealed class MetaDataModel
    {
        public string Title { get; set; } = string.Empty;
        public string ReceiverEmail { get; set; } = string.Empty;
        public List<string> Bccs { get; set; } = [];
        public List<string> Ccs { get; set; } = [];

        public string ToJson() => JsonConvert.SerializeObject(this);

        public static MetaDataModel FromCreateSpec(CreateReportSchemaSpec spec) => new()
        {
            Title = spec.Title,
            ReceiverEmail = spec.ReceiverEmail,
            Bccs = spec.Bccs,
            Ccs = spec.Ccs
        };

        public static MetaDataModel FromUpdateSpec(UpdateReportSchemaSpec spec) => new()
        {
            Title = spec.Title,
            ReceiverEmail = spec.ReceiverEmail,
            Bccs = spec.Bccs,
            Ccs = spec.Ccs
        };
    }
}

public static class AccountReportGroupViewModelExtensions
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> query) =>
        query.Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            Group = x.Group,
            Category = x.Category,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            Operator = x.OperatorParty.Email,
            MetaDataRaw = x.Parent == null ? x.MetaData : x.Parent.MetaData
        });
}