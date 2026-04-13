using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.Controllers;

/// <summary>Bridges client → MM-Back → QR tunnel provider (<c>paid</c> notification).</summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/v1/payment")]
[Produces("application/json")]
[Tags("Client / QR Code tunnel")]
public class QrCodeTunnelPaymentController(DepositService depositSvc) : ControllerBase
{
    /// <summary>
    /// Forward “customer paid” to the QR provider. <paramref name="id"/> is the
    /// <c>transactionId</c> from the create-deposit response (<c>transactionId</c> / <c>reference</c> fields).
    /// </summary>
    [HttpPost("{id}/paid")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> NotifyPaid([FromRoute] string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(Result.Error("Transaction id required"));

        var partyId = User.GetPartyId();
        if (partyId <= 0)
            return Unauthorized();

        var result = await depositSvc.NotifyQrCodeTunnelPaidAsync(partyId, id);
        if (result.Success)
        {
            try
            {
                var parsed = JsonConvert.DeserializeObject(result.ResponseBody ?? "{}");
                return Ok(parsed ?? new { });
            }
            catch
            {
                return Ok(new { raw = result.ResponseBody });
            }
        }

        if (result.HttpStatusCode == 404)
            return NotFound(Result.Error(result.Message ?? "Notify failed"));

        var status = result.HttpStatusCode is >= 400 and <= 599
            ? result.HttpStatusCode
            : StatusCodes.Status502BadGateway;

        return StatusCode(status, Result.Error(result.Message ?? "Notify failed"));
    }
}
