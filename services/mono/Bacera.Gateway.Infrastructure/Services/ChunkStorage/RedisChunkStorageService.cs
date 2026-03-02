using Microsoft.AspNetCore.Hosting;
using StackExchange.Redis;

namespace Bacera.Gateway.Services.ChunkStorage;

public class RedisChunkStorageService(
    IConnectionMultiplexer connectionMultiplexer,
    IHostingEnvironment? env = null) : IChunkStorageService
{
    private readonly string _instanceName = "upload:" + (env != null ? env.EnvironmentName.ToLower() : "UNKNOWN") + ":";
    private IDatabase Database => connectionMultiplexer.GetDatabase();
    private string GetChunkKey(string fileId, long chunkIndex) => $"{_instanceName}_upload:chunk:{fileId}:{chunkIndex}";

    private string GetChunkListKey(string fileId) => $"{_instanceName}_upload:chunks:{fileId}";

    public async Task<string> SaveChunkAsync(string fileId, long chunkIndex, byte[] data)
    {
        if (string.IsNullOrEmpty(fileId))
            fileId = Guid.NewGuid().ToString();

        var chunkKey = GetChunkKey(fileId, chunkIndex);
        var chunkListKey = GetChunkListKey(fileId);

        await Database.StringSetAsync(chunkKey, "1", TimeSpan.FromMinutes(30));

        await Database.StringSetAsync(chunkKey, data, TimeSpan.FromMinutes(30));

        await Database.ListRightPushAsync(chunkListKey, chunkIndex);
        await Database.KeyExpireAsync(chunkListKey, TimeSpan.FromMinutes(30));

        return fileId;
    }

    public async Task<byte[]> GetChunkAsync(string fileId, long chunkIndex)
    {
        var chunkKey = GetChunkKey(fileId, chunkIndex);

        var result = await Database.StringGetAsync(chunkKey);
        if (!result.HasValue)
            throw new Exception("Chunk not found");

        return result!;
    }

    public async Task<List<long>> GetUploadedChunksAsync(string fileId)
    {
        var chunkListKey = GetChunkListKey(fileId);

        var result = await Database.ListRangeAsync(chunkListKey);
        return result.Select(x => (long)x).ToList();
    }

    public async Task DeleteChunksAsync(string fileId)
    {
        var chunkListKey = GetChunkListKey(fileId);
        var chunks = await Database.ListRangeAsync(chunkListKey);
        foreach (var chunkIndex in chunks)
        {
            var chunkKey = GetChunkKey(fileId, (long)chunkIndex);
            await Database.KeyDeleteAsync(chunkKey);
        }

        await Database.KeyDeleteAsync(chunkListKey);
    }
}