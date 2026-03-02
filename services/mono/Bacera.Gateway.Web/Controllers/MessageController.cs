using Bacera.Gateway.Context;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Bacera.Gateway.Web.Response;
using MaxMind.GeoIP2.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Controllers;

using M = ContactRequest;

[AllowAnonymous]
[Tags("Public/Message")]
public class MessageController() : BaseController
{
    // [HttpGet("ws-key")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // public async Task<IActionResult> GetWebSocketKey([FromBody] M.CreateSpec spec)
    // {
    // }
}