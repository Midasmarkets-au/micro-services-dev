using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services;

public class GptService(IOptions<GptServiceOptions> options, IHttpClientFactory clientFactory)
{
    private const string Endpoint = "https://api.openai.com/v1/completions";
    private const int Timeout = 30;

    public Task<(bool, string)> TranslateTextToTargetAsync(string targetLang, string text)
        => CompleteAsync($"Translate to {targetLang}: {text}");

    public Task<(bool, string)> TranslateTextFromSourceToTargetAsync(string sourceLang, string targetLang, string text)
        => CompleteAsync($"Translate from {sourceLang} to {targetLang}: {text}");

    private async Task<(bool, string)> CompleteAsync(string prompt)
    {
        var request = new
        {
            prompt,
            model = "gpt-3.5-turbo-instruct",
            max_tokens = 1000,
        };
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var client = CreateClient(options.Value.ApiKey);
        var response = await client.PostAsync("", content);
        if (!response.IsSuccessStatusCode) return (false, "__GPT_REQUEST_FAILED__");
        var result = await response.Content.ReadAsStringAsync();
        var resultObject = JsonConvert.DeserializeObject<dynamic>(result);
        try
        {
            return (true, resultObject!.choices[0].text.ToString().Trim());
        }
        catch
        {
            return (false, "__GPT_RESPONSE_FAILED__");
        }
    }

    private HttpClient CreateClient(string apiKey)
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        var client = clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(Timeout);
        client.BaseAddress = new Uri(Endpoint);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        return client;
    }
}