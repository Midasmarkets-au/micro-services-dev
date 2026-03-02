using OpenIddict.Validation.AspNetCore;
﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using M = Bacera.Gateway.Symbol;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Tags("Tenant/Symbol")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class SymbolController(
    TenantDbContext tenantDbContext,
    ConfigurationService configurationService) : TenantBaseController
{
    private string[] targetOptionNames = new[] { "Standard", "alpha" };

    /// <summary>
    /// Symbol pagination
    /// </summary>
    /// <param name="criteria">Search criteria</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<List<M>, M.Criteria>))]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var result = await tenantDbContext.Symbols
            .PagedFilterBy<M, int>(criteria)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.CreatedOn,
                x.Category,
                x.CategoryId,
                x.Type,
                UpdatedOn = x.CreatedOn,
            })
            .ToListAsync();
        return Ok(Result.Of(result, criteria));
    }

    /// <summary>
    /// Create Symbol
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(M.ResponseModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] M.CreateOrUpdateModel model)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(model.Code))
        {
            return BadRequest(new { error = "Code is required." });
        }

        if (string.IsNullOrWhiteSpace(model.Category))
        {
            return BadRequest(new { error = "Category is required." });
        }

        // Check if symbol with same CategoryId, Code, and Type already exists
        var existingSymbol = await tenantDbContext.Symbols
            .AnyAsync(x => x.CategoryId == model.CategoryId && x.Code == model.Code && x.Type == model.Type);
        
        if (existingSymbol)
        {
            return BadRequest(new { error = $"A symbol with CategoryId '{model.CategoryId}', Code '{model.Code}', and Type '{model.Type}' already exists." });
        }

        var symbol = new M
        {
            Code = model.Code,
            CategoryId = model.CategoryId,
            Category = model.Category,
            Type = model.Type,
            CreatedOn = DateTime.UtcNow,
            OperatorPartyId = GetPartyId()
        };

        tenantDbContext.Symbols.Add(symbol);
        await tenantDbContext.SaveChangesAsync();

        var response = symbol.ToResponseModel();

        return CreatedAtAction(nameof(Get), new { id = symbol.Id }, response);
    }

    /// <summary>
    /// Batch Import Symbols
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BatchImport([FromBody] M.BatchImportModel model)
    {
        // Validate required fields
        if (model.Codes == null || !model.Codes.Any())
        {
            return BadRequest(new { error = "Codes array is required and cannot be empty." });
        }

        if (string.IsNullOrWhiteSpace(model.Category))
        {
            return BadRequest(new { error = "Category is required." });
        }

        var results = new List<object>();
        var errors = new List<object>();
        var successCount = 0;
        var skippedCount = 0;

        foreach (var code in model.Codes)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                errors.Add(new { code = code, error = "Code cannot be empty." });
                continue;
            }

            // Check if symbol with same CategoryId, Code, and Type already exists
            var existingSymbol = await tenantDbContext.Symbols
                .AnyAsync(x => x.CategoryId == model.CategoryId && x.Code == code && x.Type == model.Type);
            
            if (existingSymbol)
            {
                skippedCount++;
                errors.Add(new { code = code, error = $"A symbol with CategoryId '{model.CategoryId}', Code '{code}', and Type '{model.Type}' already exists." });
                continue;
            }

            var symbol = new M
            {
                Code = code,
                CategoryId = model.CategoryId,
                Category = model.Category,
                Type = model.Type,
                CreatedOn = DateTime.UtcNow,
                OperatorPartyId = GetPartyId()
            };

            tenantDbContext.Symbols.Add(symbol);
            successCount++;
            results.Add(new { code = code, status = "created" });
        }

        await tenantDbContext.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            totalProcessed = model.Codes.Count(),
            successCount = successCount,
            skippedCount = skippedCount,
            results = results,
            errors = errors
        });
    }

    /// <summary>
    /// Create a new Category with auto-generated CategoryId
    /// </summary>
    [HttpPost("category")]
    [ProducesResponseType(typeof(M.ResponseModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] M.CreateCategoryModel model)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(model.Category))
        {
            return BadRequest(new { error = "Category is required." });
        }

        // Get the max CategoryId for the specified Type
        var maxCategoryId = await tenantDbContext.Symbols
            .Where(x => x.Type == model.Type)
            .MaxAsync(x => (int?)x.CategoryId) ?? 0;

        var newCategoryId = maxCategoryId + 1;

        // Use "default" as Code if not provided
        var code = string.IsNullOrWhiteSpace(model.Code) ? "default" : model.Code;

        // Check if symbol with same CategoryId, Code, and Type already exists
        var existingSymbol = await tenantDbContext.Symbols
            .AnyAsync(x => x.CategoryId == newCategoryId && x.Code == code && x.Type == model.Type);
        
        if (existingSymbol)
        {
            return BadRequest(new { error = $"A symbol with CategoryId '{newCategoryId}', Code '{code}', and Type '{model.Type}' already exists." });
        }

        var symbol = new M
        {
            Code = code,
            CategoryId = newCategoryId,
            Category = model.Category,
            Type = model.Type,
            CreatedOn = DateTime.UtcNow,
            OperatorPartyId = GetPartyId()
        };

        tenantDbContext.Symbols.Add(symbol);

        // Update DefaultRebateLevelSetting Configuration
        // Add the new category to "Standard" and "alpha" options
        await UpdateDefaultRebateLevelSettingAsync(newCategoryId);

        // Save Symbol + Configuration add
        await tenantDbContext.SaveChangesAsync();

        // await CleanupOrphanedCategoriesAsync();

        var response = symbol.ToResponseModel();

        return CreatedAtAction(nameof(Get), new { id = symbol.Id }, response);
    }

    /// <summary>
    /// Update Category name for all symbols with matching Type 
    /// </summary>
    [HttpPut("category/{categoryId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] M.UpdateCategoryModel model)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(model.Category))
        {
            return BadRequest(new { error = "Category is required." });
        }

        // Build query based on Type and optional Code
        var query = tenantDbContext.Symbols.Where(x => x.Type == model.Type && x.CategoryId == categoryId);

        var symbolsToUpdate = await query.ToListAsync();

        if (!symbolsToUpdate.Any())
        {
            var errorMsg = string.IsNullOrWhiteSpace(model.Code)
                ? $"No symbols found for Type '{model.Type}'."
                : $"No symbols found for Type '{model.Type}' and Code '{model.Code}'.";
            return NotFound(new { error = errorMsg });
        }

        // Update category for all matching symbols
        foreach (var symbol in symbolsToUpdate)
        {
            symbol.Category = model.Category;
        }

        await tenantDbContext.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Category updated successfully.",
            updatedCount = symbolsToUpdate.Count,
            updatedSymbols = symbolsToUpdate.Select(x => new
            {
                id = x.Id,
                code = x.Code,
                categoryId = x.CategoryId,
                category = x.Category,
                type = x.Type
            }).ToArray()
        });
    }

    /// <summary>
    /// Delete Category and all symbols/codes within it
    /// </summary>
    /// <param name="type">Symbol Type (e.g., 300, 400)</param>
    /// <param name="categoryId">Category Id</param>
    [HttpDelete("type/{type:int}/category/{categoryId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int type, int categoryId)
    {
        // Get all symbols in this category with the specified type
        var symbolsInCategory = await tenantDbContext.Symbols
            .Where(x => x.CategoryId == categoryId && x.Type == type)
            .ToListAsync();

        if (!symbolsInCategory.Any())
        {
            return NotFound(new { error = $"No symbols found for CategoryId '{categoryId}' and Type '{type}'." });
        }

        // Delete all symbols in this category
        tenantDbContext.Symbols.RemoveRange(symbolsInCategory);

        // Delete the corresponding category from DefaultRebateLevelSetting configuration
        await DeleteFromDefaultRebateLevelSettingAsync(categoryId);

        // Save symbol deletions and configuration updates before cleanup
        await tenantDbContext.SaveChangesAsync();

        // await CleanupOrphanedCategoriesAsync();

        return Ok(new
        {
            success = true,
            message = $"Category with CategoryId '{categoryId}' and Type '{type}' deleted successfully.",
            deletedCount = symbolsInCategory.Count,
            deletedCodes = symbolsInCategory.Select(x => x.Code).ToArray()
        });
    }

    /// <summary>
    /// Update Symbol Info
    /// </summary>
    /// <param name="id">Symbol Id</param>
    /// <param name="model">CreateOrUpdateModel</param
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] M.CreateOrUpdateModel model)
    {
        var symbol = await tenantDbContext.Symbols.FindAsync(id);
        if (symbol == null)
        {
            return NotFound();
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(model.Code))
        {
            return BadRequest(new { error = "Code is required." });
        }

        if (string.IsNullOrWhiteSpace(model.Category))
        {
            return BadRequest(new { error = "Category is required." });
        }

        // Check if another symbol with same CategoryId, Code, and Type already exists (excluding current symbol)
        var existingSymbol = await tenantDbContext.Symbols
            .AnyAsync(x => x.CategoryId == model.CategoryId && x.Code == model.Code && x.Type == model.Type && x.Id != id);
        
        if (existingSymbol)
        {
            return BadRequest(new { error = $"A symbol with CategoryId '{model.CategoryId}', Code '{model.Code}', and Type '{model.Type}' already exists." });
        }

        // Update symbol properties
        symbol.Code = model.Code;
        symbol.CategoryId = model.CategoryId;
        symbol.Category = model.Category;
        symbol.Type = model.Type;

        await tenantDbContext.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Get Symbol by Id
    /// </summary>
    /// <param name="id">Symbol Id</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(M.ResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var item = await tenantDbContext.Symbols
            .Include(x => x.OperatorParty)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (item == null) return NotFound();

        var response = item.ToResponseModel();

        return Ok(response);
    }

    /// <summary>
    /// Delete Symbol. Do not care if symbol is referenced in other tables (e.g., TradeRebates)
    /// </summary>
    /// <param name="id">Symbol Id</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        var symbol = await tenantDbContext.Symbols.FindAsync(id);
        if (symbol == null)
        {
            return NotFound();
        }

        // Not allow delete if there's no symbol after delete
        var remainingCount = await tenantDbContext.Symbols
            .Where(x => x.CategoryId == symbol.CategoryId && x.Type == symbol.Type)
            .Where(x => x.Id != id)
            .CountAsync();
        if (remainingCount == 0)
        {
            return BadRequest(Result.Error("__CANNOT_DELETE_LAST_SYMBOL_IN_ONE_CATEGORY__"));
        }

        tenantDbContext.Symbols.Remove(symbol);
        await tenantDbContext.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Delete Multiple Symbols by IDs
    /// </summary>
    /// <param name="model">Model containing array of symbol IDs to delete</param>
    [HttpDelete("batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteMultiple([FromBody] M.DeleteMultipleModel model)
    {
        if (model.Ids == null || !model.Ids.Any())
        {
            return BadRequest(new { error = "Ids array is required and cannot be empty." });
        }

        // Get all symbols with the provided IDs
        var symbolsToDelete = await tenantDbContext.Symbols
            .Where(x => model.Ids.Contains(x.Id))
            .ToListAsync();

        if (!symbolsToDelete.Any())
        {
            return Ok(new
            {
                success = true,
                message = "No symbols found with the provided IDs.",
                deletedCount = 0,
                notFoundIds = model.Ids.ToArray()
            });
        }

        var foundIds = symbolsToDelete.Select(x => x.Id).ToList();
        var notFoundIds = model.Ids.Except(foundIds).ToArray();

        // Not allow delete if there's no symbol after delete
        var remainingCount = await tenantDbContext.Symbols
            .Where(x => x.CategoryId == symbolsToDelete[0].CategoryId && x.Type == symbolsToDelete[0].Type)
            .Where(x => !model.Ids.Contains(x.Id))
            .CountAsync();
        if (remainingCount == 0)
        {
            return BadRequest(Result.Error("__CANNOT_DELETE_LAST_SYMBOL_IN_ONE_CATEGORY__"));
        }

        // Get the categories and types of symbols being deleted
        var deletedCategoriesAndTypes = symbolsToDelete
            .Select(x => new { x.CategoryId, x.Type })
            .Distinct()
            .ToList();

        // Delete the symbols
        tenantDbContext.Symbols.RemoveRange(symbolsToDelete);

        // Commented out to prevent automatic removal of categories from configuration
        // Check if any categories are now empty and clean up DefaultRebateLevelSetting configuration
        //var emptyCategories = new List<int>();
        //foreach (var cat in deletedCategoriesAndTypes)
        //{
            // Check if there are any remaining symbols in this category with this type
            //var remainingCount = await tenantDbContext.Symbols
            //    .Where(x => x.CategoryId == cat.CategoryId && x.Type == cat.Type)
            //    .Where(x => !model.Ids.Contains(x.Id)) // Exclude the ones we're deleting
            //    .CountAsync();

            //if (remainingCount == 0)
            //{
            //    // No more symbols in this category, remove from configuration
            //    await DeleteFromDefaultRebateLevelSettingAsync(cat.CategoryId);
            //    emptyCategories.Add(cat.CategoryId);
            //}
        //}

        // Save deletions and empty category removals before cleanup
        await tenantDbContext.SaveChangesAsync();

        //await CleanupOrphanedCategoriesAsync();

        return Ok(new
        {
            success = true,
            message = $"Successfully deleted {symbolsToDelete.Count} symbol(s).",
            deletedCount = symbolsToDelete.Count,
            deletedIds = foundIds.ToArray(),
            notFoundIds = notFoundIds,
            //emptyCategoriesRemoved = emptyCategories.ToArray()
        });
    }

    /// <summary>
    /// Get distinct symbol categories grouped by Type
    /// </summary>
    /// <param name="type">Optional: Filter by symbol type (e.g., 300, 400)</param>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories([FromQuery] int? type = null)
    {
        var query = tenantDbContext.Symbols.AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(x => x.Type == type.Value);
        }

        var categories = await query
            .GroupBy(x => new { x.CategoryId, x.Category, x.Type })
            .Select(g => new
            {
                CategoryId = g.Key.CategoryId,
                Category = g.Key.Category,
                Type = g.Key.Type,
                Symbols = g.OrderByDescending(x => x.Id).Select(x => new { Id = x.Id, Code = x.Code }).ToArray()
            })
            .OrderBy(x => x.Type)
            .ThenByDescending(x => x.CategoryId)
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Update DefaultRebateLevelSetting configuration to include new category
    /// Adds the new category to "Standard" and "alpha" options with default value 0
    /// </summary>
    /// <param name="newCategoryId">The new category ID to add</param>
    private async Task UpdateDefaultRebateLevelSettingAsync(int newCategoryId)
    {
        const string configName = "DefaultRebateLevelSetting";
        const long configRowId = 0;
        const int defaultValue = 0;

        try
        {
            // Get the configuration
            var config = await tenantDbContext.Configurations
                .Where(x => x.Name.ToLower().Contains(configName.ToLower()) && x.RowId == configRowId)
                .FirstOrDefaultAsync();

            if (config == null)
            {
                // Configuration not found, log warning but don't fail the category creation
                return;
            }

            // Parse the JSON
            var jsonData = JObject.Parse(config.Value);
            bool updated = false;
            
            // Convert newCategoryId to string for comparison
            var newCategoryKey = newCategoryId.ToString();

            // Process each account type in the configuration
            foreach (var accountTypeProp in jsonData.Properties())
            {
                var optionsArray = accountTypeProp.Value as JArray;
                if (optionsArray == null) continue;

                // Find and update "Standard" and "alpha" options
                foreach (var optionToken in optionsArray)
                {
                    var option = optionToken as JObject;
                    if (option == null) continue;

                    var optionName = option["OptionName"]?.ToString();
                    if (optionName == null || !targetOptionNames.Contains(optionName, StringComparer.OrdinalIgnoreCase))
                        continue;

                    // Get the Category object
                    var categoryObj = option["Category"] as JObject;
                    if (categoryObj == null) continue;

                    // Check if the newCategoryId already exists in the configuration
                    if (categoryObj.ContainsKey(newCategoryKey))
                    {
                        // Category already exists, no need to add
                        return;
                    }
                    
                    // Add new category with the provided categoryId
                    categoryObj[newCategoryKey] = defaultValue;
                    updated = true;

                    // Also update AllowPipSetting Items if they exist
                    var allowPipSetting = option["AllowPipSetting"] as JObject;
                    if (allowPipSetting != null)
                    {
                        foreach (var pipProp in allowPipSetting.Properties())
                        {
                            var pipItem = pipProp.Value as JObject;
                            if (pipItem == null) continue;

                            var items = pipItem["Items"] as JObject;
                            if (items != null && !items.ContainsKey(newCategoryKey))
                            {
                                items[newCategoryKey] = 0.0;
                            }
                        }
                    }

                    // Also update AllowCommissionSetting Items if they exist
                    var allowCommissionSetting = option["AllowCommissionSetting"] as JObject;
                    if (allowCommissionSetting != null)
                    {
                        foreach (var commProp in allowCommissionSetting.Properties())
                        {
                            var commItem = commProp.Value as JObject;
                            if (commItem == null) continue;

                            var items = commItem["Items"] as JObject;
                            if (items != null && !items.ContainsKey(newCategoryKey))
                            {
                                items[newCategoryKey] = 0.0;
                            }
                        }
                    }
                }
            }

            // Save the updated configuration if changes were made
            if (updated)
            {
                config.Value = jsonData.ToString(Formatting.Indented);
                config.UpdatedOn = DateTime.UtcNow;
                config.UpdatedBy = GetPartyId();
                // Note: SaveChangesAsync will be called by the caller
                
                // Refresh cache to make changes take effect immediately
                await configurationService.ResetCacheAsync();
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the category creation
            // In production, you might want to use ILogger here
            Console.WriteLine($"Warning: Failed to update DefaultRebateLevelSetting: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete category from DefaultRebateLevelSetting configuration
    /// Removes the category key from "Standard" and "alpha" options
    /// </summary>
    /// <param name="categoryIdToDelete">The category ID to delete</param>
    private async Task DeleteFromDefaultRebateLevelSettingAsync(int categoryIdToDelete)
    {
        const string configName = "DefaultRebateLevelSetting";
        const long configRowId = 0;

        try
        {
            // Get the configuration
            var config = await tenantDbContext.Configurations
                .Where(x => x.Name.ToLower().Contains(configName.ToLower()) && x.RowId == configRowId)
                .FirstOrDefaultAsync();

            if (config == null)
            {
                // Configuration not found, log warning but don't fail the category deletion
                return;
            }

            // Parse the JSON
            var jsonData = JObject.Parse(config.Value);
            bool updated = false;

            // The key to delete (categoryId as string)
            var keyToDelete = categoryIdToDelete.ToString();

            // Process each account type in the configuration
            foreach (var accountTypeProp in jsonData.Properties())
            {
                var optionsArray = accountTypeProp.Value as JArray;
                if (optionsArray == null) continue;

                // Find and update "Standard" and "alpha" options
                foreach (var optionToken in optionsArray)
                {
                    var option = optionToken as JObject;
                    if (option == null) continue;

                    var optionName = option["OptionName"]?.ToString();
                    if (optionName == null || !targetOptionNames.Contains(optionName, StringComparer.OrdinalIgnoreCase))
                        continue;

                    // Get the Category object
                    var categoryObj = option["Category"] as JObject;
                    if (categoryObj != null && categoryObj.ContainsKey(keyToDelete))
                    {
                        categoryObj.Remove(keyToDelete);
                        updated = true;
                    }

                    // Also remove from AllowPipSetting Items if they exist
                    var allowPipSetting = option["AllowPipSetting"] as JObject;
                    if (allowPipSetting != null)
                    {
                        foreach (var pipProp in allowPipSetting.Properties())
                        {
                            var pipItem = pipProp.Value as JObject;
                            if (pipItem == null) continue;

                            var items = pipItem["Items"] as JObject;
                            if (items != null && items.ContainsKey(keyToDelete))
                            {
                                items.Remove(keyToDelete);
                            }
                        }
                    }

                    // Also remove from AllowCommissionSetting Items if they exist
                    var allowCommissionSetting = option["AllowCommissionSetting"] as JObject;
                    if (allowCommissionSetting != null)
                    {
                        foreach (var commProp in allowCommissionSetting.Properties())
                        {
                            var commItem = commProp.Value as JObject;
                            if (commItem == null) continue;

                            var items = commItem["Items"] as JObject;
                            if (items != null && items.ContainsKey(keyToDelete))
                            {
                                items.Remove(keyToDelete);
                            }
                        }
                    }
                }
            }

            // Save the updated configuration if changes were made
            if (updated)
            {
                config.Value = jsonData.ToString(Formatting.Indented);
                config.UpdatedOn = DateTime.UtcNow;
                config.UpdatedBy = GetPartyId();
                // Note: SaveChangesAsync will be called by the caller
                
                // Refresh cache to make changes take effect immediately
                await configurationService.ResetCacheAsync();
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the category deletion
            // In production, you might want to use ILogger here
            Console.WriteLine($"Warning: Failed to delete category from DefaultRebateLevelSetting: {ex.Message}");
        }
    }

    /// <summary>
    /// Clean up orphaned categories in DefaultRebateLevelSetting
    /// Removes categories that exist in configuration but not in Symbol table
    /// Also sorts the category keys in numerical order
    /// </summary>
    /// <param name="type">Symbol type to check (e.g., 300)</param>
    /// <remarks>
    /// ⚠️ DISABLED DUE TO CRITICAL BUGS ⚠️
    /// This method has multiple critical issues:
    /// 1. It only gets valid categories for ONE specific type, but removes ANY category key from the configuration
    ///    that's not in this list - INCLUDING categories that belong to OTHER types!
    /// 2. The DefaultRebateLevelSetting configuration stores categories for ALL symbol types together,
    ///    but this method treats it as type-specific, causing cross-type data deletion.
    /// 3. Race condition: When called after RemoveRange but before SaveChanges, the database query
    ///    may not reflect pending deletions, causing incorrect cleanup.
    /// TODO: Fix by either:
    /// - Getting ALL valid categories across ALL types (not just the specified type)
    /// - Or tracking which categories belong to which type in the configuration structure
    /// - Or removing this automatic cleanup entirely and requiring manual cleanup
    /// </remarks>
    private async Task CleanupOrphanedCategoriesAsync()
    {
        const string configName = "DefaultRebateLevelSetting";
        const long configRowId = 0;
        const int targetType = 300; // Only for type 300

        try
        {
            // Get all valid category IDs from Symbol table for this type
            var validCategoryIds = await tenantDbContext.Symbols
                .Where(x => x.Type == targetType)
                .Select(x => x.CategoryId)
                .Distinct()
                .ToListAsync();

            var validCategoryKeys = validCategoryIds.Select(x => x.ToString()).ToHashSet();

            // Get the configuration
            var config = await tenantDbContext.Configurations
                .Where(x => x.Name.ToLower().Contains(configName.ToLower()) && x.RowId == configRowId)
                .FirstOrDefaultAsync();

            if (config == null)
            {
                return;
            }

            // Parse the JSON
            var jsonData = JObject.Parse(config.Value);
            bool updated = false;

            // Process each account type in the configuration
            foreach (var accountTypeProp in jsonData.Properties())
            {
                var optionsArray = accountTypeProp.Value as JArray;
                if (optionsArray == null) continue;

                // Find and update "Standard" and "alpha" options
                foreach (var optionToken in optionsArray)
                {
                    var option = optionToken as JObject;
                    if (option == null) continue;

                    var optionName = option["OptionName"]?.ToString();
                    if (optionName == null || !targetOptionNames.Contains(optionName, StringComparer.OrdinalIgnoreCase))
                        continue;

                    // Clean up Category object
                    var categoryObj = option["Category"] as JObject;
                    if (categoryObj != null)
                    {
                        var orphanedKeys = categoryObj.Properties()
                            .Select(p => p.Name)
                            .Where(key => !validCategoryKeys.Contains(key))
                            .ToList();

                        foreach (var orphanedKey in orphanedKeys)
                        {
                            categoryObj.Remove(orphanedKey);
                            updated = true;
                        }

                        // Sort the category keys in numerical order
                        if (updated)
                        {
                            var sortedCategory = new JObject(
                                categoryObj.Properties()
                                    .OrderBy(p => int.TryParse(p.Name, out int num) ? num : int.MaxValue)
                            );
                            option["Category"] = sortedCategory;
                        }
                    }

                    // Clean up AllowPipSetting Items
                    var allowPipSetting = option["AllowPipSetting"] as JObject;
                    if (allowPipSetting != null)
                    {
                        foreach (var pipProp in allowPipSetting.Properties())
                        {
                            var pipItem = pipProp.Value as JObject;
                            if (pipItem == null) continue;

                            var items = pipItem["Items"] as JObject;
                            if (items != null)
                            {
                                var orphanedKeys = items.Properties()
                                    .Select(p => p.Name)
                                    .Where(key => !validCategoryKeys.Contains(key))
                                    .ToList();

                                foreach (var orphanedKey in orphanedKeys)
                                {
                                    items.Remove(orphanedKey);
                                }

                                // Sort the items keys in numerical order
                                if (orphanedKeys.Any())
                                {
                                    var sortedItems = new JObject(
                                        items.Properties()
                                            .OrderBy(p => int.TryParse(p.Name, out int num) ? num : int.MaxValue)
                                    );
                                    pipItem["Items"] = sortedItems;
                                }
                            }
                        }
                    }

                    // Clean up AllowCommissionSetting Items
                    var allowCommissionSetting = option["AllowCommissionSetting"] as JObject;
                    if (allowCommissionSetting != null)
                    {
                        foreach (var commProp in allowCommissionSetting.Properties())
                        {
                            var commItem = commProp.Value as JObject;
                            if (commItem == null) continue;

                            var items = commItem["Items"] as JObject;
                            if (items != null)
                            {
                                var orphanedKeys = items.Properties()
                                    .Select(p => p.Name)
                                    .Where(key => !validCategoryKeys.Contains(key))
                                    .ToList();

                                foreach (var orphanedKey in orphanedKeys)
                                {
                                    items.Remove(orphanedKey);
                                }

                                // Sort the items keys in numerical order
                                if (orphanedKeys.Any())
                                {
                                    var sortedItems = new JObject(
                                        items.Properties()
                                            .OrderBy(p => int.TryParse(p.Name, out int num) ? num : int.MaxValue)
                                    );
                                    commItem["Items"] = sortedItems;
                                }
                            }
                        }
                    }
                }
            }

            // Save the updated configuration if changes were made
            if (updated)
            {
                config.Value = jsonData.ToString(Formatting.Indented);
                config.UpdatedOn = DateTime.UtcNow;
                config.UpdatedBy = GetPartyId();
                
                // Save changes immediately 
                await tenantDbContext.SaveChangesAsync();
                
                // Refresh cache to make changes take effect immediately
                await configurationService.ResetCacheAsync();
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the operation
            Console.WriteLine($"Warning: Failed to cleanup orphaned categories: {ex.Message}");
        }
    }
}