using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Msg.Controllers;

[Authorize]
public class ChatController(TenantDbContext tenantCtx) : BaseController
{
    [HttpGet("/ping")]
    public IActionResult Ping() => Ok("pong");

    [HttpGet]
    public async Task<IActionResult> GetChatList()
    {
        var partyId = GetPartyId();
        var items = await tenantCtx.Chats
            .Where(x => x.Participants.Any(p => p.PartyId == partyId))
            .Where(x => x.Status == "active")
            .OrderByDescending(x => x.Id)
            .Take(20)
            .ToClientPageModel()
            .ToListAsync();
        
        return Ok(items);
    }

    [HttpGet("{hashId}")]
    public async Task<IActionResult> GetChat(string hashId)
    {
        var chatId = Chat.HashDecode(hashId);
        var partyId = GetPartyId();
        var item = await tenantCtx.Chats
            .Where(x => x.Id == chatId && x.Participants.Any(p => p.PartyId == partyId))
            .ToClientDetailModel()
            .FirstOrDefaultAsync();

        return Ok(item);
    }
}