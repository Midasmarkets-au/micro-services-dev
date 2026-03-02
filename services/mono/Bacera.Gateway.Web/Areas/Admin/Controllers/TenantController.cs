using Microsoft.AspNetCore.Mvc;
using M = Bacera.Gateway.Tenant;
using MSG = Bacera.Gateway.ResultMessage.Tenant;
using Bacera.Gateway.Services;
using StackExchange.Redis;

namespace Bacera.Gateway.Web.Areas.Admin.Controllers;

public class TenantController : AdminBaseController
{
    private readonly ITenantService _tenantSvc;
    private readonly IMyCache _cache;

    public TenantController(ITenantService tenantService, IMyCache cache)
    {
        _tenantSvc = tenantService;
        _cache = cache;
    }

    [HttpGet]
    public async Task<ActionResult> Index()
    {
        var items = await _tenantSvc.GetAllTenantsAsync();
        return Ok(items);
    }

    [HttpPut("cache/reload")]
    public ActionResult ReloadCache()
    {
        Task.Run(() => _tenantSvc.ResetCacheAsync());
        return NoContent();
    }

    /// <summary>
    /// Get list of Redis keys
    /// </summary>
    /// <param name="pattern">Optional pattern to filter keys (e.g., "portal:*")</param>
    /// <param name="pageSize">Number of keys to return per page (default: 100, max: 1000)</param>
    /// <param name="cursor">Cursor for pagination (default: 0)</param>
    /// <returns>List of Redis keys with pagination info</returns>
    [HttpGet("redis/keys")]
    public async Task<ActionResult> GetRedisKeys([FromQuery] string? pattern = null, [FromQuery] int pageSize = 100, [FromQuery] int cursor = 0)
    {
        try
        {
            var db = _cache.GetDatabase();
            var searchPattern = pattern ?? "*";
            
            // Limit page size to prevent performance issues
            var effectivePageSize = Math.Min(Math.Max(pageSize, 1), 1000);
            
            var keys = new List<string>();
            var server = _cache.GetServer();
            
            // Use SCAN for better performance with large datasets
            var scanCursor = (long)cursor;
            do
            {
                var scanResult = await db.ExecuteAsync("SCAN", scanCursor.ToString(), "MATCH", searchPattern, "COUNT", effectivePageSize.ToString());
                var redisResults = (RedisResult[])scanResult!;
                
                scanCursor = long.Parse((string)redisResults[0]!);
                var resultKeys = (RedisResult[])redisResults[1]!;
                
                foreach (var key in resultKeys)
                {
                    keys.Add(key.ToString()!);
                    if (keys.Count >= effectivePageSize)
                        break;
                }
            } while (scanCursor != 0 && keys.Count < effectivePageSize);
            
            var totalCount = keys.Count;
            var nextCursor = scanCursor != 0 ? (int)scanCursor : -1;
            
            return Ok(Result.Of(new
            {
                keys,
                totalCount,
                nextCursor,
                pattern = searchPattern
            }));
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Error($"Failed to retrieve Redis keys: {ex.Message}"));
        }
    }

    /// <summary>
    /// Delete one or multiple Redis keys
    /// </summary>
    /// <param name="keys">Array of keys to delete</param>
    /// <returns>Number of keys deleted</returns>
    [HttpDelete("redis/keys")]
    public async Task<ActionResult> DeleteRedisKeys([FromBody] string[] keys)
    {
        try
        {
            if (keys == null || keys.Length == 0)
            {
                return BadRequest(Result.Error("No keys provided for deletion"));
            }

            // Filter out null/empty keys
            var validKeys = keys.Where(k => !string.IsNullOrWhiteSpace(k)).ToArray();
            if (validKeys.Length == 0)
            {
                return BadRequest(Result.Error("No valid keys provided for deletion"));
            }

            var db = _cache.GetDatabase();
            var deletedCount = 0;
            var failedKeys = new List<string>();
            var isClusterMode = Environment.GetEnvironmentVariable("REDIS_CLUSTER_MODE")?
                .Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

            // In cluster mode or for single key, delete individually to handle cross-slot issues
            if (isClusterMode || validKeys.Length == 1)
            {
                // Delete keys individually - handle each deletion separately so one failure doesn't stop others
                var deleteTasks = validKeys.Select(async key =>
                {
                    try
                    {
                        var deleted = await db.KeyDeleteAsync(key);
                        return new { Key = key, Success = deleted, Error = (string?)null };
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue with other keys
                        return new { Key = key, Success = false, Error = (string?)ex.Message };
                    }
                }).ToArray();

                var results = await Task.WhenAll(deleteTasks);
                
                foreach (var result in results)
                {
                    if (result.Success)
                    {
                        deletedCount++;
                    }
                    else
                    {
                        failedKeys.Add(result.Key);
                    }
                }
            }
            else
            {
                // Try batch delete for non-cluster mode (more efficient)
                try
                {
                    var redisKeys = validKeys.Select(k => (RedisKey)k).ToArray();
                    var deleted = await db.KeyDeleteAsync(redisKeys);
                    deletedCount = (int)deleted;
                    
                    // Track failed keys (keys that weren't deleted)
                    if (deletedCount < validKeys.Length)
                    {
                        // Verify which keys still exist
                        for (int i = 0; i < validKeys.Length; i++)
                        {
                            var exists = await db.KeyExistsAsync(validKeys[i]);
                            if (exists)
                            {
                                failedKeys.Add(validKeys[i]);
                            }
                        }
                    }
                }
                catch (RedisException)
                {
                    // Batch delete failed (possibly cluster mode or cross-slot issue), fall back to individual deletion
                    var deleteTasks = validKeys.Select(async key =>
                    {
                        try
                        {
                            var deleted = await db.KeyDeleteAsync(key);
                            return new { Key = key, Success = deleted };
                        }
                        catch
                        {
                            return new { Key = key, Success = false };
                        }
                    }).ToArray();

                    var results = await Task.WhenAll(deleteTasks);
                    
                    foreach (var result in results)
                    {
                        if (result.Success)
                        {
                            deletedCount++;
                        }
                        else
                        {
                            failedKeys.Add(result.Key);
                        }
                    }
                }
            }
            
            var responseData = new
            {
                deletedCount,
                totalRequested = validKeys.Length,
                failedCount = failedKeys.Count,
                failedKeys = failedKeys.Count > 0 ? failedKeys.ToArray() : null
            };

            if (failedKeys.Count > 0)
            {
                return Ok(Result.Success(
                    $"Successfully deleted {deletedCount} out of {validKeys.Length} keys. {failedKeys.Count} key(s) failed to delete.",
                    responseData
                ));
            }
            
            return Ok(Result.Success($"Successfully deleted {deletedCount} out of {validKeys.Length} keys", responseData));
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Error($"Failed to delete Redis keys: {ex.Message}"));
        }
    }

    /// <summary>
    /// Flush all Redis keys (WARNING: This will delete ALL keys in the Redis database)
    /// Note: This operation is not available in Redis cluster mode for security reasons.
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("redis/flush-all")]
    public async Task<ActionResult> FlushAllRedisKeys()
    {
        try
        {
            // Check if running in cluster mode
            var isClusterMode = Environment.GetEnvironmentVariable("REDIS_CLUSTER_MODE")?
                .Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
            
            if (isClusterMode)
            {
                return BadRequest(Result.Error(
                    "FlushAll operation is not available in Redis cluster mode. " +
                    "Admin commands are disabled in cluster mode for security reasons. " +
                    "In cluster mode, FLUSHALL would only flush the current node, not the entire cluster. " +
                    "Please use the delete keys endpoint to remove specific keys instead."
                ));
            }

            var server = _cache.GetServer();
            await server.FlushAllDatabasesAsync();
            
            return Ok(Result.Success("All Redis keys have been flushed successfully"));
        }
        catch (RedisException ex) when (ex.Message.Contains("admin", StringComparison.OrdinalIgnoreCase) || 
                                         ex.Message.Contains("not allowed", StringComparison.OrdinalIgnoreCase) ||
                                         ex.Message.Contains("command", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(Result.Error(
                "FlushAll operation failed: Admin commands are not enabled. " +
                "This is typically disabled in Redis cluster mode for security reasons. " +
                "Please use the delete keys endpoint to remove specific keys instead."
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Error($"Failed to flush Redis keys: {ex.Message}"));
        }
    }
}