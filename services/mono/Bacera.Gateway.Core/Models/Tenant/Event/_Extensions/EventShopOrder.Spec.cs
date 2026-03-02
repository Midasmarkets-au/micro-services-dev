using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = EventShopOrder;

public partial class EventShopOrder
{
    public sealed class CreateSpec
    {
        public string ShopItemHashId { get; set; } = string.Empty;
        public string AddressHashId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public sealed class UpdateShippingSpec
    {
        public string TrackingNumber { get; set; } = string.Empty;
    }

    public sealed class ShippingModel
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public Address.CopyModel Address { get; set; } = new();
        public static ShippingModel Build() => new();

        public ShippingModel SetTrackingNumber(string trackingNumber)
        {
            TrackingNumber = trackingNumber;
            return this;
        }

        public ShippingModel SetAddress(Address.CopyModel address)
        {
            Address = address;
            return this;
        }

        public static ShippingModel FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ShippingModel>(json)!;
        }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}