using System.Data;

using MySqlConnector;

namespace Bacera.Gateway.TradingData;

public class MetaTradeDataConverter : IDisposable
{
    private readonly DateTime _unixEpoch;
    private readonly MySqlConnection _connection;
    private readonly int _commandTimeoutInSeconds;

    public MetaTradeDataConverter(string connectionString)
    {
        _commandTimeoutInSeconds = 30 * 60;
        _connection = new MySqlConnection(connectionString);
        _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    // public async Task<List<TradeTransaction>> GetTransactionsAsync(TradeAccount account, DateTime toDateTime,
    //     int page = 0, int pageSize = 500,
    //     string tableName = "MT4_TRADES")
    // {
    //     var result = new List<TradeTransaction>();
    //     var sql = buildQuery(account, toDateTime, page, pageSize, tableName);
    //     await using var command = new MySqlCommand(sql, _connection);
    //     await _connection.OpenAsync();
    //     command.CommandTimeout = _commandTimeoutInSeconds;
    //     await using (var reader = await command.ExecuteReaderAsync())
    //     {
    //         while (await reader.ReadAsync())
    //         {
    //             var item = readRow(reader);
    //             item.TradeAccountId = account.Id;
    //             item.ServiceId = account.ServiceId;
    //             result.Add(item);
    //         }
    //     }

    //     await _connection.CloseAsync();

    //     return result;
    // }

    // public async Task<List<TradeTransaction>> GetTransactionsByRangeAsync(DateTime from, DateTime to,
    //     int page = 0, int pageSize = 500,
    //     string tableName = "MT4_TRADES")
    // {
    //     var result = new List<TradeTransaction>();
    //     var sql = buildQueryForRange(from, to, page, pageSize, tableName);
    //     await using var command = new MySqlCommand(sql, _connection);
    //     command.CommandTimeout = _commandTimeoutInSeconds;
    //     await _connection.OpenAsync();
    //     await using (var reader = await command.ExecuteReaderAsync())
    //     {
    //         while (await reader.ReadAsync())
    //         {
    //             var item = readRow(reader);
    //             item.TradeAccountId = 0;
    //             item.ServiceId = 0;
    //             result.Add(item);
    //         }
    //     }

    //     await _connection.CloseAsync();

    //     return result;
    // }

    public async Task<int> CountTransactionByRangeAsync(DateTime from, DateTime to, string tableName = "MT4_TRADES")
    {
        var count = 0;
        var sql = buildCountQueryForRange(from, to, tableName);
        await using var command = new MySqlCommand(sql, _connection);
        command.CommandTimeout = _commandTimeoutInSeconds;
        await _connection.OpenAsync();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (!await reader.ReadAsync())
            {
                await _connection.CloseAsync();
                return count;
            }

            count = reader.GetInt32(0);
        }

        await _connection.CloseAsync();

        return count;
    }

    public async Task<TradeAccountStatus?> GetStatusAsync(long login, string tableName = "MT4_USERS")
    {
        var sql = "SELECT "
                  + " `LEVERAGE`, "
                  + " `AGENT_ACCOUNT`, "
                  + " `BALANCE`, "
                  + " `PREVMONTHBALANCE`, "
                  + " `PREVBALANCE`, "
                  + " `CREDIT`, "
                  + " `INTERESTRATE`, "
                  + " `TAXES`, "
                  + " `EQUITY`, "
                  + " `MARGIN`, "
                  + " `MARGIN_LEVEL`, "
                  + " `MARGIN_FREE`, "
                  + " `CURRENCY`, "
                  + " `MODIFY_TIME`, "
                  + " `REGDATE`, "
                  + " `GROUP` "
                  + " FROM `" + tableName + "` "
                  + $" WHERE `LOGIN` = {login} LIMIT 1 ";
        TradeAccountStatus? status;
        await using var command = new MySqlCommand(sql, _connection);
        command.CommandTimeout = _commandTimeoutInSeconds;
        await _connection.OpenAsync();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            await reader.ReadAsync();
            if (!reader.HasRows)
            {
                await _connection.CloseAsync();
                return null;
            }
            status = new TradeAccountStatus
            {
                Leverage = reader.GetInt32(0),
                AgentAccount = reader.GetInt32(1),
                Balance = reader.GetDouble(2),
                PrevMonthBalance = reader.GetDouble(3),
                PrevBalance = reader.GetDouble(4),
                Credit = reader.GetDouble(5),
                InterestRate = reader.GetDouble(6),
                Taxes = reader.GetDouble(7),
                Equity = reader.GetDouble(8),
                Margin = reader.GetDouble(9),
                MarginLevel = reader.GetDouble(10),
                MarginFree = reader.GetDouble(11),
                Currency = reader.GetString(12),
                ModifiedOn = DateTime.SpecifyKind(reader.GetDateTime(13), DateTimeKind.Utc),
                CreatedOn = DateTime.SpecifyKind(reader.GetDateTime(14), DateTimeKind.Utc),
                Group = reader.GetString(15),
                ReadOnlyCode = "",
                UpdatedOn = DateTime.UtcNow,
            };
        }

