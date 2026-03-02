using Bacera.Gateway.Web.Controllers.V2;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers.V2;

[ApiController]
[Area("Tenant")]
[Produces("application/json")]
[Route("api/" + VersionTypes.V2 + "/[Area]/[controller]")]
public class TenantBaseControllerV2 : BaseControllerV2
{
}