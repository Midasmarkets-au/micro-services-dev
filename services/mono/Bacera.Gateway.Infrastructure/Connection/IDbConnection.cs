namespace Bacera.Gateway.Connection;

public interface IDbConnection
{
    Task<List<T>> ToListAsync<T>(string sql, object? param = null);
    Task<int> ExecuteAsync(string sql, object? param = null);
    Task<T?> FirstOrDefaultAsync<T>(string sql, object? param = null);
    Task<T> FirstAsync<T>(string sql, object? param = null);
    Task<T> SingleAsync<T>(string sql, object? param = null);
    Task<T?> SingleOrDefaultAsync<T>(string sql, object? param = null);
}