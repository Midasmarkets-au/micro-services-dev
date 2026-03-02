namespace Bacera.Gateway;

public class ApiOptions
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }

    public string User { get; set; } = null!;
    public string Password { get; set; } = null!;

    public static ApiOptions Create(string host, int port, string? user = "", string? password = "")
        => new() { Host = host, Port = port, User = user ?? string.Empty, Password = password ?? string.Empty };

    public bool HasUserNameAndPassword() => !string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Password);
}