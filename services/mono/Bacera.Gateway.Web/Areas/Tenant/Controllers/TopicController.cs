using Bacera.Gateway.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Bacera.Gateway.Topic;

[Tags("Tenant/Topic")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class TopicController(ITopicService topicSvc) : TenantBaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria = null)
        => Ok(await topicSvc.QueryAsync(criteria ?? new M.Criteria()));
}
