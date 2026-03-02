namespace Bacera.Gateway.Vendor.PayPal;

public class PayPalOptions
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string TokenEndPoint { get; set; } = null!;
    public string CheckOutEndPoint { get; set; } = null!;
    public string BrandName { get; set; } = null!;

    public static PayPalOptions FromJson(string json)
        => Utils.JsonDeserializeObjectWithDefault<PayPalOptions>(json);
}