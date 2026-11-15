using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhatsAppClone.Api.Interface;
using WhatsAppClone.Api.Models;

namespace WhatsAppClone.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/contacts")]
public class ContactsController(IContactManager contacts) : ControllerBase
{
    private long UserId => long.Parse(User.FindFirst("uid")!.Value);

    [HttpPost("sync")]
    public async Task<IActionResult> Sync(ContactSyncRequest req)
    {
        var r = await contacts.SyncAsync(UserId, req);
        return r.Success ? Ok(r) : BadRequest(r);
    }
}