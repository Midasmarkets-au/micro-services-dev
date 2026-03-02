using StackExchange.Redis;

namespace Bacera.Gateway.Services;

public interface IMyCache
{
    string GetInstanceName();
    IDatabase GetDatabase();
    IServer GetServer();
    Task SetStringAsync(string key, RedisValue value, TimeSpan? expiry = null);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null) where T : class, new();
    Task HSetStringAsync(string key, string field, string value, TimeSpan? expiry = null);
    Task HSetDeleteByKeyFieldAsync(string key, string field);
    Task HSetDeleteByKeyAsync(string key);
    Task<string?> GetStringAsync(string key);
    Task<string?> HGetStringAsync(string key, string field);
    Task<T?> HGetAsync<T>(string key, string field) where T : class, new();

    Task<T> HGetOrSetAsync<T>(string key, string field, Func<Task<T>> factory, TimeSpan? expiry = null)
        where T : class, new();
    
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    Task<T?> GetAsync<T>(string key) where T : class, new();
    Task<long> HDecrementAsync(string key, string field, long value = 1);
    Task<long> HIncrementAsync(string key, string field, long value = 1);

    Task KeyDeleteAsync(string key);
    Task<bool> AddToSetAsync(string key, string value, TimeSpan? expiry = null);
    Task<bool> IsMemberOfSetAsync(string key, string value);
    Task<string[]> GetAllMembersAsync(string key);
    Task<bool> RemoveFromSetAsync(string key, string value);
    Task<long> GetSetCountAsync(string key);
    Task<bool> AddToSortedSetAsync(string key, string value, double score, TimeSpan? expiry = null);
    Task<string[]> GetFromSortedSetAsync(string key, long start = 0, long stop = -1);
    bool TryGetDistributedLock(string lockKey, string lockValue, TimeSpan expiry);
    bool ReleaseDistributedLock(string lockKey, string lockValue);
    Task<TimeSpan?> GetTtlAsync(string key);
}