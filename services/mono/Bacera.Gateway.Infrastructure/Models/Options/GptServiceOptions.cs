namespace Bacera.Gateway;

public class GptServiceOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public static GptServiceOptions Build(string apiKey) => new() { ApiKey = apiKey };
}