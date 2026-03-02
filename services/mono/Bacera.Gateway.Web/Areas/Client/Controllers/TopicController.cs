using OpenIddict.Validation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

using M = Topic;

[Tags("Client/Topic")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class TopicController : ClientBaseController
{
    private readonly ITopicService _topicService;

    public TopicController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    /// <summary>
    /// News pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("news")]
    [ProducesResponseType(typeof(Result<List<M.ResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> NewsQuery([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.SortField = nameof(M.Id);
        criteria.Type = TopicTypes.News;

        var result = await _topicService.QueryAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get news
    /// </summary>
    /// <param name="id"></param>
    /// <param name="language" example="en-us"></param>
    /// <returns></returns>
    [HttpGet("news/{id:int}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> News(int id, [FromQuery] string? language = null)
    {
        var result = await _topicService.GetNewsAsync(id, language);
        return result.Id == 0 ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Notice pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("notice")]
    [ProducesResponseType(typeof(Result<List<M.ResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> NoticeQuery([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.SortField = nameof(M.Id);
        criteria.Type = TopicTypes.Notice;
        criteria.IsEffective = true;

        var result = await _topicService.QueryAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get Notice
    /// </summary>
    /// <param name="id"></param>
    /// <param name="language" example="en-us"></param>
    /// <returns></returns>
    [HttpGet("notice/{id:int}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Notice(int id, [FromQuery] string? language = null)
    {
        var now = DateTime.UtcNow;
        var result = await _topicService.GetNoticeAsync(id, language);
        return result.Id == 0 ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Calendar pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("calendar")]
    [ProducesResponseType(typeof(Result<List<M.ResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CalendarQuery([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.SortField = nameof(M.Id);
        criteria.Type = TopicTypes.Calendar;
        var result = await _topicService.QueryAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Get Calendar
    /// </summary>
    /// <param name="id"></param>
    /// <param name="language" example="en-us"></param>
    /// <returns></returns>
    [HttpGet("calendar/{id:int}")]
    [ProducesResponseType(typeof(M.ResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Calendar(int id, [FromQuery] string? language = null)
    {
        var result = await _topicService.GetCalenderAsync(id, language);
        return result.Id == 0 ? NotFound() : Ok(result);
    }
}