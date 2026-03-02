using Bacera.Gateway.Core.Types;
using HashidsNet;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = Address;

public partial class Address : IHasHashId
{
    public static readonly Hashids Hashids = new(HashIdSaltTypes.UserAddress, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.UserAddress]);

    public string HashId => HashEncode(Id);
    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();

    public class BaseModel
    {
        public string Name { get; set; } = "";
        public string CCC { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Country { get; set; } = "";
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public class TenantBaseModel : BaseModel
    {
        public long Id { get; set; }
        public long PartyId { get; set; }
        public DateTime? DeletedOn { get; set; }
    }

    public sealed class TenantPageModel : TenantBaseModel
    {
    }

    public sealed class TenantDetailModel : TenantBaseModel
    {
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ContentJson { get; set; } = "{}";

        public dynamic Content => Utils.JsonDeserializeDynamic(ContentJson);
    }

    public class ClientBaseModel : BaseModel
    {
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long PartyId { get; set; }

        public string HashId => HashEncode(Id);
    }

    public sealed class ClientPageModel : ClientBaseModel
    {
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ContentJson { get; set; } = "{}";

        public dynamic Content => Utils.JsonDeserializeDynamic(ContentJson);
    }

    public sealed class ClientDetailModel : ClientBaseModel
    {
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ContentJson { get; set; } = "{}";

        public dynamic Content => Utils.JsonDeserializeDynamic(ContentJson);
    }

    public sealed class CopyModel
    {
        public string Name { get; set; } = "";
        public string CCC { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Country { get; set; } = "";
        public string Content { get; set; } = "{}";
    }
}

public static class AddressModelExtension
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> q)
        => q.Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            PartyId = x.PartyId,
            Name = x.Name,
            CCC = x.CCC,
            Phone = x.Phone,
            Country = x.Country,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            DeletedOn = x.DeletedOn
        });

    public static IQueryable<M.TenantDetailModel> ToTenantDetailModel(this IQueryable<M> q)
        => q.Select(x => new M.TenantDetailModel
        {
            Id = x.Id,
            PartyId = x.PartyId,
            Name = x.Name,
            CCC = x.CCC,
            Phone = x.Phone,
            Country = x.Country,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            DeletedOn = x.DeletedOn,
            ContentJson = x.Content
        });

    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> q)
        => q.Select(x => new M.ClientPageModel
        {
            Id = x.Id,
            PartyId = x.PartyId,
            Name = x.Name,
            CCC = x.CCC,
            Phone = x.Phone,
            Country = x.Country,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            ContentJson = x.Content
        });

    public static IQueryable<M.ClientDetailModel> ToClientDetailModel(this IQueryable<M> q)
        => q.Select(x => new M.ClientDetailModel
        {
            Id = x.Id,
            PartyId = x.PartyId,
            Name = x.Name,
            CCC = x.CCC,
            Phone = x.Phone,
            Country = x.Country,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            ContentJson = x.Content
        });

    public static IQueryable<M.CopyModel> ToCopyModel(this IQueryable<M> q)
        => q.Select(x => new M.CopyModel
        {
            Name = x.Name,
            CCC = x.CCC,
            Phone = x.Phone,
            Country = x.Country,
            Content = x.Content
        });
}