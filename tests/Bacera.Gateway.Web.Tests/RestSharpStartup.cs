using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Bacera.Gateway.Web.Tests;

public class RestSharpStartup
{
    private string _token = string.Empty;
    private readonly HttpClient _client;
    public readonly RestClient Client;
    private readonly string _username;
    private readonly string _password;

    public RestSharpStartup()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();
        var baseUrl = configuration["RestSharp:BaseUrl"] ?? string.Empty;
        _username = configuration["RestSharp:Username"] ?? string.Empty;
        _password = configuration["RestSharp:Password"] ?? string.Empty;
        _client = new HttpClient
        {
            BaseAddress = baseUrl.EndsWith("/") ? new Uri(baseUrl) : new Uri($"{baseUrl}/")
        };
        Client = new RestClient(options: new RestClientOptions
            { BaseUrl = baseUrl.EndsWith("/") ? new Uri(baseUrl) : new Uri($"{baseUrl}/") });
    }

    public async Task<string> GetTenantToken()
    {
        if (!string.IsNullOrEmpty(_token)) return _token;
        // Define the token endpoint and client credentials (replace these with your values)
        // var tokenEndpoint = "/connect/token";
        var clientId = "api";
        var apiScope = "api";

        // Use the client to create a discovery document
        // This will give us the endpoint to request the token
        var discoveryDocument = await _client.GetDiscoveryDocumentAsync();
        discoveryDocument.IsError.ShouldBeFalse();

        // Request a token using the client credentials flow
        var tokenResponse = await _client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = discoveryDocument.TokenEndpoint,
            ClientId = clientId,
            Scope = apiScope,
            Password = _password,
            UserName = _username,
        });

        tokenResponse.ShouldNotBeNull();
        tokenResponse.IsError.ShouldBeFalse();
        tokenResponse.AccessToken.ShouldNotBeNullOrEmpty();
        _token = tokenResponse.AccessToken;

        _client.SetBearerToken(_token);
        Client.AddDefaultHeader("Authorization", $"Bearer {_token}");
        return _token;
    }
}