using OpenIddict.Validation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Topic;

[Tags("Tenant/Topic")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class TopicController : TenantBaseController
{
    private readonly ITopicService _svc;

    public TopicController(
         ITopicService topicService
    )
    {
        _svc = topicService;
    }

    /// <summary>
    /// Topic pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<M.ResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
        => Ok(await _svc.QueryAsync(criteria ?? new M.Criteria()));

    /// <summary>
    /// Get topic
    /// </summary>
    /// <param name="id"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Details(int id, [FromQuery] string? language = null)
    {
        M item;
        if (!string.IsNullOrEmpty(language) && LanguageTypes.IsExists(language))
        {
            item = await _svc.GetWithLanguageAsync(id, language);
        }
        else
        {
            item = await _svc.GetAsync(id);
        }

        return item.Id == 0 ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Get languages of a topic
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>

    [HttpGet("{id:int}/languages")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Languages(int id)
    {
        if (await _svc.ExistsAsync(id) == false) return NotFound();
        var result = await _svc.GetLanguagesAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Create topic
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] M.CreateSpec spec)
    {
        var item = await _svc.CreateAsync(spec);
        if (item.Id == 0) return BadRequest();
        return Ok(item);
    }

    /// <summary>
    /// Update topic
    /// </summary>
    /// <param name="id" example="10"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] M.UpdateSpec spec)
    {
        var item = await _svc.UpdateAsync(id, spec);
        return item.Id == 0 ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Add a content to a topic
    /// </summary>
    /// <param name="id" example="10"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/content")]
    [ProducesResponseType(typeof(TopicContent), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateContent(int id, [FromBody] TopicContent.Spec spec)
    {
        var item = await _svc.CreateContentAsync(id, spec);
        return item.Id == 0 ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Move to trash
    /// </summary>
    /// <param name="id" example="10"></param>
    /// <returns></returns>
    [HttpPut("{id:int}/move-to-trash")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MoveToTrash(int id)
    {
        await _svc.MoveToTrash(id);
        return NoContent();
    }

    /// <summary>
    /// Update a content of a topic
    /// </summary>
    /// <param name="id"></param>
    /// <param name="contentId"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    [HttpPut("{id:int}/content/{contentId:int}")]
    [ProducesResponseType(typeof(TopicContent), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateContent(int id, int contentId, [FromBody] TopicContent.Spec spec)
    {
        var item = await _svc.UpdateContentAsync(contentId, spec);
        return item.Id == 0 ? NotFound() : Ok(item);
    }

    /// <summary>
    /// Delete topic
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _svc.ExistsAsync(id) == false) return NotFound();
        await _svc.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Delete a content of a topic
    /// </summary>
    /// <param name="id"></param>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpDelete("{id:int}/content/{contentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContent(int id, int contentId)
    {
        if (await _svc.ExistsAsync(id) == false) return NotFound();
        await _svc.DeleteContentAsync(contentId);
        return NoContent();
    }
}