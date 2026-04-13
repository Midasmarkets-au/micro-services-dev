
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers.V2;

[Tags("Client/Topic")]
[Area("Client")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
[Route("api/" + VersionTypes.V2 + "/[Area]/topic")]

public class TopicControllerV2(TenantDbContext tenantCtx): ClientBaseControllerV2
{
    /// <summary>
    /// Notice pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("notice")]
    public async Task<IActionResult> NoticeQuery([FromQuery] Topic.ClientCriteria? criteria = null)
    {
        criteria ??= new Topic.ClientCriteria();

        criteria.Language = criteria.Language switch
        {
            LanguageTypes.Chinese => LanguageTypes.Chinese,
            LanguageTypes.TraditionalChineseTaiWan => LanguageTypes.TraditionalChineseTaiWan,
            LanguageTypes.TraditionalChineseHongKong => LanguageTypes.TraditionalChineseTaiWan,
            _ => LanguageTypes.English
        };

        criteria.Type = TopicTypes.Notice;
        criteria.IsEffective = true;
        
        var items = await tenantCtx.Topics
            .PagedFilterBy<Topic, int>(criteria)
            .ToClientPageModel(criteria.Language)
            .ToListAsync();

        return Ok(Result<List<Topic.ClientPageModel>, Topic.ClientCriteria>.Of(items, criteria));
    }
}