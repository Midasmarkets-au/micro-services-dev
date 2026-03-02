using Bacera.Gateway.ViewModels.Tenant;
using Newtonsoft.Json;
using RestSharp;

namespace Bacera.Gateway.Web.Tests.RestSharp.Areas.Tenant;

using M = Account;

[Trait(TraitTypes.Types, TraitTypes.Value.RestSharp)]
public class AccountTests : IClassFixture<RestSharpStartup>
{
    private const string UriPrefix = "api/v1/tenant/account";
    private readonly RestSharpStartup _startup;

    public AccountTests(RestSharpStartup startup)
    {
        _startup = startup;
    }

    [Fact]
    public async Task AccountPagination_ShouldBeSuccess()
    {
        await _startup.GetTenantToken();
        var request = new RestRequest(UriPrefix);
        var response = _startup.Client.Get(request);
        response.IsSuccessStatusCode.ShouldBeTrue();
        response.ShouldNotBeNull();
        response.Content.ShouldNotBeNull();
        var result = JsonConvert.DeserializeObject<Result<List<AccountViewModel>, M.Criteria>>(response.Content);
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
    }
}