using OpenIddict.Validation.AspNetCore;
﻿using Microsoft.AspNetCore.Mvc;
using Bacera.Gateway.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Bacera.Gateway.Web.Areas.Admin.Controllers;

[ApiController]
[Area("Admin")]
[Produces("application/json")]
[Route("api/" + VersionTypes.V1 + "/[Area]/[controller]")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class AdminBaseController : BaseController
{
}