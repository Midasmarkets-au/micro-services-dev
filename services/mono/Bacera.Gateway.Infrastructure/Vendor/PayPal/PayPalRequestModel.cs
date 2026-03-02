using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.PayPal;

public class PayPalRequestModel
{
    [JsonProperty("intent")] public string Intent { get; set; } = null!;
    [JsonProperty("purchase_units")] public PurchaseUnit[] PurchaseUnits { get; set; } = null!;

    [JsonProperty("payment_source")] public PaymentSource PaymentSource { get; set; } = null!;

    public string ToJson() => JsonConvert.SerializeObject(this);
}

public sealed class PurchaseUnit
{
    [JsonProperty("reference_id")] public string ReferenceId { get; set; } = null!;
    [JsonProperty("custom_id")] public string CustomId { get; set; } = null!;
    [JsonProperty("amount")] public Amount Amount { get; set; } = null!;
}

public sealed class Amount
{
    [JsonProperty("value")] public string Value { get; set; } = null!;
    [JsonProperty("currency_code")] public string CurrencyCode { get; set; } = null!;
}

public sealed class PaymentSource
{
    [JsonProperty("paypal")] public PayPalSource Paypal { get; set; } = null!;
    // [JsonProperty("card")] public CardSource Card { get; set; } = null!;
}

public sealed class CardSource
{
    [JsonProperty("allowed_payment_methods")]
    public string[] AllowedPaymentMethods { get; set; } = ["DEBIT"];

    [JsonProperty("stored_credential")] public StoredCredential StoredCredential { get; set; } = new();
}

public sealed class StoredCredential
{
    [JsonProperty("payment_initiator")] public string PaymentInitiator { get; set; } = "MERCHANT";
    [JsonProperty("payment_type")] public string PaymentType { get; set; } = "ONE_TIME";
    [JsonProperty("usage")] public string Usage { get; set; } = "FIRST";
    [JsonProperty("return_url")] public string ReturnUrl { get; set; } = null!;
    [JsonProperty("cancel_url")] public string CancelUrl { get; set; } = null!;
}

public sealed class PayPalSource
{
    [JsonProperty("experience_context")] public ExperienceContext ExperienceContext { get; set; } = null!;
}

public sealed class ExperienceContext
{
    [JsonProperty("payment_method_preference")]
    public string PaymentMethodPreference { get; set; } = "IMMEDIATE_PAYMENT_REQUIRED";

    [JsonProperty("brand_name")] public string BrandName { get; set; } = null!;

    [JsonProperty("locale")] public string Locale { get; set; } = "en-US";

    [JsonProperty("landing_page")] public string LandingPage { get; set; } = "LOGIN";

    // [JsonProperty("shipping_preference")] public string ShippingPreference { get; set; } = "SET_PROVIDED_ADDRESS";

    [JsonProperty("user_action")] public string UserAction { get; set; } = "PAY_NOW";

    [JsonProperty("return_url")] public string ReturnUrl { get; set; } = null!;

    [JsonProperty("cancel_url")] public string CancelUrl { get; set; } = null!;
}