using Bacera.Gateway.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Bacera.Gateway.Services;

public class MyCache(
    IConnectionMultiplexer connectionMultiplexer,
    IWebHostEnvironment? env = null)
    : IMyCache
{
    private readonly string _instanceName = "portal:" + (env != null ? env.EnvironmentName.ToLower() : "UNKNOWN") + "_";
    public string GetInstanceName() => _instanceName;

    private IDatabase Database => connectionMultiplexer.GetDatabase();

    public IDatabase GetDatabase() => Database;
    public IServer GetServer() => connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().First());

    private const string ReleaseLockScript = """
                                             if redis.call('get', KEYS[1]) == ARGV[1] then
                                                 return redis.call('del', KEYS[1])
                                             else
                                                 return 0
                                             end
                                             """;

    public bool TryGetDistributedLock(string lockKey, string lockValue, TimeSpan expiry)
        => Database.StringSet(lockKey, lockValue, expiry, When.NotExists);

    public bool ReleaseDistributedLock(string lockKey, string lockValue)
        => (int)Database.ScriptEvaluate(ReleaseLockScript, [lockKey], [lockValue]) != 0;

    public async Task SetStringAsync(string key, RedisValue value, TimeSpan? expiry = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;
        await Database.StringSetAsync(key, value, expiry);
    }

    public async Task<long> HIncrementAsync(string key, string field, long value = 1)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(field))
            return 0;
        return await Database.HashIncrementAsync(key, field, value);
    }

    public async Task<long> HDecrementAsync(string key, string field, long value = 1)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(field))
            return 0;
        return await Database.HashDecrementAsync(key, field, value);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(key))
            return new T();
        var value = await Database.StringGetAsync(key);
        if (value.HasValue)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value!)!;
            }
            catch (Exception)
            {
                // BcrLog.Slack($"Cache_GetOrSetAsync_error deserializing cache message: {e.Message}, value: {value}");
                await Database.KeyDeleteAsync(key);
                return await factory();
            }
        }

        var newValue = await factory();
        await Database.StringSetAsync(key, JsonConvert.SerializeObject(newValue), expiry);
        return newValue;
    }

    public async Task HSetStringAsync(string key, string field, string value, TimeSpan? expiry = null)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(field))
            return;
        await Database.HashSetAsync(key, field, value);
        if (expiry != null)
            await Database.KeyExpireAsync(key, expiry);
    }

    public async Task HSetDeleteByKeyFieldAsync(string key, string field)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(field))
            return;
        await Database.HashDeleteAsync(key, field);
    }

    public async Task HSetDeleteByKeyAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;
        await Database.KeyDeleteAsync(key);
    }

    public async Task<string?> GetStringAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        return await Database.StringGetAsync(key);
    }

    public async Task<string?> HGetStringAsync(string key, string field)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(field))
            return null;

        return await Database.HashGetAsync(key, field);
    }

    public async Task<Dictionary<string, bool>> HGetManyAsBoolAsync(string key, IEnumerable<string> fields)
    {
        if (string.IsNullOrWhiteSpace(key))
            return new Dictionary<string, bool>();

        var fieldList = fields.Where(f => !string.IsNullOrWhiteSpace(f)).Distinct().ToList();
        if (fieldList.Count == 0)
            return new Dictionary<string, bool>();

        var db = Database;
        var batch = db.CreateBatch();
        var tasks = fieldList.ToDictionary(f => f, f => batch.HashGetAsync(key, f));
        batch.Execute();
        await Task.WhenAll(tasks.Values);

        return tasks.ToDictionary(kv => kv.Key, kv => kv.Value.Result == "1");
    }

    public async Task<T?> HGetAsync<T>(string key, string field) where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(field))
            return null;

        var value = await Database.HashGetAsync(key, field);
        if (!value.HasValue)
            return null;
        try
        {
            return JsonConvert.DeserializeObject<T>(value!);
        }
        catch (Exception e)
        {
            BcrLog.Slack($"Cache_HGetAsync_error deserializing cache value: {e.Message}");
            await Database.HashDeleteAsync(key, field);
            return null;
        }
    }

    public async Task<T> HGetOrSetAsync<T>(string key, string field, Func<Task<T>> factory, TimeSpan? expiry = null)
        where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(field))
            return new T();

        var value = await Database.HashGetAsync(key, field);
        if (value.HasValue)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value!)!;
            }
            catch (Exception e)
            {
                BcrLog.Slack($"Cache_HGetOrSetAsync_error deserializing cache value: {e.Message}");
                await Database.HashDeleteAsync(key, field);
                return await factory();
            }
        }

        var newValue = await factory();
        await Database.HashSetAsync(key, field, JsonConvert.SerializeObject(newValue));
        if (expiry != null)
            await Database.KeyExpireAsync(key, expiry);
        return newValue;
    }


    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;
        await Database.StringSetAsync(key, JsonConvert.SerializeObject(value), expiry);
    }

    public async Task<T?> GetAsync<T>(string key) where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var value = await Database.StringGetAsync(key);
        if (!value.HasValue)
            return null;

        try
        {
            return JsonConvert.DeserializeObject<T>(value!);
        }
        catch (Exception)
        {
            BcrLog.Slack($"Cache_GetAsync_error deserializing cache value: {key}");
            await Database.KeyDeleteAsync(key);
            return null;
        }
    }

    public async Task KeyDeleteAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        await Database.KeyDeleteAsync(key);
    }

    public async Task<bool> AddToSetAsync(string key, string value, TimeSpan? expiry = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        var result = await Database.SetAddAsync(key, value);
        if (expiry != null)
            await Database.KeyExpireAsync(key, expiry);
        return result;
    }

    public async Task<bool> IsMemberOfSetAsync(string key, string value)
    {
        return await Database.SetContainsAsync(key, value);
    }

    public async Task<string[]> GetAllMembersAsync(string key)
    {
        var values = await Database.SetMembersAsync(key);
        return values.Select(x => x.ToString()).ToArray();
    }

    public async Task<bool> RemoveFromSetAsync(string key, string value)
    {
        return await Database.SetRemoveAsync(key, value);
    }

    public async Task<long> GetSetCountAsync(string key)
    {
        return await Database.SetLengthAsync(key);
    }

    public async Task<bool> AddToSortedSetAsync(string key, string value, double score, TimeSpan? expiry = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        var result = await Database.SortedSetAddAsync(key, value, score);
        if (expiry != null)
            await Database.KeyExpireAsync(key, expiry);
        return result;
    }

    public async Task<string[]> GetFromSortedSetAsync(string key, long start = 0, long stop = -1)
    {
        var values = await Database.SortedSetRangeByRankAsync(key, start, stop);
        return values.Select(x => x.ToString()).ToArray();
    }

    public async Task<TimeSpan?> GetTtlAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        return await Database.KeyTimeToLiveAsync(key);
    }

    // public async Task RemoveWithWildCardAsync(string keyRoot)
    // {
    //     if (string.IsNullOrWhiteSpace(keyRoot))
    //         return;
    //
    //     // get all the keys* and remove each one
    //     await foreach (var key in GetKeysAsync(keyRoot + "*"))
    //     {
    //         await Database.KeyDeleteAsync(key);
    //     }
    // }
    //
    // public async IAsyncEnumerable<string> GetKeysAsync(string pattern)
    // {
    //     foreach (var endpoint in _connectionMultiplexer.GetEndPoints())
    //     {
    //         var server = _connectionMultiplexer.GetServer(endpoint);
    //         await foreach (var key in server.KeysAsync(pattern: _instanceName + pattern))
    //         {
    //             yield return key.ToString(); // stream the results back
    //         }
    //     }
    // }
    //
    // public IEnumerable<RedisFeatures> GetRedisFeatures()
    // {
    //     foreach (var endpoint in _connectionMultiplexer.GetEndPoints())
    //     {
    //         var server = _connectionMultiplexer.GetServer(endpoint);
    //         yield return server.Features;
    //     }
    // }
}