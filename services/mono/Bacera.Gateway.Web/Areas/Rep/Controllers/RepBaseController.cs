
﻿using Microsoft.AspNetCore.Mvc;
using Bacera.Gateway.Web.Controllers;
using Bacera.Gateway.Web.Middleware;
using Bacera.Gateway.Web.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Bacera.Gateway.Web.Areas.Rep.Controllers;

[ApiController]
[Area("Rep")]
[Produces("application/json")]
[Route("api/" + VersionTypes.V1 + "/[Area]/{repUid:long}/[controller]")]
[ServiceFilter(typeof(RepAreaFilter))]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.Rep)]
public class RepBaseController : BaseController
{
    
}