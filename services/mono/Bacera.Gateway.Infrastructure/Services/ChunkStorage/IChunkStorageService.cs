namespace Bacera.Gateway.Services.ChunkStorage;

public interface IChunkStorageService
{
    Task<string> SaveChunkAsync(string fileId, long chunkIndex, byte[] data);
    Task<byte[]> GetChunkAsync(string fileId, long chunkIndex);
    Task<List<long>> GetUploadedChunksAsync(string fileId);
    Task DeleteChunksAsync(string fileId);
}