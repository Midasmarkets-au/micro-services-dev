using Dapper;

namespace Bacera.Gateway.Connection;

public class CentralDbConnection : IDbConnection
{
    private readonly System.Data.IDbConnection _connection;

    public CentralDbConnection(System.Data.IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<List<T>> ToListAsync<T>(string sql, object? param = null)
        => (await _connection.QueryAsync<T>(sql, param)).ToList();

    public Task<int> ExecuteAsync(string sql, object? param = null)
        => _connection.ExecuteAsync(sql, param);

    public Task<T> FirstAsync<T>(string sql, object? param = null)
        => _connection.QueryFirstAsync<T>(sql, param);

    public Task<T?> FirstOrDefaultAsync<T>(string sql, object? param = null)
        => _connection.QueryFirstOrDefaultAsync<T>(sql, param);

    public Task<T> SingleAsync<T>(string sql, object? param = null)
        => _connection.QuerySingleAsync<T>(sql, param);

    public Task<T?> SingleOrDefaultAsync<T>(string sql, object? param = null)
        => _connection.QuerySingleOrDefaultAsync<T>(sql, param);
}