using RestSharp;

namespace Bacera.Gateway.Web.Tests.RestSharp;
[Trait(TraitTypes.Types, TraitTypes.Value.RestSharp)]
public class UserTests : IClassFixture<RestSharpStartup>
{
    private readonly RestSharpStartup _startup;

    public UserTests(RestSharpStartup startup)
    {
        _startup = startup;
    }

    [Fact]
    public async Task GetToken_ForClient_ShouldBeSuccess()
    {
        await _startup.GetTenantToken();
        var request = new RestRequest("api/v1/user/me");
        var me = _startup.Client.Get(request);
        me.IsSuccessStatusCode.ShouldBeTrue();
        me.ShouldNotBeNull();
    }
}