        await _connection.CloseAsync();
        return status;
    }

    private static string buildQuery(TradeAccount account, DateTime toDateTime, int page, int pageSize,
        string tableName = "MT4_TRADES")
    {
        var sql = getSelectQuery(tableName);
        sql += $" AND `LOGIN`= {account.AccountNumber}";
        if (account.LastSyncedOn.HasValue)
        {
            sql += $" AND `MODIFY_TIME` >= '{account.LastSyncedOn:yyyy-MM-dd HH:mm:ss}'";
            sql += $" AND `MODIFY_TIME` <= '{toDateTime:yyyy-MM-dd HH:mm:ss}'";
        }

        sql += " ORDER BY `TIMESTAMP` ASC";
        sql += $" LIMIT {pageSize} OFFSET {page * pageSize}";
        return sql;
    }

    private string buildQueryForRange(DateTime from, DateTime to, int page, int pageSize,
        string tableName = "MT4_TRADES")
    {
        var sql = getSelectQuery(tableName);
        sql += $" AND `TIMESTAMP` >= {getTimeSpan(from)}";
        sql += $" AND `TIMESTAMP` <= {getTimeSpan(to)}";

        sql += " ORDER BY `TIMESTAMP` ASC";
        sql += $" LIMIT {pageSize} OFFSET {page * pageSize}";
        return sql;
    }

    private string buildCountQueryForRange(DateTime from, DateTime to, string tableName = "MT4_TRADES")
    {
        var sql = "SELECT COUNT(*) AS cnt";
        sql += " FROM `" + tableName + "` ";
        sql += " WHERE 1=1";
        sql += $" AND `TIMESTAMP` >= {getTimeSpan(from)}";
        sql += $" AND `TIMESTAMP` <= {getTimeSpan(to)}";
        return sql;
    }


    private static string getSelectQuery(string tableName = "MT4_TRADES")
        => "SELECT "
           + "`TICKET`" // 0
           + ",`LOGIN`" // 1
           + ",`SYMBOL`" // 2
           + ",`DIGITS`" // 3
           + ",`CMD`" // 4
           + ",`VOLUME`" // 5
           + ",`OPEN_TIME`" // 6
           + ",`OPEN_PRICE`" // 7
           + ",`SL`" // 8
           + ",`TP`" // 9
           + ",`CLOSE_TIME`" // 10
           + ",`EXPIRATION`" // 11
           + ",`REASON`" // 12
           + ",`CONV_RATE1`" // 13
           + ",`CONV_RATE2`" // 14
           + ",`COMMISSION`" // 15
           + ",`COMMISSION_AGENT`" // 16
           + ",`SWAPS`" // 17
           + ",`CLOSE_PRICE`" // 18
           + ",`PROFIT`" // 19
           + ",`TAXES`" // 20
           + ",`COMMENT`" // 21
           + ",`INTERNAL_ID`" // 22
           + ",`MARGIN_RATE`" // 23
           + ",`TIMESTAMP`" // 24
           + ",`MAGIC`" // 25
           + ",`GW_VOLUME`" // 26
           + ",`GW_OPEN_PRICE`" // 27
           + ",`GW_CLOSE_PRICE`" // 28
           + ",`MODIFY_TIME`" // 29
           + " FROM `" + tableName + "` "
           + " WHERE 1=1 "
    // + " ORDER BY `TIMESTAMP` ASC"
    ;

    private long getTimeSpan(DateTime toDateTime)
        => (long)(toDateTime - _unixEpoch).TotalSeconds;

    // private static TradeTransaction readRow(IDataRecord reader)
    //     => new()
    //     {
    //         Ticket = reader.GetInt64(0),
    //         AccountNumber = reader.GetInt64(1),
    //         Symbol = reader.GetString(2),
    //         Digits = reader.GetInt32(3),
    //         Cmd = reader.GetInt32(4),
    //         Volume = reader.GetInt32(5),
    //         OpenAt = DateTime.SpecifyKind(reader.GetDateTime(6), DateTimeKind.Utc),
    //         OpenPrice = reader.GetDouble(7),
    //         Sl = reader.GetDouble(8),
    //         Tp = reader.GetDouble(9),
    //         CloseAt = DateTime.SpecifyKind(reader.GetDateTime(10), DateTimeKind.Utc),
    //         ExpiresAt = DateTime.SpecifyKind(reader.GetDateTime(11), DateTimeKind.Utc),
    //         Reason = reader.GetInt32(12),
    //         ConvertRate = reader.GetDouble(13),
    //         ConvertRate2 = reader.GetDouble(14),
    //         Commission = reader.GetDouble(15),
    //         CommissionAgent = reader.GetDouble(16),
    //         Swaps = reader.GetDouble(17),
    //         ClosePrice = reader.GetDouble(18),
    //         Profit = reader.GetDouble(19),
    //         Taxes = reader.GetDouble(20),
    //         Comment = reader.GetString(21),
    //         // InternalId = reader.GetInt32(22),
    //         MarginRate = reader.GetDouble(23),
    //         TimeStamp = reader.GetInt32(24),
    //         ModifiedAt = DateTime.SpecifyKind(reader.GetDateTime(29), DateTimeKind.Utc),
    //         Status = 0,
    //     };

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}