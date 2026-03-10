using Api.V1;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Controllers;

/// <summary>
/// 消息广播接口 — 通过 gRPC 调用 Boardcast 服务向指定频道推送消息。
/// </summary>
public class BoardcastController : BaseController
{
    private readonly BoardcastService.BoardcastServiceClient _boardcast;

    public BoardcastController(BoardcastService.BoardcastServiceClient boardcast)
    {
        _boardcast = boardcast;
    }

    /// <summary>
    /// 向指定频道发布一条广播消息。
    /// </summary>
    /// <remarks>
    /// 消息通过 gRPC 发送到 Boardcast 服务，订阅该频道的 SSE 客户端将实时收到推送。
    /// </remarks>
    [HttpPost("publish")]
    [ProducesResponseType(typeof(PublishResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Publish([FromBody] PublishRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Channel))
            return BadRequest(new { error = "channel must not be empty" });

        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "message must not be empty" });

        var reply = await _boardcast.PublishAsync(new PublishRequest
        {
            Channel = request.Channel,
            Message = request.Message,
        });

        return Ok(new PublishResultDto { Ok = reply.Ok });
    }
}

/// <summary>发布消息请求体</summary>
public record PublishRequestDto
{
    /// <summary>目标频道名称</summary>
    public string Channel { get; init; } = string.Empty;

    /// <summary>消息内容</summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>发布消息响应</summary>
public record PublishResultDto
{
    /// <summary>是否发布成功</summary>
    public bool Ok { get; init; }
}
