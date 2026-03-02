using Newtonsoft.Json;
using System;

namespace Bacera.Gateway.Vendor.MonetixPay;

public class MonetixPayCallbackResponse
{
    public sealed class Customer
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;
    }

    public sealed class Sum
    {
        [JsonProperty("amount")] public int Amount { get; set; }

        [JsonProperty("currency")] public string Currency { get; set; } = string.Empty;
    }

    public sealed class Payment
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;

        [JsonProperty("type")] public string Type { get; set; } = string.Empty;

        [JsonProperty("status")] public string Status { get; set; } = string.Empty;

        [JsonProperty("date")] public DateTime Date { get; set; } = DateTime.MinValue;

        [JsonProperty("method")] public string Method { get; set; } = string.Empty;

        [JsonProperty("sum")] public Sum Sum { get; set; } = new();

        [JsonProperty("description")] public string Description { get; set; } = string.Empty;
    }

    public sealed class SumInitial
    {
        [JsonProperty("amount")] public long Amount { get; set; }

        [JsonProperty("currency")] public string Currency { get; set; } = string.Empty;
    }

    public sealed class SumConverted
    {
        [JsonProperty("amount")] public long Amount { get; set; }

        [JsonProperty("currency")] public string Currency { get; set; } = string.Empty;
    }

    public sealed class Provider
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("payment_id")] public string PaymentId { get; set; } = string.Empty;

        [JsonProperty("auth_code")] public string AuthCode { get; set; } = string.Empty;
    }

    public sealed class Operation
    {
        [JsonProperty("sum_initial")] public SumInitial SumInitial { get; set; } = new();

        [JsonProperty("sum_converted")] public SumConverted SumConverted { get; set; } = new();

        [JsonProperty("code")] public string Code { get; set; } = string.Empty;

        [JsonProperty("message")] public string Message { get; set; } = string.Empty;

        [JsonProperty("provider")] public Provider Provider { get; set; } = new();

        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("type")] public string Type { get; set; } = string.Empty;

        [JsonProperty("status")] public string Status { get; set; } = string.Empty;

        [JsonProperty("date")] public DateTime Date { get; set; } = DateTime.MinValue;

        [JsonProperty("created_date")] public DateTime CreatedDate { get; set; } = DateTime.MinValue;

        [JsonProperty("request_id")] public string RequestId { get; set; } = string.Empty;
    }

    public sealed class Root
    {
        [JsonProperty("customer")] public Customer Customer { get; set; } = new();

        [JsonProperty("project_id")] public int ProjectId { get; set; }

        [JsonProperty("payment")] public Payment Payment { get; set; } = new();

        [JsonProperty("operation")] public Operation Operation { get; set; } = new();

        [JsonProperty("signature")] public string Signature { get; set; } = string.Empty;

        public static Root FromJson(string json) => JsonConvert.DeserializeObject<Root>(json, new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.None
        })!;
    }
}