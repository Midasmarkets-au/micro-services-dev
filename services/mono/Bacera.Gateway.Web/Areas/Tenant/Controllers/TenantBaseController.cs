using Microsoft.AspNetCore.Mvc;
using Bacera.Gateway.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[ApiController]
[Area("Tenant")]
[Produces("application/json")]
[Route("api/" + VersionTypes.V1 + "/[Area]/[controller]")]
public class TenantBaseController : BaseController
{

}