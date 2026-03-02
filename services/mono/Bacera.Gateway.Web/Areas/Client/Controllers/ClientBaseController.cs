using Microsoft.AspNetCore.Mvc;
using Bacera.Gateway.Web.Controllers;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

[ApiController]
[Area("Client")]
[Produces("application/json")]
[Route("api/" + VersionTypes.V1 + "/[Area]/[controller]")]
public class ClientBaseController : BaseController
{
}