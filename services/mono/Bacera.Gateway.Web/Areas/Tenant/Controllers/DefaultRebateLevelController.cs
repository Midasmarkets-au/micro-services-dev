using OpenIddict.Validation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

/// <summary>
/// Default Rebate Level Setting Controller (客户基础返佣表)
/// </summary>
[Tags("Tenant/DefaultRebateLevel")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class DefaultRebateLevelController(
    TenantDbContext tenantDbContext,
    ConfigurationService configurationService,
    ConfigurationSnapshotService snapshotService) : TenantBaseController
{
    private const string ConfigName = "DefaultRebateLevelSetting";
    private const long ConfigRowId = 0;

    /// <summary>
    /// Get all default rebate level settings
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get()
    {
        var config = await tenantDbContext.Configurations
            .Where(x => x.Name.ToLower().Contains(ConfigName.ToLower()) && x.RowId == ConfigRowId)
            .FirstOrDefaultAsync();

        if (config == null)
        {
            return NotFound(new { error = "Default Rebate Level Setting configuration not found." });
        }

        try
        {
            // Parse the JSON value
            var jsonData = JObject.Parse(config.Value);
            var response = new DefaultRebateLevelSetting.ResponseModel();

            // Get category mappings from Symbol table
            var categoryMappings = await tenantDbContext.Symbols
                .Where(x => x.Type == 300)
                .GroupBy(x => new { x.CategoryId, x.Category })
                .Select(g => new { CategoryId = g.Key.CategoryId, CategoryName = g.Key.Category })
                .OrderBy(x => x.CategoryId)
                .ToListAsync();

            var categoryNameMap = categoryMappings.ToDictionary(x => x.CategoryId, x => x.CategoryName);

            // Process each account type
            foreach (var accountTypeProp in jsonData.Properties())
            {
                var accountTypeId = int.Parse(accountTypeProp.Name);
                var accountTypeName = ((AccountTypes)accountTypeId).ToString();
                var optionsArray = accountTypeProp.Value as JArray;

                if (optionsArray == null) continue;

                var accountTypeRebate = new DefaultRebateLevelSetting.AccountTypeRebate
                {
                    AccountTypeId = accountTypeId,
                    AccountTypeName = accountTypeName,
                    Options = new List<DefaultRebateLevelSetting.RebateOption>()
                };

                foreach (var optionToken in optionsArray)
                {
                    var option = optionToken as JObject;
                    if (option == null) continue;

                    var rebateOption = new DefaultRebateLevelSetting.RebateOption
                    {
                        OptionName = option["OptionName"]?.ToString() ?? 
                                     option["optionName"]?.ToString() ?? 
                                     "",
                        Category = new Dictionary<string, double>(),
                        AllowPipOptions = new List<int>(),
                        AllowPipSetting = new Dictionary<string, DefaultRebateLevelSetting.AllowPipOrCommissionItem>(),
                        AllowCommissionOptions = new List<int>(),
                        AllowCommissionSetting = new Dictionary<string, DefaultRebateLevelSetting.AllowPipOrCommissionItem>()
                    };

                    // Parse Category
                    var categoryObj = option["Category"] ?? option["category"];
                    if (categoryObj is JObject catObj)
                    {
                        foreach (var catProp in catObj.Properties())
                        {
                            rebateOption.Category[catProp.Name] = catProp.Value.ToObject<double>();
                        }
                    }

                    // Parse AllowPipOptions
                    var allowPipOptions = option["AllowPipOptions"];
                    if (allowPipOptions is JArray pipArray)
                    {
                        rebateOption.AllowPipOptions = pipArray.ToObject<List<int>>() ?? new List<int>();
                    }

                    // Parse AllowPipSetting
                    var allowPipSetting = option["AllowPipSetting"];
                    if (allowPipSetting is JObject pipSettingObj)
                    {
                        foreach (var pipProp in pipSettingObj.Properties())
                        {
                            var pipItem = pipProp.Value.ToObject<DefaultRebateLevelSetting.AllowPipOrCommissionItem>();
                            if (pipItem != null)
                            {
                                rebateOption.AllowPipSetting[pipProp.Name] = pipItem;
                            }
                        }
                    }

                    // Parse AllowCommissionOptions
                    var allowCommissionOptions = option["AllowCommissionOptions"];
                    if (allowCommissionOptions is JArray commArray)
                    {
                        rebateOption.AllowCommissionOptions = commArray.ToObject<List<int>>() ?? new List<int>();
                    }

                    // Parse AllowCommissionSetting
                    var allowCommissionSetting = option["AllowCommissionSetting"];
                    if (allowCommissionSetting is JObject commSettingObj)
                    {
                        foreach (var commProp in commSettingObj.Properties())
                        {
                            var commItem = commProp.Value.ToObject<DefaultRebateLevelSetting.AllowPipOrCommissionItem>();
                            if (commItem != null)
                            {
                                rebateOption.AllowCommissionSetting[commProp.Name] = commItem;
                            }
                        }
                    }

                    accountTypeRebate.Options.Add(rebateOption);
                }

                response.AccountTypes.Add(accountTypeRebate);
            }

            return Ok(new
            {
                AccountTypes = response.AccountTypes,
                CategoryNameMap = categoryNameMap
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to parse configuration data.", details = ex.Message });
        }
    }

    /// <summary>
    /// Get detailed rebate settings for a specific account type and option
    /// </summary>
    /// <param name="accountTypeId">Account Type ID</param>
    /// <param name="optionName">Option Name (e.g., "Standard", "Alpha")</param>
    [HttpGet("{accountTypeId:int}/{optionName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int accountTypeId, string optionName)
    {
        var config = await tenantDbContext.Configurations
            .Where(x => x.Name.ToLower().Contains(ConfigName.ToLower()) && x.RowId == ConfigRowId)
            .FirstOrDefaultAsync();

        if (config == null)
        {
            return NotFound(new { error = "Default Rebate Level Setting configuration not found." });
        }

        try
        {
            var jsonData = JObject.Parse(config.Value);
            var accountTypeKey = accountTypeId.ToString();

            if (!jsonData.ContainsKey(accountTypeKey))
            {
                return NotFound(new { error = $"Account type {accountTypeId} not found in configuration." });
            }

            var optionsArray = jsonData[accountTypeKey] as JArray;
            if (optionsArray == null)
            {
                return NotFound(new { error = "Invalid options data." });
            }

            // Find the specific option
            JObject? targetOption = null;
            foreach (var optionToken in optionsArray)
            {
                var option = optionToken as JObject;
                if (option == null) continue;

                var currentOptionName = option["OptionName"]?.ToString() ?? option["optionName"]?.ToString();
                if (currentOptionName?.Equals(optionName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    targetOption = option;
                    break;
                }
            }

            if (targetOption == null)
            {
                return NotFound(new { error = $"Option '{optionName}' not found for account type {accountTypeId}." });
            }

            // Get category mappings
            var categoryMappings = await tenantDbContext.Symbols
                .Where(x => x.Type == 300)
                .GroupBy(x => new { x.CategoryId, x.Category })
                .Select(g => new { CategoryId = g.Key.CategoryId, CategoryName = g.Key.Category })
                .OrderBy(x => x.CategoryId)
                .ToDictionaryAsync(x => x.CategoryId, x => x.CategoryName);

            var response = new DefaultRebateLevelSetting.DetailResponseModel
            {
                AccountTypeId = accountTypeId,
                AccountTypeName = ((AccountTypes)accountTypeId).ToString(),
                OptionName = targetOption["OptionName"]?.ToString() ?? 
                             targetOption["optionName"]?.ToString() ?? 
                             "",
                Categories = new List<DefaultRebateLevelSetting.RebateCategoryInfo>(),
                AllowPipOptions = new List<int>(),
                AllowPipSetting = new Dictionary<string, DefaultRebateLevelSetting.AllowPipOrCommissionItem>(),
                AllowCommissionOptions = new List<int>(),
                AllowCommissionSetting = new Dictionary<string, DefaultRebateLevelSetting.AllowPipOrCommissionItem>()
            };

            // Parse Category with names
            var categoryObj = targetOption["Category"] ?? targetOption["category"];
            if (categoryObj is JObject catObj)
            {
                foreach (var catProp in catObj.Properties())
                {
                    var categoryId = int.Parse(catProp.Name);
                    var value = catProp.Value.ToObject<double>();
                    var categoryName = categoryMappings.TryGetValue(categoryId, out var name) 
                        ? name 
                        : $"Unknown ({categoryId})";

                    response.Categories.Add(new DefaultRebateLevelSetting.RebateCategoryInfo
                    {
                        CategoryId = categoryId,
                        CategoryName = categoryName,
                        Value = value
                    });
                }
            }

            // Parse AllowPipOptions
            var allowPipOptions = targetOption["AllowPipOptions"];
            if (allowPipOptions is JArray pipArray)
            {
                response.AllowPipOptions = pipArray.ToObject<List<int>>() ?? new List<int>();
            }

            // Parse AllowPipSetting
            var allowPipSetting = targetOption["AllowPipSetting"];
            if (allowPipSetting is JObject pipSettingObj)
            {
                foreach (var pipProp in pipSettingObj.Properties())
                {
                    var pipItem = pipProp.Value.ToObject<DefaultRebateLevelSetting.AllowPipOrCommissionItem>();
                    if (pipItem != null)
                    {
                        response.AllowPipSetting[pipProp.Name] = pipItem;
                    }
                }
            }

            // Parse AllowCommissionOptions
            var allowCommissionOptions = targetOption["AllowCommissionOptions"];
            if (allowCommissionOptions is JArray commArray)
            {
                response.AllowCommissionOptions = commArray.ToObject<List<int>>() ?? new List<int>();
            }

            // Parse AllowCommissionSetting
            var allowCommissionSetting = targetOption["AllowCommissionSetting"];
            if (allowCommissionSetting is JObject commSettingObj)
            {
                foreach (var commProp in commSettingObj.Properties())
                {
                    var commItem = commProp.Value.ToObject<DefaultRebateLevelSetting.AllowPipOrCommissionItem>();
                    if (commItem != null)
                    {
                        response.AllowCommissionSetting[commProp.Name] = commItem;
                    }
                }
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to parse configuration data.", details = ex.Message });
        }
    }

    /// <summary>
    /// Update the entire default rebate level settings
    /// Automatically saves snapshot for reference 
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromBody] DefaultRebateLevelSetting.UpdateModel model)
    {
        var config = await tenantDbContext.Configurations
            .Where(x => x.Name.ToLower().Contains(ConfigName.ToLower()) && x.RowId == ConfigRowId)
            .FirstOrDefaultAsync();

        if (config == null)
        {
            return NotFound(new { error = "Default Rebate Level Setting configuration not found." });
        }

        try
        {
            // Validate the model
            if (model.AccountTypes == null || model.AccountTypes.Count == 0)
            {
                return BadRequest(new { error = "AccountTypes is required and cannot be empty." });
            }

            var userId = GetPartyId();

            // Convert model to JSON
            var jsonString = ConvertModelToJsonString(model);
            
            // Always save snapshot with user's updated data (for reference/housekeeping)
            // This allows tracking what was saved and when, useful for auditing/debugging
            var snapshotVersion = DateTime.UtcNow;
            await snapshotService.CreateSnapshotAsync(
                userId, ConfigName, ConfigRowId, snapshotVersion, jsonString);

            // Validate JSON can be parsed back
            try
            {
                JObject.Parse(jsonString);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = "Generated JSON is invalid.",
                    details = ex.Message
                });
            }

            config.Value = jsonString;
            config.UpdatedOn = DateTime.UtcNow;
            config.UpdatedBy = userId;

            await tenantDbContext.SaveChangesAsync();

            // Refresh cache to make changes take effect immediately
            await configurationService.ResetCacheAsync();

            return Ok(new
            {
                success = true,
                message = "Default Rebate Level Setting updated successfully.",
                updatedOn = config.UpdatedOn
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to update configuration.", details = ex.Message });
        }
    }

    /// <summary>
    /// Convert UpdateModel to JSON string
    /// </summary>
    private string ConvertModelToJsonString(DefaultRebateLevelSetting.UpdateModel model)
    {
        var jsonData = new Dictionary<string, List<object>>();

        foreach (var accountType in model.AccountTypes)
        {
            var accountTypeKey = accountType.AccountTypeId.ToString();
            var options = new List<object>();

            foreach (var option in accountType.Options)
            {
                var categoryDict = new Dictionary<string, double>();
                foreach (var cat in option.Category)
                {
                    categoryDict[cat.Key] = cat.Value;
                }

                var optionData = new
                {
                    OptionName = option.OptionName,
                    Category = categoryDict,
                    AllowPipOptions = option.AllowPipOptions,
                    AllowPipSetting = option.AllowPipSetting,
                    AllowCommissionOptions = option.AllowCommissionOptions,
                    AllowCommissionSetting = option.AllowCommissionSetting
                };

                options.Add(optionData);
            }

            jsonData[accountTypeKey] = options;
        }

        return JsonConvert.SerializeObject(jsonData, Formatting.Indented);
    }

    /// <summary>
    /// Get all available categories from Symbol table
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await tenantDbContext.Symbols
            .Where(x => x.Type == 300)
            .GroupBy(x => new { x.CategoryId, x.Category })
            .Select(g => new
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Category
            })
            .OrderBy(x => x.CategoryId)
            .ToListAsync();

        return Ok(categories);
    }
}

