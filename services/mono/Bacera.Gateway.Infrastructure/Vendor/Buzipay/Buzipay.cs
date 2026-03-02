using System.Security.Cryptography;
using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Buzipay;

public class Buzipay
{
    public class RequestClient
    {
        public string PaymentNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Description { get; set; } = "Deposit";
        public string CustomerId { get; set; } = string.Empty;
        public BuzipayOptions Options { get; set; } = new();
        public HttpClient Client { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;
        
        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            try
            {
                // Build request
                var requestBody = new CreateSessionRequest
                {
                    Description = Description,
                    Subject = "MM Deposit",
                    Currency = Currency,
                    Amount = Amount.ToString("F2"),
                    SuccessUrl = Options.SuccessUrl,
                    CancelUrl = Options.CancelUrl,
                    ClientReferenceId = PaymentNumber,
                    CustomerId = CustomerId,  // Enable card saving
                    PaymentMethodOptions = new PaymentMethodOptions
                    {
                        Card = new CardOptions
                        {
                            SetupFutureUsage = "on_session"
                        }
                    }
                };
                
                var jsonBody = JsonConvert.SerializeObject(requestBody);
                Logger.LogInformation("Buzipay.RequestAsync - Request body: {Body}", jsonBody);
                
                // Create HTTP request
                var request = new HttpRequestMessage(HttpMethod.Post, $"{Options.ApiUrl}/v1/checkout/sessions")
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("Authorization", $"Bearer {Options.SecretKey}");
                
                // Send request
                var response = await Client.SendAsync(request);
                var responseJson = await response.Content.ReadAsStringAsync();
                Logger.LogInformation("Buzipay.RequestAsync - Response: {Response}", responseJson);
                
                // Parse response
                var result = JsonConvert.DeserializeObject<CreateSessionResponse>(responseJson);
                
                if (result?.Code != "success" || string.IsNullOrEmpty(result?.Data?.Url))
                {
                    return DepositCreatedResponseModel.Fail($"Buzipay API error: {result?.Msg ?? "Unknown error"}");
                }
                
                // Return success with redirect URL
                return new DepositCreatedResponseModel
                {
                    IsSuccess = true,
                    Action = PaymentResponseActionTypes.Redirect,
                    Message = result.Data.SessionId,
                    RedirectUrl = result.Data.Url,
                    PaymentNumber = PaymentNumber  // Must set PaymentNumber for payment creation
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Buzipay.RequestAsync - Exception");
                return DepositCreatedResponseModel.Fail($"Buzipay request failed: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Validate webhook signature using HMAC-SHA256
    /// </summary>
    public static bool ValidateWebhookSignature(
        string timestamp, 
        string receivedSignature, 
        string rawBody, 
        string webhookSecretKey,
        ILogger? logger = null)
    {
        try
        {
            // Create signed payload: timestamp + "." + body
            var signedPayload = $"{timestamp}.{rawBody}";
            
            // Compute HMAC-SHA256
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
            var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            
            // Secure comparison
            var isValid = computedSignature.Equals(receivedSignature, StringComparison.OrdinalIgnoreCase);
            
            logger?.LogInformation("Buzipay signature validation: {Result}. Computed: {Computed}, Received: {Received}",
                isValid ? "Valid" : "Invalid", computedSignature, receivedSignature);
            
            return isValid;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Buzipay signature validation failed");
            return false;
        }
    }
    
    /// <summary>
    /// Retrieve session status for verification
    /// </summary>
    public static async Task<RetrieveSessionResponse?> RetrieveSessionAsync(
        string sessionId,
        string secretKey,
        string apiUrl,
        HttpClient client,
        ILogger? logger = null)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}/v1/checkout/sessions/{sessionId}");
            request.Headers.Add("Authorization", $"Bearer {secretKey}");
            
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            
            logger?.LogInformation("Buzipay.RetrieveSessionAsync - Response: {Json}", json);
            
            return JsonConvert.DeserializeObject<RetrieveSessionResponse>(json);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Buzipay.RetrieveSessionAsync - Exception");
            return null;
        }
    }
    
    // Request Models
    public class CreateSessionRequest
    {
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonProperty("subject")]
        public string Subject { get; set; } = string.Empty;
        
        [JsonProperty("currency")]
        public string Currency { get; set; } = string.Empty;
        
        [JsonProperty("amount")]
        public string Amount { get; set; } = string.Empty;
        
        [JsonProperty("successUrl")]
        public string SuccessUrl { get; set; } = string.Empty;
        
        [JsonProperty("cancelUrl")]
        public string CancelUrl { get; set; } = string.Empty;
        
        [JsonProperty("clientReferenceId")]
        public string ClientReferenceId { get; set; } = string.Empty;
        
        [JsonProperty("customerId")]
        public string? CustomerId { get; set; }
        
        [JsonProperty("paymentMethodOptions")]
        public PaymentMethodOptions? PaymentMethodOptions { get; set; }
    }
    
    public class PaymentMethodOptions
    {
        [JsonProperty("card")]
        public CardOptions Card { get; set; } = new();
    }
    
    public class CardOptions
    {
        [JsonProperty("setupFutureUsage")]
        public string SetupFutureUsage { get; set; } = "on_session";
    }
    
    // Response Models
    public class CreateSessionResponse
    {
        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;
        
        [JsonProperty("msg")]
        public string? Msg { get; set; }
        
        [JsonProperty("data")]
        public SessionData? Data { get; set; }
    }
    
    public class SessionData
    {
        [JsonProperty("sessionId")]
        public string SessionId { get; set; } = string.Empty;
        
        [JsonProperty("paymentIntentId")]
        public string PaymentIntentId { get; set; } = string.Empty;
        
        [JsonProperty("clientReferenceId")]
        public string ClientReferenceId { get; set; } = string.Empty;
        
        [JsonProperty("expiresAt")]
        public long ExpiresAt { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;
    }
    
    public class RetrieveSessionResponse
    {
        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;
        
        [JsonProperty("msg")]
        public string? Msg { get; set; }
        
        [JsonProperty("data")]
        public SessionDetailData? Data { get; set; }
    }
    
    public class SessionDetailData
    {
        [JsonProperty("sessionId")]
        public string SessionId { get; set; } = string.Empty;
        
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;  // "expired", "complete", "open"
        
        [JsonProperty("paymentIntentId")]
        public string PaymentIntentId { get; set; } = string.Empty;
        
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        
        [JsonProperty("currency")]
        public string Currency { get; set; } = string.Empty;
    }
}
