using Dapper;

namespace Bacera.Gateway.Connection;

public class TenantDbConnection(System.Data.IDbConnection connection) : IDbConnection, IDisposable, IAsyncDisposable
{
    public async Task<List<T>> ToListAsync<T>(string sql, object? param = null)
        => (await connection.QueryAsync<T>(sql, param)).ToList();

    public Task<int> ExecuteAsync(string sql, object? param = null)
        => connection.ExecuteAsync(sql, param);

    public Task<T> FirstAsync<T>(string sql, object? param = null)
        => connection.QueryFirstAsync<T>(sql, param);

    public Task<T?> FirstOrDefaultAsync<T>(string sql, object? param = null)
        => connection.QueryFirstOrDefaultAsync<T>(sql, param);

    public Task<T> SingleAsync<T>(string sql, object? param = null)
        => connection.QuerySingleAsync<T>(sql, param);

    public Task<T?> SingleOrDefaultAsync<T>(string sql, object? param = null)
        => connection.QuerySingleOrDefaultAsync<T>(sql, param);

    public void Dispose()
    {
        connection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (connection is IAsyncDisposable connectionAsyncDisposable)
            await connectionAsyncDisposable.DisposeAsync();
        else
            connection.Dispose();
    }
}