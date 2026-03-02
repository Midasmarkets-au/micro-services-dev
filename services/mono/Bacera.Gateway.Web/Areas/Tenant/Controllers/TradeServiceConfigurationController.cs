using OpenIddict.Validation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = TradeService;

/// <summary>
/// TradeService Configuration Management Controller
/// Manages MT5 Demo Account Prefixes and other TradeService configurations
/// </summary>
[Tags("Tenant/TradeService/Configuration")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class TradeServiceConfigurationController : TenantBaseController
{
    private readonly TenantDbContext _tenantDbContext;

    public TradeServiceConfigurationController(TenantDbContext tenantDbContext)
    {
        _tenantDbContext = tenantDbContext;
    }

    /// <summary>
    /// Get TradeService Configuration (including MT5 Demo Account Prefixes)
    /// </summary>
    /// <param name="serviceId">TradeService ID (e.g., 31 for MM-MT5)</param>
    /// <returns>Full TradeService configuration in JSON format</returns>
    /// <response code="200">Returns the TradeService configuration</response>
    /// <response code="404">TradeService not found</response>
    [HttpGet("{serviceId:int}/configuration")]
    [ProducesResponseType(typeof(M.ConfigurationModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConfiguration(int serviceId)
    {
        var tradeService = await _tenantDbContext.TradeServices
            .Where(x => x.Id == serviceId)
            .FirstOrDefaultAsync();

        if (tradeService == null)
        {
            return NotFound(new { error = "TradeService not found", serviceId });
        }

        try
        {
            // Deserialize the Configuration JSON string to the model
            var configuration = JsonConvert.DeserializeObject<M.ConfigurationModel>(
                tradeService.Configuration
            );

            return Ok(configuration);
        }
        catch (JsonException ex)
        {
            return StatusCode(500, new { 
                error = "Failed to parse configuration", 
                detail = ex.Message 
            });
        }
    }

    /// <summary>
    /// Get only MT5 Demo Account Prefixes (simplified response)
    /// </summary>
    /// <param name="serviceId">TradeService ID (e.g., 31 for MM-MT5)</param>
    /// <returns>Account prefixes dictionary</returns>
    /// <response code="200">Returns the account prefixes</response>
    /// <response code="404">TradeService not found or no account prefixes configured</response>
    [HttpGet("{serviceId:int}/account-prefixes")]
    [ProducesResponseType(typeof(M.AccountPrefixesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountPrefixes(int serviceId)
    {
        var tradeService = await _tenantDbContext.TradeServices
            .Where(x => x.Id == serviceId)
            .FirstOrDefaultAsync();

        if (tradeService == null)
        {
            return NotFound(new { error = "TradeService not found", serviceId });
        }

        try
        {
            var configuration = JsonConvert.DeserializeObject<M.ConfigurationModel>(
                tradeService.Configuration
            );

            if (configuration?.AccountPrefixes == null)
            {
                return NotFound(new { error = "No account prefixes configured", serviceId });
            }

            var response = new M.AccountPrefixesResponse
            {
                ServiceId = tradeService.Id,
                ServiceName = tradeService.Name,
                AccountPrefixes = configuration.AccountPrefixes
            };

            return Ok(response);
        }
        catch (JsonException ex)
        {
            return StatusCode(500, new { 
                error = "Failed to parse configuration", 
                detail = ex.Message 
            });
        }
    }

    /// <summary>
    /// Update MT5 Demo Account Prefixes
    /// </summary>
    /// <param name="serviceId">TradeService ID (e.g., 31 for MM-MT5)</param>
    /// <param name="accountPrefixes">Dictionary of currency codes to prefix numbers</param>
    /// <returns>Updated account prefixes</returns>
    /// <response code="200">Account prefixes updated successfully</response>
    /// <response code="400">Invalid request format</response>
    /// <response code="404">TradeService not found</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/tenant/tradeservice-configuration/31/account-prefixes
    ///     {
    ///         "USD": 32000000,
    ///         "USC": 33000000,
    ///         "AUD": 36000000,
    ///         "EUR": 37000000,
    ///         "GBP": 34000000,
    ///         "DEFAULT": 36000000
    ///     }
    /// 
    /// </remarks>
    [HttpPut("{serviceId:int}/account-prefixes")]
    [ProducesResponseType(typeof(M.AccountPrefixesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAccountPrefixes(
        int serviceId,
        [FromBody] Dictionary<string, long> accountPrefixes)
    {
        if (accountPrefixes == null || accountPrefixes.Count == 0)
        {
            return BadRequest(new { error = "Account prefixes cannot be empty" });
        }

        var tradeService = await _tenantDbContext.TradeServices
            .Where(x => x.Id == serviceId)
            .FirstOrDefaultAsync();

        if (tradeService == null)
        {
            return NotFound(new { error = "TradeService not found", serviceId });
        }

        try
        {
            // Deserialize existing configuration
            var configuration = JsonConvert.DeserializeObject<M.ConfigurationModel>(
                tradeService.Configuration
            );

            if (configuration == null)
            {
                return StatusCode(500, new { error = "Failed to parse existing configuration" });
            }

            // Update only the AccountPrefixes
            configuration.AccountPrefixes = accountPrefixes;

            // Serialize back to JSON
            var configurationJson = JsonConvert.SerializeObject(configuration, Formatting.None);
            tradeService.Configuration = configurationJson;
            tradeService.UpdatedOn = DateTime.UtcNow;

            await _tenantDbContext.SaveChangesAsync();

            var response = new M.AccountPrefixesResponse
            {
                ServiceId = tradeService.Id,
                ServiceName = tradeService.Name,
                AccountPrefixes = accountPrefixes
            };

            return Ok(response);
        }
        catch (JsonException ex)
        {
            return BadRequest(new { 
                error = "Invalid configuration format", 
                detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                error = "Failed to update account prefixes", 
                detail = ex.Message 
            });
        }
    }
}

