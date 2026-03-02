using Bacera.Gateway.Core.Types;
using HashidsNet;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = EventShopOrder;

public partial class EventShopOrder : IHasHashId
{
    public static readonly Hashids Hashids = new(HashIdSaltTypes.EventShopOrder, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.EventShopOrder]);

    public string HashId => HashEncode(Id);
    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();

    public class BaseModel
    {
        public string EventShopItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public long TotalPoint { get; set; }
        public EventShopOrderStatusTypes Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public class TenantBaseModel : BaseModel
    {
        public long Id { get; set; }
        public long EventPartyId { get; set; }
        public string EventPartyNativeName { get; set; } = string.Empty;
        public string EventPartyEmail { get; set; } = string.Empty;
        public long EventShopItemId { get; set; }

        public long AddressId { get; set; }

        public long OperatorPartyId { get; set; }
        public string OperatorName { get; set; } = string.Empty;
    }

    public class TenantPageModel : TenantBaseModel
    {
    }

    public class TenantDetailModel : TenantBaseModel
    {
        public Address.TenantDetailModel Address { get; set; } = new();
        public string Comment { get; set; } = string.Empty;

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ShippingJson { get; set; } = string.Empty;

        public ShippingModel Shipping => Utils.JsonDeserializeObjectWithDefault<ShippingModel>(ShippingJson);
    }

    public class ClientBaseModel : BaseModel
    {
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; set; }

        public string HashId => HashEncode(Id);
    }

    public sealed class ClientPageModel : ClientBaseModel
    {
    }

    public sealed class ClientDetailModel : ClientBaseModel
    {
        public Address.ClientDetailModel Address { get; set; } = new();
        public string Comment { get; set; } = string.Empty;

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ShippingJson { get; set; } = string.Empty;

        public ShippingModel Shipping => Utils.JsonDeserializeObjectWithDefault<ShippingModel>(ShippingJson);
    }
}

public static class EventShopOrderViewModelExtension
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> q, string lang)
        => q.Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            EventPartyId = x.EventPartyId,
            EventPartyNativeName = x.EventParty.Party.NativeName,
            EventPartyEmail = x.EventParty.Party.Email,
            EventShopItemId = x.EventShopItemId,
            EventShopItemName = (x.EventShopItem.EventShopItemLanguages
                                    .Where(y => y.Language == lang)
                                    .Select(y => y.Name)
                                    .FirstOrDefault() 
                                ?? x.EventShopItem.EventShopItemLanguages
                                    .Where(y => y.Language == "en-us")
                                    .Select(y => y.Name)
                                    .FirstOrDefault()) 
                               ?? "",
            Quantity = x.Quantity,
            TotalPoint = x.TotalPoint,
            AddressId = x.AddressId,
            Status = (EventShopOrderStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            OperatorPartyId = x.OperatorPartyId,
            OperatorName = x.OperatorParty.NativeName
        });

    public static IQueryable<M.TenantDetailModel> ToTenantDetailModel(this IQueryable<M> q, string lang)
        => q.Select(x => new M.TenantDetailModel
        {
            Id = x.Id,
            EventPartyId = x.EventPartyId,
            EventPartyNativeName = x.EventParty.Party.NativeName,
            EventPartyEmail = x.EventParty.Party.Email,
            EventShopItemId = x.EventShopItemId,
            EventShopItemName = (x.EventShopItem.EventShopItemLanguages
                                     .Where(y => y.Language == lang)
                                     .Select(y => y.Name)
                                     .FirstOrDefault() 
                                 ?? x.EventShopItem.EventShopItemLanguages
                                     .Where(y => y.Language == "en-us")
                                     .Select(y => y.Name)
                                     .FirstOrDefault()) 
                                ?? "",
            Quantity = x.Quantity,
            TotalPoint = x.TotalPoint,
            Comment = x.Comment,
            AddressId = x.AddressId,
            Status = (EventShopOrderStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            OperatorPartyId = x.OperatorPartyId,
            OperatorName = x.OperatorParty.NativeName,
            ShippingJson = x.Shipping,
            Address = new Address.TenantDetailModel
            {
                Id = x.Address.Id,
                PartyId = x.Address.PartyId,
                Name = x.Address.Name,
                CCC = x.Address.CCC,
                Phone = x.Address.Phone,
                Country = x.Address.Country,
                CreatedOn = x.Address.CreatedOn,
                UpdatedOn = x.Address.UpdatedOn,
                DeletedOn = x.Address.DeletedOn,
                ContentJson = x.Address.Content
            },
        });

    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> q, string lang)
        => q.Select(x => new M.ClientPageModel
        {
            Id = x.Id,
            EventShopItemName = (x.EventShopItem.EventShopItemLanguages
                                     .Where(y => y.Language == lang)
                                     .Select(y => y.Name)
                                     .FirstOrDefault() 
                                 ?? x.EventShopItem.EventShopItemLanguages
                                     .Where(y => y.Language == "en-us")
                                     .Select(y => y.Name)
                                     .FirstOrDefault()) 
                                ?? "",
            Quantity = x.Quantity,
            TotalPoint = x.TotalPoint,
            Status = (EventShopOrderStatusTypes)x.Status,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn
        });

    public static IQueryable<M.ClientDetailModel> ToClientDetailModel(this IQueryable<M> q, string lang)
        => q.Select(x => new M.ClientDetailModel
        {
            Id = x.Id,
            EventShopItemName = (x.EventShopItem.EventShopItemLanguages
                                     .Where(y => y.Language == lang)
                                     .Select(y => y.Name)
                                     .FirstOrDefault() 
                                 ?? x.EventShopItem.EventShopItemLanguages
                                     .Where(y => y.Language == "en-us")
                                     .Select(y => y.Name)
                                     .FirstOrDefault()) 
                                ?? "",
            Quantity = x.Quantity,
            TotalPoint = x.TotalPoint,
            Status = (EventShopOrderStatusTypes)x.Status,
            Comment = x.Comment,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            ShippingJson = x.Shipping,
            Address = new Address.ClientDetailModel
            {
                Id = x.Address.Id,
                PartyId = x.Address.PartyId,
                Name = x.Address.Name,
                CCC = x.Address.CCC,
                Phone = x.Address.Phone,
                Country = x.Address.Country,
                CreatedOn = x.Address.CreatedOn,
                UpdatedOn = x.Address.UpdatedOn,
                ContentJson = x.Address.Content
            },
        });
}