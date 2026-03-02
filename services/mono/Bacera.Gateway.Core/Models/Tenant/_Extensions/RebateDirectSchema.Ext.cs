namespace Bacera.Gateway;

public static class RebateDirectSchemaExtensions
{
    public static IQueryable<RebateDirectSchema.ResponseModel> ToResponseModel(this IQueryable<RebateDirectSchema> query)
        => query.Select(x => new RebateDirectSchema.ResponseModel
        {
            Id = x.Id,
            Name = x.Name,
            Note = x.Note,
            UpdatedOn = x.UpdatedOn,

            CreatedOn = x.CreatedOn,
            CreatedBy = x.CreatedBy,
            CreatedByPartyName = x.CreatedByNavigation.Name,

            ConfirmedOn = x.ConfirmedOn,
            ConfirmedBy = x.ConfirmedBy,
            ConfirmedByPartyName = x.ConfirmedBy != null
                ? x.ConfirmedByNavigation!.Name
                : string.Empty,
        });
}