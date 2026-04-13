
﻿using Microsoft.AspNetCore.Mvc;
using Bacera.Gateway.Web.Controllers;
using Bacera.Gateway.Web.Middleware;
using Bacera.Gateway.Web.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Bacera.Gateway.Web.Areas.Agent.Controllers;

[ApiController]
[Area("IB")]
[Produces("application/json")]
[Route("api/" + VersionTypes.V1 + "/[Area]/{agentUid:long}/[controller]")]
[ServiceFilter(typeof(AgentAreaFilter))]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.IB)]
public class AgentBaseController : BaseController
{
    
}