using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhatsAppClone.Api.Interface;
using WhatsAppClone.Api.Managers;
using WhatsAppClone.Api.Models;

namespace WhatsAppClone.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/users")]
public class UsersController(IUserManager users) : ControllerBase
{
    private long UserId => long.Parse(User.FindFirst("uid")!.Value);

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var r = await users.GetMeAsync(UserId);
        return r.Success ? Ok(r) : NotFound(r);
    }

    [HttpPut("me")]
    public async Task<IActionResult> Update(UpdateProfileRequest req)
    {
        var r = await users.UpdateProfileAsync(UserId, req);
        return r.Success ? Ok(r) : BadRequest(r);
    }
}
