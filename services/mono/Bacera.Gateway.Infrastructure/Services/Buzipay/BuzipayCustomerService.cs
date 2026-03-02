using System.Text;
using Bacera.Gateway.Vendor.Buzipay;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services.Buzipay;

public class BuzipayCustomerService
{
    private readonly TenantDbContext _ctx;
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _clientFactory;
    
    public BuzipayCustomerService(
        TenantDbContext ctx, 
        ILogger logger, 
        IHttpClientFactory clientFactory)
    {
        _ctx = ctx;
        _logger = logger;
        _clientFactory = clientFactory;
    }
    
    public async Task<string> GetOrCreateCustomerIdAsync(long partyId, BuzipayOptions options)
    {
        try
        {
            // Check if customer already has Buzipay ID stored in Party.Note JSON
            var party = await _ctx.Parties.FindAsync(partyId);
            if (party == null)
            {
                _logger.LogWarning("Party {PartyId} not found", partyId);
                return string.Empty;
            }
            
            var note = JsonConvert.DeserializeObject<Dictionary<string, object>>(party.Note ?? "{}") 
                ?? new Dictionary<string, object>();
                
            if (note.TryGetValue("BuzipayCustomerId", out var existingId) && existingId != null)
            {
                var customerId = existingId.ToString();
                if (!string.IsNullOrEmpty(customerId))
                {
                    _logger.LogInformation("Using existing Buzipay customer ID for party {PartyId}: {CustomerId}", 
                        partyId, customerId);
                    return customerId;
                }
            }
            
            // Create new customer via Buzipay API
            _logger.LogInformation("Creating new Buzipay customer for party {PartyId}", partyId);
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"{options.ApiUrl}/v1/customers")
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {options.SecretKey}");
            
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("Buzipay Create Customer Response: {Response}", json);
            
            var result = JsonConvert.DeserializeObject<dynamic>(json);
            var customerIdValue = result?.data?.customerId;
            var newCustomerId = customerIdValue != null ? ((object)customerIdValue).ToString() : null;
            
            if (!string.IsNullOrEmpty(newCustomerId))
            {
                // Save to Party.Note
                note["BuzipayCustomerId"] = newCustomerId;
                party.Note = JsonConvert.SerializeObject(note);
                await _ctx.SaveChangesAsync();
                
                _logger.LogInformation("Created and saved Buzipay customer ID for party {PartyId}: {CustomerId}", 
                    partyId, newCustomerId);
                return newCustomerId;
            }
            
            _logger.LogWarning("Failed to create Buzipay customer for party {PartyId}: {Response}", partyId, json);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while getting/creating Buzipay customer for party {PartyId}", partyId);
            return string.Empty;
        }
    }
}
