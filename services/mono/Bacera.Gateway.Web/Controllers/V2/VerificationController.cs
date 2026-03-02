using OpenIddict.Validation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Bacera.Gateway.Core.Types;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Controllers.V2;

[Tags("User Verification")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("api/" + VersionTypes.V2 + "/user/verification")]

public class VerificationControllerV2() : BaseControllerV2
{
    
}