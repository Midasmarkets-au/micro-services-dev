using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = ApiLog;

public partial class ApiLog
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public sealed class TenantPageModel
    {
        public long? Id { get; set; }

        public long PartyId { get; set; }

        public int? StatusCode { get; set; }
        public string? ConnectionId { get; set; }
        public string? Method { get; set; }

        public string? Action { get; set; }

        public string? UserAgent { get; set; }

        public string? Referer { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [JsonInclude]
        public string? ParametersString { get; set; }

        public dynamic? Parameters => ParametersString is null ? null : Utils.JsonDeserializeDynamic(ParametersString);

        [Newtonsoft.Json.JsonIgnore]
        [JsonInclude]
        public string? RequestContentString { get; set; }

        public dynamic? RequestContent =>
            RequestContentString is null ? null : Utils.JsonDeserializeDynamic(RequestContentString);

        [Newtonsoft.Json.JsonIgnore]
        [JsonInclude]
        public string? ResponseContentString { get; set; }

        public dynamic? ResponseContent =>
            ResponseContentString is null ? null : Utils.JsonDeserializeDynamic(ResponseContentString);

        public string Email { get; set; } = "";

        public string? Ip { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }

        public TenantUserBasicModel User { get; set; } = TenantUserBasicModel.Empty();

        public long? DurationInSeconds => UpdatedOn != null && CreatedOn != null
            ? (long)(UpdatedOn - CreatedOn).Value.TotalMilliseconds
            : null;
    }
}

public static class ApiLogExt
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> query, bool hideEmail = false) =>
        query
        .Include(x => x.Party.PartyComments)
        .Include(x => x.Party.Tags)
        .Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            PartyId = x.PartyId,
            StatusCode = x.StatusCode,
            ConnectionId = x.ConnectionId,
            Method = x.Method,
            Action = x.Action,
            ParametersString = x.Parameters,
            RequestContentString = x.RequestContent,
            ResponseContentString = x.ResponseContent,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            User = x.Party.ToTenantBasicViewModel(hideEmail),
            Ip = x.Ip
        });
}