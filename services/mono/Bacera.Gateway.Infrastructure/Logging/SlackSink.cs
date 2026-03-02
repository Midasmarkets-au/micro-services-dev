using System.Text;
using Newtonsoft.Json;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Bacera.Gateway.Web.Services;

public class SlackSink(string webhookUrl) : ILogEventSink
{
    private readonly HttpClient _client = new();

    public void Emit(LogEvent logEvent) => Task.Run(() => SendAsync(logEvent.RenderMessage()));

    private async Task SendAsync(string message)
    {
        var ss = $"[{DateTime.UtcNow}]: {message}";
        var body = new { text = ss };
        var json = JsonConvert.SerializeObject(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await _client.PostAsync(webhookUrl, content);
    }

    public void Dispose() => _client.Dispose();
}

public static class SlackSinkExtensions
{
    public static void ToSlack(this LoggerSinkConfiguration loggerConfiguration, string webhookUrl)
        => loggerConfiguration.Sink(new SlackSink(webhookUrl));
}