using Microsoft.AspNetCore.Mvc;
using Bacera.Gateway.Web.Controllers.V2;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.V2;

[ApiController]
[Area("Client")]
[Produces("application/json")]
[Route("api/" + VersionTypes.V2 + "/[Area]/[controller]")]
public class ClientBaseControllerV2 : BaseControllerV2
{
}