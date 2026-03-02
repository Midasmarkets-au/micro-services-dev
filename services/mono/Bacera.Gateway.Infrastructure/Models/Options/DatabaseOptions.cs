namespace Bacera.Gateway
{
    public abstract class DatabaseOptions
    {
        public string Username { get; init; } = null!;
        public string Password { get; init; } = null!;
        public string Host { get; init; } = null!;
        public int Port { get; init; } = 5432;
        public string Database { get; set; } = string.Empty;

        public bool? ConvertZeroDateTime { get; set; }

        // Virtual properties that can be overridden by specific database types
        protected virtual int MaxPoolSize => 100;
        protected virtual int MinPoolSize => 10;
        protected virtual int ConnectionIdleLifetime => 120;
        protected virtual int ConnectionLifetime => 600;
        protected virtual int CommandTimeout => 30;
        protected virtual int ConnectionTimeout => 15;
        protected virtual bool EnablePooling => true;

        // Database-specific connection string builders
        protected virtual string BuildPostgreSqlConnectionString()
            => $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};Maximum Pool Size={MaxPoolSize};Minimum Pool Size={MinPoolSize};Pooling={EnablePooling.ToString().ToLower()};Connection Idle Lifetime={ConnectionIdleLifetime};Connection Lifetime={ConnectionLifetime};Command Timeout={CommandTimeout};Timeout={ConnectionTimeout}";

        protected virtual string BuildMySqlConnectionString()
            => $"Server={Host};Port={Port};Database={Database};Uid={Username};Pwd={Password};Max Pool Size={MaxPoolSize};Min Pool Size={MinPoolSize};Pooling={EnablePooling.ToString().ToLower()};Connection Idle Timeout={ConnectionIdleLifetime}";

        public virtual string ConnectionString
        {
            get
            {
                var connectionString = GetBaseConnectionString();

                if (ConvertZeroDateTime == true)
                {
                    connectionString += ";ConvertZeroDateTime=True";
                }

                return connectionString;
            }
        }

        protected virtual string GetBaseConnectionString()
            => $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password}";

        public virtual string GetConnectionString()
            => ConnectionString;

        public bool IsValidated()
            => !string.IsNullOrEmpty(Username)
               && !string.IsNullOrEmpty(Password)
               && !string.IsNullOrEmpty(Host)
               && !string.IsNullOrEmpty(Database);
    }

    public class CentralDatabaseOptions : DatabaseOptions
    {
        protected override string GetBaseConnectionString() => BuildPostgreSqlConnectionString();

        public static CentralDatabaseOptions Create(string host, string database, string username, string password,
            int port = 5432)
            => new()
            {
                Host = host,
                Port = port,
                Database = database,
                Username = username,
                Password = password,
            };
    }

    public class WebsiteDatabaseOptions : DatabaseOptions
    {
        // Override MySQL-specific settings
        protected override int MaxPoolSize => 200;

        protected override string GetBaseConnectionString() => BuildMySqlConnectionString();

        public static WebsiteDatabaseOptions Create(string host, string database, string username, string password,
            int port = 3306)
            => new()
            {
                Host = host,
                Port = port,
                Database = database,
                Username = username,
                Password = password,
            };
    }

    public class MybcrDatabaseOptions : DatabaseOptions
    {
        // Override MySQL-specific settings
        protected override int MaxPoolSize => 200;

        protected override string GetBaseConnectionString() => BuildMySqlConnectionString();

        public static MybcrDatabaseOptions Create(string host, string database, string username, string password,
            int port = 3306)
            => new()
            {
                Host = host,
                Port = port,
                Database = database,
                Username = username,
                Password = password,
            };
    }

    public class TenantDatabaseOptions : DatabaseOptions
    {
        protected override string GetBaseConnectionString() => BuildPostgreSqlConnectionString();

        public TenantDatabaseOptions SetDatabase(string database)
        {
            Database = database;
            return this;
        }

        public static TenantDatabaseOptions Create(string host, string database, string username, string password,
            int port = 5432)
            => new()
            {
                Host = host,
                Port = port,
                Database = database,
                Username = username,
                Password = password,
            };
    }

    public class HangfireDatabaseOptions : DatabaseOptions
    {
        protected override string GetBaseConnectionString() => BuildPostgreSqlConnectionString();

        public static HangfireDatabaseOptions Create(string host, string database, string username, string password,
            int port = 5432)
            => new()
            {
                Host = host,
                Port = port,
                Database = database,
                Username = username,
                Password = password,
            };
    }

    public class DatabaseWithTablesOptions : DatabaseOptions
    {
        public string UserTableName { get; init; } = string.Empty;
        public string TradeTableName { get; init; } = string.Empty;
    }
}