using Newtonsoft.Json;
using M = Bacera.Gateway.Symbol;
namespace Bacera.Gateway;

partial class Symbol
{
    public class ResponseModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Category { get; set; } = null!;
        public int CategoryId { get; set; }
        public int Type { get; set; }
        public DateTime CreatedOn { get; set; }
        public long OperatorPartyId { get; set; }
    }

    public class CreateOrUpdateModel
    {
        public string Code { get; set; } = null!;
        public string Category { get; set; } = null!;
        public int CategoryId { get; set; }
        public int Type { get; set; }
    }

    public class BatchImportModel
    {
        public string[] Codes { get; set; } = null!;
        public string Category { get; set; } = null!;
        public int CategoryId { get; set; }
        public int Type { get; set; }
    }

    public class CreateCategoryModel
    {
        public string? Code { get; set; }
        public string Category { get; set; } = null!;
        public int Type { get; set; }
    }

    public class DeleteMultipleModel
    {
        public int[] Ids { get; set; } = null!;
    }

    public class UpdateCategoryModel
    {
        public string Category { get; set; } = null!;
        public string? Code { get; set; }
        public int Type { get; set; }
    }

    public ResponseModel ToResponseModel()
    {
        return new ResponseModel
        {
            Id = Id,
            Code = Code,
            Category = Category,
            CategoryId = CategoryId,
            Type = Type,
            CreatedOn = CreatedOn,
            OperatorPartyId = OperatorPartyId
        };
    }
}
public static class SymbolExtension
{
    public static IQueryable<RebateSymbol> ToRebateSymbols(this IQueryable<M> q, int type) =>
        q.Where(x => x.Type == type).Select(x => new RebateSymbol
        {
            Code = x.Code,
            Category = x.Category,
            CategoryId = x.CategoryId
        });

    public static IQueryable<KeyValuePair<int, string>> ToRebateCategories(this IQueryable<M> q, int type) =>
        q.Where(x => x.Type == type)
            .GroupBy(x => new { x.CategoryId, x.Category })
            .OrderBy(x => x.Key.CategoryId)
            .Select(x => new KeyValuePair<int, string>(x.Key.CategoryId, x.Key.Category));
}