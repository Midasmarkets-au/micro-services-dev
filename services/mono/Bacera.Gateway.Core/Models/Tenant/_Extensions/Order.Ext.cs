using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class Order
{
    public class SupplementModal
    {
    }

    public class TenantResponseModel
    {
        public long Id { get; set; }

        public long PartyId { get; set; }
        public long ProductId { get; set; }

        public long Number { get; set; }

        public long Amount { get; set; }

        public short Status { get; set; }

        public string Recipient { get; set; } = null!;

        public string Note { get; set; } = null!;

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
        public long PartyId { get; set; }
        public long ProductId { get; set; }

        public long Number { get; set; }

        public long Amount { get; set; }

        public short Status { get; set; }

        public string Recipient { get; set; } = null!;

        public string Note { get; set; } = null!;

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

public static class OrderExtensions
{
    public static IQueryable<Order.TenantResponseModel> ToTenantResponseModels(this IQueryable<Order> query)
        => query.Select(x => new Order.TenantResponseModel
        {
            Id = x.Id,
            PartyId = x.PartyId,
            ProductId = x.ProductId,
            Number = x.Number,
            Amount = x.Amount,
            Status = x.Status,
            Recipient = x.Recipient,
            Note = x.Note,
            SupplementJson = x.Supplement ?? string.Empty,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn
        });

    public static IQueryable<Order.ResponseModel> ToResponseModels(this IQueryable<Order> query)
        => query.Select(x => new Order.ResponseModel
        {
            Number = x.Number,
            Amount = x.Amount,
            Status = x.Status,
            Recipient = x.Recipient,
            Note = x.Note,
            SupplementJson = x.Supplement ?? string.Empty,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn
        });
}