using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class Product
{
    public class SupplementModal
    {
        public string ImageLink { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class TenantResponseModel
    {
        public long Id { get; set; }

        public ProductTypes Type { get; set; }

        public ProductStatusType Status { get; set; }

        public string Name { get; set; } = string.Empty;

        public long Point { get; set; }

        public long Total { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string SupplementJson { get; set; } = string.Empty;

        public SupplementModal Supplement
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<SupplementModal>(SupplementJson) ??
                           new SupplementModal();
                }
                catch
                {
                    return new SupplementModal();
                }
            }
        }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }

    public class ResponseModel
    {
        public ProductTypes Type { get; set; }

        public ProductStatusType Status { get; set; }

        public string Name { get; set; } = string.Empty;

        public long Point { get; set; }

        public long Total { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string SupplementJson { get; set; } = string.Empty;

        public SupplementModal Supplement
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<SupplementModal>(SupplementJson) ??
                           new SupplementModal();
                }
                catch
                {
                    return new SupplementModal();
                }
            }
        }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}

public static class ProductExtensions
{
    public static IQueryable<Product.TenantResponseModel> ToTenantResponseModel(this IQueryable<Product> query)
        => query.Select(x => new Product.TenantResponseModel
        {
            Id = x.Id,
            Name = x.Name,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            Type = (ProductTypes)x.Type,
            Status = (ProductStatusType)x.Status,
            Point = x.Point,
            Total = x.Total,
            SupplementJson = x.Supplement ?? string.Empty
        });

    public static IQueryable<Product.ResponseModel> ToResponseModel(this IQueryable<Product> query)
        => query.Select(x => new Product.ResponseModel
        {
            Name = x.Name,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            Type = (ProductTypes)x.Type,
            Status = (ProductStatusType)x.Status,
            Point = x.Point,
            Total = x.Total,
            SupplementJson = x.Supplement ?? string.Empty
        });
}