using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class SalesRebateSchema
{
    public class BaseModel
    {
        public long SalesAccountId { get; set; }
        public long SalesAccountUid { get; set; }
        public long RebateAccountId { get; set; }
        public long RebateAccountUid { get; set; }
        public int Rebate { get; set; }
        [JsonIgnore] public string ExcludeAccountJson { get; set; } = "[]";
        public List<string> ExcludeAccount => Utils.JsonDeserializeObjectWithDefault<List<string>>(ExcludeAccountJson);

        [JsonIgnore] public string ExcludeSymbolJson { get; set; } = "[]";
        public List<string> ExcludeSymbol => Utils.JsonDeserializeObjectWithDefault<List<string>>(ExcludeSymbolJson);

        public SalesRebateSchemaStatusTypes Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? Note { get; set; }
        public string OperatorFirstName { get; set; } = string.Empty;
        public int AlphaRebate { get; set; }
        public int ProRebate { get; set; }
        public SalesRebateSchemaSalesTypes SalesType { get; set; }
        public SalesRebateSchemaScheduleTypes Schedule { get; set; }
    }
    
    public class TenantPageModel : BaseModel
    {
        public long Id { get; set; }
        public string SalesName { get; set; } = null!; 
        public string SalesCode { get; set; } = null!;
    }
}

public static class SalesRebateSchemaExtensions
{
    public static IQueryable<SalesRebateSchema.TenantPageModel> ToTenantPageModel(this IQueryable<SalesRebateSchema> @event)
        => @event.Select(x => new SalesRebateSchema.TenantPageModel
        {
            Id = x.Id,
            SalesAccountId = x.SalesAccountId,
            SalesAccountUid = x.SalesAccount.Uid,
            SalesName = x.SalesAccount.Party.NativeName,
            SalesCode = x.SalesAccount.Code,
            RebateAccountId = x.RebateAccountId,
            RebateAccountUid = x.RebateAccount.Uid,
            Rebate = x.Rebate,
            ExcludeAccountJson = x.ExcludeAccount,
            ExcludeSymbolJson = x.ExcludeSymbol,
            Status = (SalesRebateSchemaStatusTypes)x.Status,
            Note = x.Note,
            AlphaRebate = x.AlphaRebate,
            ProRebate = x.ProRebate,
            SalesType = (SalesRebateSchemaSalesTypes)x.SalesType,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            OperatorFirstName = x.OperatorParty.FirstName, 
            Schedule = (SalesRebateSchemaScheduleTypes)x.Schedule
        });
}