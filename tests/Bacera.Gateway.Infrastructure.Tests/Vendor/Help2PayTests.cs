using Bacera.Gateway.Vendor.Help2Pay.Models;
using Bogus;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests.Vendor;

public class Help2PayTests
{
    private readonly Faker _faker;
    private readonly HttpClient _client;
    private const string DefaultMerchantCode = "INT0368";
    private const string SecurityCode = "sQ5B5X7dgfXQy6UYUGJA";
    private const string EndPoint = "https://api.testingzone88.com/MerchantTransfer";

    public Help2PayTests()
    {
        _faker = new Faker();
        _client = new HttpClient();
    }

    [Fact]
    public async Task CreateTest()
    {
    
    }
}