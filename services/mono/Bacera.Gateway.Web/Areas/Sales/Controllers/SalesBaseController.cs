
﻿using Microsoft.AspNetCore.Mvc;
using Bacera.Gateway.Web.Controllers;
using Bacera.Gateway.Web.Middleware;
using Bacera.Gateway.Web.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

[ApiController]
[Area("Sales")]
[Produces("application/json")]
[Route("api/" + VersionTypes.V1 + "/[Area]/{salesUid:long}/[controller]")]
[ServiceFilter(typeof(SalesAreaFilter))]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class SalesBaseController : BaseController
{
}