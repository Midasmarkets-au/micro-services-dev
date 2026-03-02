using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Controllers;

using M = Topic;

[Tags("Public/Content")]
public class ContentController : BaseController
{
    private readonly ITopicService _topicService;

    public ContentController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    /// <summary>
    /// Get content by title
    /// </summary>
    /// <param name="title"></param>
    /// <param name="language" example="en-us"></param>
    /// <returns></returns>
    [HttpGet("{title}")]
    [ProducesResponseType(typeof(M), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContent(string title, [FromQuery] string? language = null)
    {
        var result = await _topicService.GetContentAsync(title, language);
        return result.IsEmpty() ? NotFound() : Ok(result);
    }
}