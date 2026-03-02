using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bacera.Gateway;

using M = Bacera.Gateway.ReferralCode;

partial class ReferralCode
{
    public sealed class ParentPageModel
    {
        public int ServiceType { get; set; }

        public int IsDefault { get; set; }


        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public string Code { get; set; } = "";

        public string Name { get; set; } = "";

        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]

        public string SummaryJson { get; set; } = "{}";

        public dynamic Summary => Utils.JsonDeserializeDynamic(SummaryJson);
    }
}

public static class ReferralCodeViewModelExtension
{
    public static IQueryable<M.ParentPageModel> ToParentPageModel(this IQueryable<M> query) =>
        query.Select(x => new M.ParentPageModel
        {
            ServiceType = x.ServiceType,
            IsDefault = x.IsDefault,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            Code = x.Code,
            Name = x.Name,
            SummaryJson = x.Summary
        });
}