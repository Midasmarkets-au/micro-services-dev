namespace Bacera.Gateway;

partial class RebateBaseSchema
{
    public class CreateSpec
    {
        public string Name { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public List<RebateDirectSchemaItem.CreateSpec> Items { get; set; } = null!;
    }

    public class UpdateSpec
    {
        public string Name { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }

    public class ResponseModel
    {
        public long Id { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public long CreatedBy { get; set; }
        public string CreatedByPartyName { get; set; } = string.Empty;
    }
}

public static class RebateRuleTemplateExtensions
{
    public static IQueryable<RebateBaseSchema.ResponseModel> ToResponseModel(
        this IQueryable<RebateBaseSchema> query)
        => query.Select(x => new RebateBaseSchema.ResponseModel
        {
            Id = x.Id,

            CreatedOn = x.CreatedOn,
            CreatedBy = x.CreatedBy,
            CreatedByPartyName = x.CreatedByNavigation.Name,
            UpdatedOn = x.UpdatedOn,
            Name = x.Name,
            Note = x.Note,
            // TODO: Update after database migration
            // ConfirmedByPartyName = x.ConfirmedBy != null ? x.ConfirmedByNavigation!.Name : string.Empty,
        });
}