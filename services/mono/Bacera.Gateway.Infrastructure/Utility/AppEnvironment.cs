namespace Bacera.Gateway;

public static class AppEnvironment
{
    public static string GetEnvironment()
        => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
           ?? Environment.GetEnvironmentVariable("APP_ENV")
           ?? "unknown";

    public static bool IsProduction()
        => GetEnvironment().Equals("Production", StringComparison.OrdinalIgnoreCase)
           || GetEnvironment().Equals("production", StringComparison.OrdinalIgnoreCase);

    public static bool IsDevelopment()
        => GetEnvironment().Equals("Development", StringComparison.OrdinalIgnoreCase)
           || GetEnvironment().Equals("dev", StringComparison.OrdinalIgnoreCase);
}